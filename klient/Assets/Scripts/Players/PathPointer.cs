using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathPointer : MonoBehaviour
{
    public MainPathPointers pathObjParent;
    public bool isFree = false;
    public List<PlayerManager> playersOnPointList = new List<PlayerManager>();

    private void Start()
    {
        pathObjParent = GetComponentInParent<MainPathPointers>();
    }

    public void AddPoint(PlayerManager pathPointer_)
    {
        playersOnPointList.Add(pathPointer_);
        AdjustPlayersToPathPointer();

    }
    public void RemovePoint(PlayerManager pathPointer_)
    {
        if (playersOnPointList.Contains(pathPointer_)) // Jeżeli pionek jest na planszy
        {
            playersOnPointList.Remove(pathPointer_);
        }
        else
        {
            Debug.Log("Cannot Find Player Pawn");
        }

    }
    public void AdjustPlayersToPathPointer() // Zmiana skali i pozycji gdy jest więcej niż 1 pionek na danym miejscu planszy
    {


        if(playersOnPointList.Count > 1) // Jeżeli na danej pozycji jest więcej niż 1 gracz
        {
            for(int i = 0; i < playersOnPointList.Count; ++i)
            { if (i % 2 == 0)
                {   
                    playersOnPointList[i].GetComponent<RectTransform>().anchoredPosition = new Vector3(playersOnPointList[i].GetComponent<RectTransform>().anchoredPosition.x + i * 5, playersOnPointList[i].GetComponent<RectTransform>().anchoredPosition.y, 1f);
                     
                }
                else
                {
                    playersOnPointList[i].transform.localPosition = new Vector3(playersOnPointList[i].GetComponent<RectTransform>().anchoredPosition.x - i * 5, playersOnPointList[i].GetComponent<RectTransform>().anchoredPosition.y, 1f);
                }
            }
        }
    }
    public void bitUpPawn()
    {
        if (playersOnPointList.Count == 2 && isFree == false)
        {
            if (playersOnPointList[0].id_pionek / 4 != playersOnPointList[1].id_pionek / 4)
            {
                switch (playersOnPointList[0].id_pionek / 4)
                {
                    case 0:
                        playersOnPointList[0].goToBase(playersOnPointList[0].pathParent.redPoints);
                        break;
                    case 1:
                        playersOnPointList[0].goToBase(playersOnPointList[0].pathParent.greenPoints);
                        break;
                    case 2:
                        playersOnPointList[0].goToBase(playersOnPointList[0].pathParent.bluePoints);
                        break;
                    case 3:
                        playersOnPointList[0].goToBase(playersOnPointList[0].pathParent.yellowPoints);
                        break;
                }
                
            }
        }
    }
}
