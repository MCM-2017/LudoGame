using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public bool isOutBase; // Wartość 0 - Pionek znajduje się w bazie. 1 - Pionek znajduje się na planszy.
    public bool canMove; // Wartość 0 - Pionek nie ma możliwości ruchu. 1 - Pionek ma możliwość ruchu
    public bool moveNow;
    public int current_moveNumberofSteps;
    public PathPointer p_pathPointer; // poprzedni punkt na planszy
    public PathPointer c_pathPointer; // aktualna punkt na planszy

    public int id_pionek;

    public MainPathPointers pathParent;
    public  Vector3 startPos;
    Coroutine move_Coroutine;
    Coroutine goToBase_Coroutine;
    private void Start()
    {

    }
    private void Awake()
    {
        pathParent = FindObjectOfType<MainPathPointers>();

    }

    public void Move(PathPointer[] currentPathPointers)
    {
        move_Coroutine = StartCoroutine(MoveStepsEnum(currentPathPointers));
    }
    public void goToBase(PathPointer[] currentPathPointers)
    {
       goToBase_Coroutine = StartCoroutine(BackToBase(currentPathPointers));
    }

    public void goOutFromBase(PathPointer[] currentPathPointers)
    {
        isOutBase = true;
        transform.position = currentPathPointers[0].transform.position; // ustawienie początkowe pionka
        current_moveNumberofSteps = 1;
        c_pathPointer = currentPathPointers[0]; //Jak wyjdę z bazy to aktualny i poprzedni pionek są tymi samymi
        p_pathPointer = currentPathPointers[0];
        GameManager.gm.AddPlayerToBoardPoint(c_pathPointer);
        //
        GameManager.gm.aktualny_pionek = this.id_pionek;
        c_pathPointer.AddPoint(this);
        c_pathPointer.AdjustPlayersToPathPointer();
        
    }
    bool isPossibleToMove(int stepsToMove, int alreadyMovedAmount, PathPointer[] currentPathPointers) // czy pójście o x kroków nie naruszy IOR 
    {
        int remainingSteps = currentPathPointers.Length - alreadyMovedAmount; // wyliczam pozostałą ilość dostępnych kroków
        return remainingSteps >= stepsToMove ? true : false; // zwracam czy można pójść dalej
    }
    public IEnumerator MoveStepsEnum(PathPointer[] currentPathPointers)
    {
        
        yield return new WaitForSeconds(0.2f);
        int gmStepsToMove = GameManager.gm.stepsToMove;
        if (canMove)
        {
            if (isPossibleToMove(gmStepsToMove, current_moveNumberofSteps, currentPathPointers))
            {
                GameManager.gm.aktualny_pionek = this.id_pionek;

                for (int i = current_moveNumberofSteps; i < current_moveNumberofSteps + gmStepsToMove; ++i)
                {
                    transform.position = currentPathPointers[i].transform.position;
                    yield return new WaitForSeconds(0.2f);
                }


                current_moveNumberofSteps += gmStepsToMove; // aktualnie przebyta odleglosc na planszy

                GameManager.gm.RemovePlayerFromBoardPoint(p_pathPointer);
                p_pathPointer.RemovePoint(this);

                c_pathPointer = currentPathPointers[current_moveNumberofSteps-1];
                c_pathPointer.AddPoint(this);
                GameManager.gm.AddPlayerToBoardPoint(c_pathPointer);
                p_pathPointer = c_pathPointer;
                c_pathPointer.bitUpPawn();
                GameManager.gm.stepsToMove = 0;
            }
        }
        if (move_Coroutine !=null)
        {
            StopCoroutine(move_Coroutine);
        }
    }
    public IEnumerator BackToBase(PathPointer[] currentPathPointers)
    {

        yield return new WaitForSeconds(0.1f);
        int gmStepsToMove = GameManager.gm.stepsToMove;
        GameManager.gm.RemovePlayerFromBoardPoint(p_pathPointer);
        p_pathPointer.RemovePoint(this);
        for (int i = current_moveNumberofSteps-1; i >0; --i)
        {
            transform.position = currentPathPointers[i].transform.position;
            yield return new WaitForSeconds(0.1f);
        }
        current_moveNumberofSteps = 0 ; // aktualnie przebyta odleglosc na planszy  
        transform.position = startPos;
        isOutBase = false;
        if (move_Coroutine != null)
        {
            StopCoroutine(goToBase_Coroutine);
        }
    }
}

