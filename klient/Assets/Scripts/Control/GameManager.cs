using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager gm; // Instancja Game Managera, zastosowanie Singletona
    public RollowanieKostka lastRolledDice; //tutaj przechowuje ostatnio losowaną kostkę
    public int stepsToMove;
    public RollowanieKostka[] Kostki;
    public RedPlayer[] redPlayers;
    public GreenPlayer[] greenPlayers;
    public BluePlayer[] bluePlayers;
    public YellowPlayer[] yellowPlayers;
    List<PathPointer> playerOnBoard = new List<PathPointer>(); // Lista Graczy, którzy są na planszy
    public int My_ID; // ID GRACZA
    public int WhoNow = -1; // Określa z pomocą serwera, czyja jest aktualnie kolej rzutu
    public Text debuglog;

    public int aktualny_pionek = -1;

    //Dane do serwera
    public string hostname;
    public int port;

    public NetworkManager networkManager;
    private void Awake()
    {
        gm = this;    
    }
    private void Start()
    {
        networkManager = GetComponent<NetworkManager>();
    }
    public void movePlayer(int id_player_, int steps_)
    {
        stepsToMove = steps_;
        //WhoNow /= 4;
        switch (WhoNow)
        {
            case 0:
                redPlayers[id_player_ % 4].MoveMe();
                break;
            case 1:
                greenPlayers[id_player_ % 4].MoveMe();
                break;
            case 2:
                bluePlayers[id_player_ % 4].MoveMe();
                break;
            case 3:
                yellowPlayers[id_player_ % 4].MoveMe();
                break;
        }
    }
    public void AddPlayerToBoardPoint(PathPointer pathPointer_)
    {
        playerOnBoard.Add(pathPointer_);

    }
    public void RemovePlayerFromBoardPoint(PathPointer pathPointer_)
    {
        if (playerOnBoard.Contains(pathPointer_)) // Jeżeli pionek jest na planszy
        {
            playerOnBoard.Remove(pathPointer_);
        }
        else
        {
            Debug.Log("Cannot Find Player Pawn");
        }

    }
    public int isPossibleMove()
    {
        switch (WhoNow)
        {
            case 0:
                for(int i = 0; i < 4; ++i)
                {
                    if (redPlayers[i].isOutBase) return -1;
                }
                break;
            case 1:
                for (int i = 0; i < 4; ++i)
                {
                    if (greenPlayers[i].isOutBase) return -1;
                }
                break;
            case 2:
                for (int i = 0; i < 4; ++i)
                {
                    if (bluePlayers[i].isOutBase) return -1;
                }
                break;
            case 3:
                for (int i = 0; i < 4; ++i)
                {
                    if (yellowPlayers[i].isOutBase) return -1;
                }
                break;
        }
        return stepsToMove == 6 ? -1 : -2;
    }
}
