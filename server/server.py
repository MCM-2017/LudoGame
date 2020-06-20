import base64
import socket
import _thread as thread
import random
import time
import pyaes  # trzeba zainstalować pakiet PYAES


class Client(object):
    def __init__(self, g, p, secret_key, client, addr, id):
        self.g = g
        self.p = p
        self.secret_key = secret_key
        self.s = 0
        self.client = client
        self.addr = addr
        self.id = id
        self.aes_key = ""
        self.iv = ""


array_clients = []


def is_prime(x):
    count = 0
    for i in range(int(x / 2)):
        if x % (i + 1) == 0:
            count += 1
        if count > 1:
            break
    if count == 1:
        return 1
    else:
        return count


def prime_number():
    while True:
        x = random.randint(100, 500)
        odp = is_prime(x)
        if odp == 1:
            return x


def gcd(a, b):
    while b != 0:
        a, b = b, a % b
    return a


def primitive_root(modulo):
    roots = []
    required_set = set(num for num in range(1, modulo) if gcd(num, modulo) == 1)

    for g in range(1, modulo):
        actual_set = set(pow(g, powers) % modulo for powers in range(1, modulo))
        if required_set == actual_set:
            roots.append(g)
    return roots[0]


def receive_by_one_byte(player):
    client_protocol = b''
    while b'\r\n\r\n' not in client_protocol:  # pobieramy pozostałą część naglowka aż do napotkania "\r\n\r\n". Naglowek wyglada "0/JD/1.1\r\nBYLEJAKALICZBA\r\n\r\n....."
        client_protocol += player.recv(1)  # pobieram po 1 bajcie
    return client_protocol.decode()


def receive_by_content_length(content_length, player):
    counter = 0
    client_data = b''
    while counter < content_length:
        client_data += player.recv(1)  # pobieram po 1 bajcie
        counter += 1
    return client_data.decode()


def encrypt_message(plaintext, id):
    key = array_clients[int(id)].aes_key.encode()

    encrypter = pyaes.Encrypter(pyaes.AESModeOfOperationECB(key))
    ciphertext = encrypter.feed(plaintext)
    ciphertext += encrypter.feed()

    ciphertextbase64 = base64.b64encode(ciphertext)
    return ciphertextbase64.decode()


def decrypt_message(ciphertextb64, id):
    key = array_clients[int(id)].aes_key.encode()

    ciphertextb64 = base64.b64decode(ciphertextb64)

    decryptor = pyaes.Decrypter(pyaes.AESModeOfOperationECB(key))
    decryptedmessage = decryptor.feed(ciphertextb64)
    decryptedmessage = decryptor.feed(ciphertextb64[:int(len(ciphertextb64) / 2)])
    decryptedmessage += decryptor.feed(ciphertextb64[int(len(ciphertextb64) / 2):])

    return decryptedmessage.decode()


def _id(id, client, addr):
    if id == 0:  # wysylamy protokol o id=1
        print("------------------------------")
        print("Client " + str(len(array_clients)) + " sent new protocol message with id = " + str(id))

        protocol_client_0 = receive_by_one_byte(client)
        print("Client's protocol: " + str(id) + " " + protocol_client_0)

        content_length = int(protocol_client_0.split()[
                                 1])  # splituję w celu wyodrębnienia wartości Content-Length przesyłanej w protokole

        message_client_0 = receive_by_content_length(content_length, client)
        print("Message: " + message_client_0)

        session_id = len(array_clients)
        # ALGORYTM DIFFIEGO HELLMANA
        p = prime_number()  # p - serwer ustawia liczbę p(liczba pierwsza) dla serwera i klienta
        g = primitive_root(
            p)  # g - serwer ustawia liczbę g(podstawę - pierwiastek pierwotny modulo p) dla serwera i klienta
        secret_key = random.randint(5, 1000)  # ustalenie tajnej liczby całkowitej tylko dla serwera(a)
        result_DH = (g ** secret_key) % p  # wynik algorytmu Diffiego Hellmana (A = g^a mod p)
        array_clients.append(
            Client(g, p, secret_key, client, addr, session_id))  # dodaję klienta do tablicy wszystkich klientów(graczy)

        data = "HELLO\r\n" + str(p) + "\r\n" + str(g) + "\r\n" + str(result_DH) + "\r\n" + str(session_id)
        protocol = "1/JD/1.1\r\n" + str(session_id) + "\r\n" + str(len(data)) + "\r\n\r\n"
        client.send((protocol + data).encode())
        print("WYSLALANO DO KLIENTA " + str(len(array_clients)) + " PROTOKOL nr 1" + "\n" + protocol.replace("\r\n",
                                                                                                             " ") + data.replace(
            "\r\n", " "))
        print("P=" + str(p) + " G=" + str(g) + " SERVER_RESULT=" + str(result_DH))
        print("------------------------------")
    elif id == 1:
        print("------------------------------")
        protocol_client_1 = receive_by_one_byte(client)
        print("Client " + str(protocol_client_1.split()[1]) + " sent: " + str(id) + " " + protocol_client_1)

        session_id = int(protocol_client_1.split()[1])  # splituję, żeby wyodrębnić session_id oraz content-length
        content_length = int(protocol_client_1.split()[2])

        message_client_1 = receive_by_content_length(content_length, client)
        print("Message content: " + message_client_1)

        client_result = int(message_client_1.split()[0])
        array_clients[session_id].s = (client_result ** array_clients[session_id].secret_key) % array_clients[
            session_id].p
        print("Value of S: " + str(array_clients[session_id].s) + " - encrypting value of AES's key")

        finally_aes_key = ''
        a = str(array_clients[session_id].s)
        for i in range(0, 16):
            finally_aes_key += a[i % len(a)]

        array_clients[session_id].aes_key = finally_aes_key
        print("FINALLY AES KEY: ", array_clients[session_id].aes_key)


def new_thread(client, addr):
    id = int(client.recv(
        1).decode())  # pobieramy tylko pierwszy bajt wiadomosci protokolu, czyli ID. protokol wyglada: "0/JD/1.1\r\n...."

    _id(id, client, addr)

    id = int(client.recv(1).decode())

    _id(id, client, addr)


def create_protocol4(player, turn):
    encrypted_message = encrypt_message(turn, player.id)
    return "4/JD/1.1\r\n" + str(player.id) + "\r\n" + str(len(encrypted_message)) + "\r\n\r\n" + encrypted_message


who_turn = "0"


def roll():
    return random.randint(1, 6)


while True:
    server = socket.socket(socket.AF_INET)
    server.bind(('localhost', 1337))
    server.listen(5)

    cl, addr = server.accept()
    print("CONNECTED NEW PLAYER ABOUT ID: " + str(len(array_clients)) + "\nIP: " + addr[0])

    thread.start_new_thread(new_thread, (cl, addr))
    time.sleep(2)

    if len(array_clients) == 4:
        counter_moves = 0
        while True:
            rolled = 0
            print("-------------")
            print("ZACZYNAMY GRE")
            for player_who_turn in array_clients:
                protocol4 = create_protocol4(player_who_turn, who_turn)  # kogo tura wysylanie
                player_who_turn.client.send(protocol4.encode())
            print("Elo")
            index = int(who_turn)
            # while True:
            player = array_clients[index]

            id_message = int(player.client.recv(1).decode())
            if id_message:
                print(id_message)
                if id_message == 5:  # odebralem 5/JD1.1
                    protocol_client_5 = receive_by_one_byte(player.client)

                    print("Client's protocol: " + protocol_client_5)
                    print("ZESPLITOWANE: ", protocol_client_5.split())
                    id_player = int(protocol_client_5.split()[1])
                    content_length = int(protocol_client_5.split()[
                                             2])  # splituję w celu wyodrębnienia wartości Content-Length przesyłanej w protokole

                    message_client_5 = receive_by_content_length(content_length, player.client)
                    print("Message content: " + message_client_5)
                    result = decrypt_message(message_client_5, id_player)
                    result = result.split()[0]
                    print("Client msg: ", result)

                    if result == "ROLL":
                        rolled = str(roll())
                        print("WYLOSOWALEM: ", rolled)
                        rolled_value = encrypt_message(rolled, player.id)
                        msg_to_send_roll = "5/JD.1.1\r\n" + str(player.id) + "\r\n" + str(
                            len(str(rolled_value))) + "\r\n\r\n" + rolled_value

                        player.client.send(str(msg_to_send_roll).encode())
                        print("WYSLALEM + aes ", str(msg_to_send_roll))

                        id_message_6 = int(player.client.recv(1).decode())
                        print("ALE WIADOMOSC:", id_message_6)
                        if id_message_6 == 6:
                            protocol_client_6 = receive_by_one_byte(player.client)
                            print("PROTOKOL 6: ", protocol_client_6)

                            content_length = int(protocol_client_6.split()[2])

                            message_client_6 = receive_by_content_length(content_length, player.client)
                            message_client_6 = decrypt_message(message_client_6, player.id).split()[0]
                            print("MESSAGE 6: ", message_client_6)
                            # SKONCZYLISMY TUTAJ, KLIENT MUSI PRZESLAC POZYCJE PIONKOW DO SERWERA(JAK TO ZROBIC?)

                            for a in array_clients:
                                if a != player:
                                    msg_roll_pozycje_pionkow = rolled + "\r\n" + message_client_6
                                    msg_roll_pozycje_pionkow = encrypt_message(msg_roll_pozycje_pionkow, a.id)
                                    fullmsg_protcol_roll_pozycje_pionkow = "7/JD.1.1\r\n" + str(a.id) + "\r\n" + str(len(msg_roll_pozycje_pionkow)) + "\r\n\r\n" + msg_roll_pozycje_pionkow
                                    a.client.send(fullmsg_protcol_roll_pozycje_pionkow.encode())
                                    print("WYSLALEM WIADOMOSC O POZYCJACH I ROLLU DO INNYCH")
            if counter_moves < 2 and rolled == "6":
                counter_moves += 1
            else:
                who_turn = str((int(who_turn) + 1) % 4)
                counter_moves = 0
