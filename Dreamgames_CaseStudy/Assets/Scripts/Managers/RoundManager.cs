using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public int moves = 1;
    [HideInInspector]
    public bool roundOver = false;

    private UIManager uiMain;

    private Board board;

    private LevelManager levelManager;

    private int boxCount = 1;
    private int stoneCount = 1;
    private int vaseCount = 1;

    private void Awake()
    {
        uiMain = FindObjectOfType<UIManager>();
        board = FindObjectOfType<Board>();
        levelManager = FindObjectOfType<LevelManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        uiMain.UpdateMovesUI(moves);
        uiMain.UpdateObstacleCount(boxCount,stoneCount,vaseCount);
    }

    public void PlayerMadeMove()
    {
        board.remainingMoves--;                       // decrease moves
        moves = board.remainingMoves;

        uiMain.UpdateObstacleCount(boxCount, stoneCount, vaseCount);

        if (board.totalObstacles <= 0)
        {
            GameOver(true);
        }
        else if (moves <= 0)
        {
            GameOver(false);  
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckForGameOver();
        obstacleCounter();
        moveCounter();

    }

    private void moveCounter()
    {
        moves = board.remainingMoves;

        uiMain.UpdateMovesUI(moves);
    }
    private void obstacleCounter()
    {
        boxCount = board.boxRemaining;
        stoneCount = board.stoneRemaining;
        vaseCount = board.vaseRemaining;

        uiMain.UpdateObstacleCount(boxCount, stoneCount, vaseCount);
    }
    public void CheckForGameOver()
    {
        if ((stoneCount+boxCount+vaseCount) <= 0)
        {
            GameOver(true);
        }
    }

    // handle game over (whether player won or lost)
    private void GameOver(bool playerWon)
    {
        if (playerWon)
        {
            uiMain.winScreen.SetActive(true);

            // save progress
            levelManager.CompleteLevel();

            //return back to main menu
            StartCoroutine(backToMainMenu());

        }
        else
        {
            uiMain.loseScreen.SetActive(true);
        }

        // disable further player actions here
        board.currentState = Board.BoardState.wait;  
    }

    private IEnumerator backToMainMenu()
    {
        yield return new WaitForSeconds(6);
        levelManager.LoadMainMenu();
    }
}
