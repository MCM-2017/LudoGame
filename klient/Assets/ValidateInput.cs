using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ValidateInput : MonoBehaviour
{
    public InputField adres;
    public InputField port;
    public Text validationText;
    // Start is called before the first frame update
    public void validateAndPlay()
    {
        int n;
        if (adres.text != "" && port.text != "")
        {
            if(int.TryParse(port.text, out n))
            {
                //GameManager.gm.port = n;
                //GameManager.gm.hostname = adres.ToString();
                SceneManager.LoadScene(2);
            }
            else
            {
                validationText.text = "Port musi być liczbą!";
            }
        }
        else
        {
            validationText.text = "Pola nie mogą być puste!";
        }
    }
}
