﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GreenPlayer : PlayerManager, IPointerClickHandler
{
    RollowanieKostka greenDice; // Kostka przypisana do gracza czerwonego
    public void Start()
    {
        greenDice = GetComponentInParent<GreenHome>().rollowanieKostka;
        startPos = this.transform.position;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameManager.gm.My_ID == this.id_pionek / 4)
        {
            if (GameManager.gm.My_ID == GameManager.gm.WhoNow)
            {
                if (!isOutBase)
                {
                    if (GameManager.gm.stepsToMove == 6) // Jeżeli nasz ruch i wylosowaliśmy 6,  to możemy wyjść pionkiem z bazy
                    {
                        goOutFromBase(pathParent.greenPoints); // wyjdz pionkiem z bazy i ustaw w pozycji początkowej
                        GameManager.gm.stepsToMove = 0;
                        return;
                    }
                }
                if (isOutBase)
                {
                    canMove = true;
                }

                Move(pathParent.greenPoints);
            }
        }
    }
    public void MoveMe()
    {

        if (!isOutBase)
        {
            if (GameManager.gm.stepsToMove == 6) // Jeżeli nasz ruch i wylosowaliśmy 6,  to możemy wyjść pionkiem z bazy
            {
                goOutFromBase(pathParent.greenPoints); // wyjdz pionkiem z bazy i ustaw w pozycji początkowej
                GameManager.gm.stepsToMove = 0;
                return;
            }
        }
        if (isOutBase)
        {
            canMove = true;
        }
        Move(pathParent.greenPoints);
    }

}