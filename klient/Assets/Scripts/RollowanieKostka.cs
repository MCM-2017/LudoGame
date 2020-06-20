using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Versioning;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RollowanieKostka : MonoBehaviour, IPointerClickHandler
{   public int id_Kostki;
    public Sprite[] stronyKostki = new Sprite[6];
    public bool canRoll = true;
    public int finalSide = 0;
    private Image rend;
    public bool isRollingNow = false;

    private void Start()
    {
        rend = GetComponent<Image>();
    }

    /*public void OnMousedown()
    {
        StartCoroutine("RollTheDice");
    }*/
    public void OnPointerClick(PointerEventData eventData)
    {   if (canRoll && GameManager.gm.My_ID == GameManager.gm.WhoNow && GameManager.gm.WhoNow == id_Kostki)
        {
            isRollingNow = true;
            StartCoroutine("RollTheDice", finalSide);

        }
    }

    public void StartRoll()
    {
        StartCoroutine("RollTheDice", finalSide);
    }

    public IEnumerator RollTheDice(int finalSide)
    {
        
        int randomDiceSide = 0;

        for(int i=0; i<=20; i++)
        {
            randomDiceSide = UnityEngine.Random.Range(0, 6);
            rend.sprite = stronyKostki[randomDiceSide];
            yield return new WaitForSeconds(0.05f);
        }
        rend.sprite = stronyKostki[finalSide-1];
        GameManager.gm.stepsToMove = finalSide;
        GameManager.gm.lastRolledDice = this;
        canRoll = false;
        //isRollingNow = false;
        Debug.Log(finalSide);
        StopCoroutine("RollTheDice");
    }









    /*public Sprite[] stronyKostki = new Sprite [6];
    public Image rander; // obrazek do randomowania

    public void OnPointerClick()
    {
        int randIndex=0;
        for(int i=0; i<20; i++)
        {
            randIndex = UnityEngine.Random.Range(0, 5);
            Debug.Log("Wylosowalem" + randIndex);
            rander.sprite = stronyKostki[randIndex];
            //yield return new WaitForSeconds(0.1f);
            //WaitForSeconds(0.1f);
            Thread.Sleep(10);
        }
        final
    }
    */
}
