using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Numerics;
using System.Security.Cryptography;
using System.IO;
using System.Globalization;
using System.Threading;

public class NetworkManager : MonoBehaviour
{
    TcpClient clientSocket = new TcpClient();
    public String hostname;
    public int port;


    NetworkStream stream;

    // PROTOCOL DIFFIE's HELLMAN - VARIABLES ---------
    int p, g, server_result;
    int session_id;
    long result_DH;
    byte[] key;
    // -----------------------------------------------


    void send_message_0_1(int i)
    {
        byte[] msgtosend;
        if (i == 0)
        {
            // SENDING PROTCOL "0/JD/1.1\r\nNOTNECESSERYVALUE\r\ncontent-length\r\n\r\ncontent"
            msgtosend = Encoding.UTF8.GetBytes("0/JD/1.1\r\n5\r\n\r\nHELLO"); //creating msg to send to server 

            Debug.Log("SENDING: " + Encoding.ASCII.GetString(msgtosend, 0, msgtosend.Length));
            stream.Write(msgtosend, 0, msgtosend.Length); //wysylanie wiadomosci
        }
        else if(i == 1)
        {
            // SENDING PROTCOL "1/JD/1.1\r\nsession_id\r\ncontent-length\r\n\r\ncontent"
            msgtosend = Encoding.UTF8.GetBytes("1/JD/1.1\r\n" + session_id.ToString() + "\r\n" + result_DH.ToString().Length + "\r\n\r\n" + result_DH.ToString());
            stream.Write(msgtosend, 0, msgtosend.Length);
        }
        else if(i == 5) // prośba o rolla
        {
            // SENDING PROTCOL "5/JD/1.1\r\nsession_id\r\ncontent-length\r\n\r\ncontent"
            string encrypted_msg = Encrypt("ROLL\r\n", key);
            Debug.Log("jestem w 5");
            msgtosend = Encoding.UTF8.GetBytes("5/JD/1.1\r\n" + session_id.ToString() + "\r\n" + encrypted_msg.Length + "\r\n\r\n" + encrypted_msg);
            stream.Write(msgtosend, 0, msgtosend.Length);
        }
    }
    void send_message_6(string a)
    {
        byte[] msgtosend;
        string encrypted_msg = Encrypt(a, key);
        Debug.Log("jestem w 6");
        msgtosend = Encoding.UTF8.GetBytes("6/JD/1.1\r\n" + session_id.ToString() + "\r\n" + encrypted_msg.Length + "\r\n\r\n" + encrypted_msg);
        stream.Write(msgtosend, 0, msgtosend.Length);
    }
    string receive_one_byte()
    {
        byte[] id_message = new byte[1]; //stworzenie buforu do odczytu calej wiadomosci od servera
        int id_message_Read = stream.Read(id_message, 0, 1); //
        return Encoding.ASCII.GetString(id_message, 0, id_message_Read);
    }
    string create_aes_key(long s)
    {
        string finally_aes_key = "";
        string a = s.ToString();
        for (int i = 0; i < 16; i++)
            finally_aes_key += a[i % a.Length];
        return finally_aes_key;
    }

    // Start is called before the first frame update
    void Start() // funkcja połączenia z serwerem
    {
        clientSocket.Connect(hostname, port);

        Debug.Log("--------------------------");
        Debug.Log("WELCOME TO LUDO GAME");
        
        stream = clientSocket.GetStream(); // tworzenie NetworkStream - obiekt zapewniający źródłowy strumień danych dla dostępu do sieci

        send_message_0_1(0); // wysłanie wiadomości 1. HELLO

        string id_server_message = receive_one_byte();

        if (id_server_message == "1")
        {
            string protocol = "";
            while(protocol.IndexOf("\r\n\r\n") == -1)
            {
                string data = receive_one_byte();
                protocol += data;
            }
            string[] separator = { "\r\n" };
            string[] splitted_protocol = protocol.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            int content_length = Convert.ToInt32(splitted_protocol[2]);
            int counter = 0;
            string server_message = "";
            while(counter < content_length)
            {
                server_message += receive_one_byte();
                counter++;
            }

            string[] splitted_message = server_message.Split('\n');

            for (int i = 0; i < splitted_message.Length; i++)
                Debug.Log("AA: " + splitted_message[i]);
     
            p = Convert.ToInt32(splitted_message[1]); // wyodrębnianie wartości do protokołu DH                                                   //
            g = Convert.ToInt32(splitted_message[2]);                                                                                             //
            server_result = Convert.ToInt32(splitted_message[3]);
            session_id = Convert.ToInt16(splitted_message[4]);

            GameManager.gm.My_ID = session_id; // ustanowienie ID w GameManagerze

            Debug.Log("P=" + p + " G=" + g + " SERVER_RESULT=" + server_result + "SESSION_ID=" + session_id);

            System.Random r = new System.Random();
            int secret_key = r.Next(5, 1001);
            Debug.Log("SECRET KEY: " + secret_key);

            result_DH = powerStrings(g.ToString(), secret_key.ToString(), p);
            Debug.Log("RESULT DH CLIENT: " + result_DH);

            long s = powerStrings(server_result.ToString(), secret_key.ToString(), p);

            Debug.Log("S= " + s);
            Debug.Log("-----------------------");

            string aes_key = create_aes_key(s);
            Debug.Log("FINALLY AES KEY: " + aes_key);

            key = Encoding.ASCII.GetBytes(aes_key);

            Debug.Log("-----------------------");
        }

        send_message_0_1(1); // SENDING PROTCOL "1/JD/1.1\r\nsession_id\r\ncontent-length\r\n\r\ncontent" 
        Thread thread = new Thread(StartGame);
        thread.Start();
    }

    // Update is called once per frame
    void Update()
    {
    }

    // POWER STRINGS ----------------------------------------------------------
    static long powerStrings(String sa, String sb, long MOD)
    {
        // We convert strings to number  
        long a = 0, b = 0;

        // calculating a % MOD  
        for (int i = 0; i < sa.Length; i++)
        {
            a = (a * 10 + (sa[i] - '0')) % MOD;
        }

        // calculating b % (MOD - 1)  
        for (int i = 0; i < sb.Length; i++)
        {
            b = (b * 10 + (sb[i] - '0')) % (MOD - 1);
        }

        // Now a and b are long long int. We  
        // calculate a^b using modulo exponentiation  
        return powerLL(a, b, MOD);
    }
    static long powerLL(long x, long n, long MOD)
    {
        long result = 1;
        while (n > 0)
        {
            if (n % 2 == 1)
            {
                result = result * x % MOD;
            }
            n = n / 2;
            x = x * x % MOD;
        }
        return result;
    }
    // ------------------------------------------------------------------------------------


    //DECRYPT AES -------------------------------------------------------------------------
    public static string Decrypt(string data, byte[] key)
    {
        AesCryptoServiceProvider csp = new AesCryptoServiceProvider();
        csp.KeySize = 256;
        csp.BlockSize = 128;
        csp.Key = key;
        csp.Padding = PaddingMode.PKCS7;
        csp.Mode = CipherMode.ECB;

        ICryptoTransform decryptor = csp.CreateDecryptor();
        byte[] encrypted_bytes = Convert.FromBase64String(data);
        byte[] decrypted_bytes = decryptor.TransformFinalBlock(encrypted_bytes, 0, encrypted_bytes.Length);
        string str = ASCIIEncoding.ASCII.GetString(decrypted_bytes);//ASCIIEncoding.ASCII.GetString(decrypted_bytes);
        return str;
    }
    public static string Encrypt(string data, byte[] key)
    {
        AesCryptoServiceProvider csp = new AesCryptoServiceProvider();
        csp.KeySize = 256;
        csp.BlockSize = 128;
        csp.Key = key;
        csp.Padding = PaddingMode.PKCS7;
        //csp.Mode = CipherMode.ECB;
        csp.Mode = CipherMode.ECB;

        ICryptoTransform encrypter = csp.CreateEncryptor();
        byte[] bajter = encrypter.TransformFinalBlock(ASCIIEncoding.ASCII.GetBytes(data), 0, data.Length);
        encrypter.Dispose();
        string str = Convert.ToBase64String(bajter);
        return str;
    }

    public static void PadToMultipleOf(ref byte[] src, int pad)
    {
        int len = (src.Length + pad - 1) / pad * pad;
        Array.Resize(ref src, len);
    }

    public static byte[] ConvertHexStringToByteArray(string hexString)
    {
        if (hexString.Length % 2 != 0)
        {
            throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
        }

        byte[] HexAsBytes = new byte[hexString.Length / 2];
        for (int index = 0; index < HexAsBytes.Length; index++)
        {
            string byteValue = hexString.Substring(index * 2, 2);
            HexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        return HexAsBytes;
    }

    public void StartGame()
    {   while (true)
        {
            GameManager.gm.aktualny_pionek = -1;
            GameManager.gm.Kostki[session_id].isRollingNow = false;
            //GameManager.gm.Kostki[session_id].finalSide = 0;
            string message_id = "";
            message_id = receive_one_byte();

            if (Convert.ToInt16(message_id) == 4)
            {
                string protocol = "";
                while (protocol.IndexOf("\r\n\r\n") == -1)
                {
                    string data = receive_one_byte();
                    protocol += data;
                }
                string[] separator = { "\r\n" };
                string[] splitted_protocol = protocol.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                int content_length = Convert.ToInt32(splitted_protocol[2]);
                int counter = 0;
                string server_message = "";
                while (counter < content_length)
                {
                    server_message += receive_one_byte();
                    counter++;
                }
                string decrypted_message = Decrypt(server_message, key);
                GameManager.gm.WhoNow = Convert.ToInt16(decrypted_message);

                if (Convert.ToInt16(decrypted_message) == GameManager.gm.My_ID) // jezeli moj ruch
                {
                    GameManager.gm.Kostki[GameManager.gm.My_ID].canRoll = true;
                    GameManager.gm.Kostki[GameManager.gm.My_ID].finalSide = 0;
                    send_message_0_1(5); // prosba o rolla

                    string id_rolled_value = receive_one_byte();
                    Debug.Log("DOSTALEM 5: " + id_rolled_value);
                    if (id_rolled_value == "5")
                    {
                        string protocol_5 = "";
                        while (protocol_5.IndexOf("\r\n\r\n") == -1)
                        {
                            string data_5 = receive_one_byte();
                            protocol_5 += data_5;
                        }
                        Debug.Log("Protocol: " + protocol_5);
                        string[] separator_5 = { "\r\n" };
                        string[] splitted_protocol_5 = protocol_5.Split(separator_5, StringSplitOptions.RemoveEmptyEntries);

                        int content_length_5 = Convert.ToInt32(splitted_protocol_5[2]);
                        int counter_5 = 0;
                        string server_message_5 = "";
                        while (counter_5 < content_length_5)
                        {
                            server_message_5 += receive_one_byte();
                            counter_5++;
                        }
                        string decrypted_message_5 = Decrypt(server_message_5, key);
                        Debug.Log("DOSTALEM ROLLA: " + decrypted_message_5);

                        GameManager.gm.debuglog.text = decrypted_message_5;
                        GameManager.gm.Kostki[session_id].finalSide = Convert.ToInt16(decrypted_message_5);

                        // WYKONALEM ROLLOWANIE I MOGLEM SIE RUSZYC

                        //JESLI KLIKNAL GRACZ
                        // czekanie na klikniecie gracza:
                        //StartCoroutine("wait_for_dice", decrypted_message_5);
                        while (GameManager.gm.Kostki[session_id].isRollingNow == false)
                        {
                        }
                        GameManager.gm.debuglog.text = "aktualny pionek: " + GameManager.gm.aktualny_pionek.ToString();
                        GameManager.gm.aktualny_pionek = -1;
                        GameManager.gm.aktualny_pionek = GameManager.gm.isPossibleMove();
                        while (GameManager.gm.aktualny_pionek == -1)
                        {
                        }
                        send_message_6(GameManager.gm.aktualny_pionek.ToString() + "\r\n" + decrypted_message_5);
                        GameManager.gm.aktualny_pionek = -1;

                        Debug.Log("siema");



                        // 6/JD1.1//session-id//contentlength\r\n\r\nROLL_VALUE\r\nPOZYCJE_PIONKOW

                        //  POWINNO BYĆ W ELSIE BO GRACZ KTOREGO JEST RUCH NIE MUSI MIEC INFORMACJI O SWOICH PIONKACH I WYLOSOWANEJ LICZBE
                    }
                }
                else
                {
                    string id_msg_roll_pozycje_pionkow = receive_one_byte();
                    Debug.Log(id_msg_roll_pozycje_pionkow);
                    if (id_msg_roll_pozycje_pionkow == "7")
                    {
                        string protocol_7_roll_pozycje = "";
                        while (protocol_7_roll_pozycje.IndexOf("\r\n\r\n") == -1)
                        {
                            string data_7 = receive_one_byte();
                            protocol_7_roll_pozycje += data_7;
                        }
                        Debug.Log("Protocol: " + protocol_7_roll_pozycje);
                        string[] separator_7 = { "\r\n" };
                        string[] splitted_protocol_7 = protocol_7_roll_pozycje.Split(separator_7, StringSplitOptions.RemoveEmptyEntries);

                        int content_length_7 = Convert.ToInt32(splitted_protocol_7[2]);
                        int counter_7 = 0;
                        string server_message_7 = "";
                        while (counter_7 < content_length_7)
                        {
                            server_message_7 += receive_one_byte();
                            counter_7++;
                        }
                        string decrypted_message_7 = Decrypt(server_message_7, key);
                        Debug.Log("DOSTALEM MSG: " + decrypted_message_7);

                        string[] splitted_message_7 = decrypted_message_7.Split(separator_7, StringSplitOptions.RemoveEmptyEntries);

                        int rolled_number = Convert.ToInt32(splitted_message_7[0]);
                        int pionek = Convert.ToInt32(splitted_message_7[1]);
                        

                        GameManager.gm.debuglog.text = rolled_number.ToString() + " " + pionek.ToString();
                        GameManager.gm.Kostki[Convert.ToInt16(decrypted_message)].finalSide = rolled_number;
                        GameManager.gm.Kostki[Convert.ToInt16(decrypted_message)].StartRoll();
                        Thread.Sleep(1500);
                        if (pionek / 4 == GameManager.gm.WhoNow)
                        {
                            GameManager.gm.movePlayer(pionek, rolled_number);
                        }
                        GameManager.gm.aktualny_pionek = -1;
                        // 
                    }
                    //GameManager.gm.Kostki[Convert.ToInt16(decrypted_message)].RollTheDice(Convert.ToInt16(decrypted_message_7));

                }
            }
        }
        
    }
    public IEnumerator wait_for_dice(string decrypted_message_5)
    {
        while (GameManager.gm.Kostki[session_id].isRollingNow == false)
        {
            yield return null;
        }
        StartCoroutine("wait_for_pionek", decrypted_message_5);
        yield return null;
    }
    public IEnumerator wait_for_pionek(string decrypted_message_5)
    {
        StopCoroutine("wait_for_dice");
        while (GameManager.gm.aktualny_pionek == -1)
        {
            yield return null;
        }
        send_message_6(GameManager.gm.aktualny_pionek.ToString() + "\r\n" + decrypted_message_5);

        //GameManager.gm.aktualny_pionek = -1;
        StopCoroutine("wait_for_pionek");
        Debug.Log("WYSLALEM WIADOMOSC 6");
        yield return null;
    }
    //------------------------------------------------------------------------------------
}







/*if (session_id == 1) // RECEIVING ENCRYPTED MESSAGE AES
        {
            string wiadomosc = receive_message(); // otrzymanie zaszyfrowanej wiadomosci
            Debug.Log("SIEMA " + wiadomosc);
            
            byte[] msgtokeyyy = Encoding.UTF8.GetBytes(aes_key);

            string resultofciphertext = Decrypt(wiadomosc, msgtokeyyy);
            Debug.Log("WIADOMOSC: " + resultofciphertext);

            // NIE DZIAŁA SZYFROWANIE POPRAWIĆ @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
            string ms = "ROLLME";
            byte[] msgtosendd = Encoding.UTF8.GetBytes(ms);
            
            string cipherowe = Encrypt(ms, msgtokeyyy);
            string wynik = Decrypt(cipherowe, msgtokeyyy);
            


            byte[] msgtosend5 = Encoding.UTF8.GetBytes("1/JD/1.1\r\n" + session_id.ToString() + "\r\n" + cipherowe.Length + "\r\n\r\n" + cipherowe);
            stream.Write(msgtosend5, 0, msgtosend5.Length);

            Debug.Log(cipherowe);
            Debug.Log("ODSZYFROWANE: " + wynik);
        }*/
