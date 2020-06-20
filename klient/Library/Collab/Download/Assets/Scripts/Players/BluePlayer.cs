using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

public class BluePlayer : PlayerManager, IPointerClickHandler
{
    RollowanieKostka blueDice; // Kostka przypisana do gracza czerwonego
    public void Start()
    {
        blueDice = GetComponentInParent<BlueHome>().rollowanieKostka;
    }
    public void OnPointerClick(PointerEventData eventData)
    {

        if (GameManager.gm.My_ID == GameManager.gm.WhoNow)
        {
            if (!isOutBase)
            {
                //if (GameManager.gm.stepsToMove == 6) // Jeżeli nasz ruch i wylosowaliśmy 6,  to możemy wyjść pionkiem z bazy
                //{
                    goOutFromBase(pathParent.bluePoints); // wyjdz pionkiem z bazy i ustaw w pozycji początkowej
                    GameManager.gm.stepsToMove = 0;
                    return;
                //}
            }
            if (isOutBase)
            {
                canMove = true;
            }

            Move(pathParent.bluePoints);
        }

    }
    public void MoveMe()
    {

        if (!isOutBase)
        {
            if (GameManager.gm.stepsToMove == 6) // Jeżeli nasz ruch i wylosowaliśmy 6,  to możemy wyjść pionkiem z bazy
            {
                goOutFromBase(pathParent.bluePoints); // wyjdz pionkiem z bazy i ustaw w pozycji początkowej
                GameManager.gm.stepsToMove = 0;
                return;
            }
        }
        if (isOutBase)
        {
            canMove = true;
        }
        Move(pathParent.bluePoints);
    }

}
