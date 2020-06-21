using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RedPlayer : PlayerManager, IPointerClickHandler
{
    RollowanieKostka redDice; // Kostka przypisana do gracza czerwonego
    public void Start()
    {
        redDice = GetComponentInParent<RedHome>().rollowanieKostka;
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
                        goOutFromBase(pathParent.redPoints); // wyjdz pionkiem z bazy i ustaw w pozycji początkowej
                        GameManager.gm.stepsToMove = 0;
                        return;
                    }
                }
                if (isOutBase)
                {
                    canMove = true;
                }

                Move(pathParent.redPoints);
            }
        }
    }
    public void MoveMe()
    {

            if (!isOutBase)
            {
                if (GameManager.gm.stepsToMove == 6) // Jeżeli nasz ruch i wylosowaliśmy 6,  to możemy wyjść pionkiem z bazy
                {
                    goOutFromBase(pathParent.redPoints); // wyjdz pionkiem z bazy i ustaw w pozycji początkowej
                    GameManager.gm.stepsToMove = 0;
                    return;
                }
            }
            if (isOutBase)
            {
                canMove = true;
            }
        Move(pathParent.redPoints);
    }


}
