using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public GameObject[] WhoTurnIcons = new GameObject[4];
    void Update()
    {
        for (int i = 0; i < 4; ++i)
        {
            if (GameManager.gm.WhoNow == GameManager.gm.My_ID)
            {
                WhoTurnIcons[GameManager.gm.My_ID].SetActive(true);
            }
            else WhoTurnIcons[i].SetActive(false);
        }
    }
}
