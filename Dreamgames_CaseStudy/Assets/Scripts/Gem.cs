using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEditor.PlayerSettings;
using static UnityEditor.Progress;
using static UnityEngine.Networking.UnityWebRequest;

public class Gem : MonoBehaviour
{
    [HideInInspector]
    public Vector2Int posIndex;

    [HideInInspector]
    public Board board;

    private bool mousePressed;

    [HideInInspector]
    public Gem clickedGem;


    public enum GemType
    {
        // gems
        blue,
        green,
        red,
        yellow,
        TNT
    }
    public GemType type;

    //[HideInInspector]
    public bool isMatched;
    [HideInInspector]
    public bool isPotentialMatch;

    // gem particle effect
    public GameObject destroyEffect;

    // TNT post explosion
    public GameObject tntExtraEffect;

    // TNT possible match 
    public GameObject tntLogo;


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // movement of the gems
        if (Vector2.Distance(transform.position, posIndex) > .01f)
        {
            transform.position = Vector2.Lerp(transform.position, posIndex, board.gemSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = new Vector3(posIndex.x, posIndex.y, 0f);
            board.allGems[posIndex.x, posIndex.y] = this;
        }


        if (mousePressed && Input.GetMouseButtonUp(0))
        {
            mousePressed = false;

            if (board.currentState == Board.BoardState.move)
            {
                CheckMove();
            }
        }
    }

    public void SetupGem(Vector2Int pos, Board theBoard)
    {
        posIndex = pos;
        board = theBoard;
    }

    private void OnMouseDown()
    {
        if(board.currentState == Board.BoardState.move && board.roundManager)
        {
            mousePressed = true;

            clickedGem = this;
            board.clickedGem = clickedGem;

            
            // if tnt, check for normal or mega explosion
            if (this.type == Gem.GemType.TNT)
            {
                if (board.matchFind.CheckForAdjacentTNT(this))  
                {
                    board.matchFind.TNTCombo(this);
                }
                else
                {
                    board.matchFind.Boom(this);  
                }
            }
            StartCoroutine(WaitAMoment());
        }
    }
    private void CheckMove()
    {
        board.currentState = Board.BoardState.wait;  

        // find matches from the clicked gem
        board.matchFind.FindMatchesFromGem(clickedGem);

        // check if a valid match was found
        if (clickedGem.isMatched || clickedGem.type == GemType.TNT)
        {
            board.roundManager.PlayerMadeMove();
            board.DestroyMatches(clickedGem.type);
        }
        else
        {
            board.currentState = Board.BoardState.move;
        }
    }


    // show the TNT logo
    public void ShowTNTLogo()
    {
        if (tntLogo != null)
        {
            tntLogo.SetActive(true);  
        }
    }

    // hide the TNT logo
    public void HideTNTLogo()
    {
        if (tntLogo != null)
        {
            tntLogo.SetActive(false);  
        }
    }
    public void ExplodeTNT()
    {
        if (!board.explodedTNTs.Contains(this))
        {
            // play the effects
            Instantiate(this.destroyEffect,
                            new Vector2(this.posIndex.x, this.posIndex.y), Quaternion.identity);

            Instantiate(this.tntExtraEffect,
                            new Vector2(this.posIndex.x, this.posIndex.y), Quaternion.identity);

            // mark this TNT as exploded
            board.explodedTNTs.Add(this);

            // explode in a 5x5 area for normal TNT
            board.ExplodeArea(posIndex, 5);

            // check and trigger other TNTs in the explosion radius
            TriggerTNTsInRadius(5);

            if (!board.moveCountedForAction)
            {
                board.roundManager.PlayerMadeMove();  // Count the move
                board.moveCountedForAction = true;    // Mark the move as counted
            }

            board.roundManager.CheckForGameOver();
            StartCoroutine(WaitAMoment());
        }

    }

    public void ExplodeTNTCombo()
    {
        if (!board.explodedTNTs.Contains(this))
        {

            // play the effects
            Instantiate(this.destroyEffect,
                            new Vector2(this.posIndex.x, this.posIndex.y), Quaternion.identity);

            Instantiate(this.tntExtraEffect,
                            new Vector2(this.posIndex.x, this.posIndex.y), Quaternion.identity);

            // mark this TNT as exploded
            board.explodedTNTs.Add(this);

            // explode in a 7x7 area for the TNT-TNT combo
            board.ExplodeArea(posIndex, 7);

            // check and trigger other TNTs in the explosion radius
            TriggerTNTsInRadius(7);

            if (!board.moveCountedForAction)
            {
                board.roundManager.PlayerMadeMove();  // Count the move
                board.moveCountedForAction = true;    // Mark the move as counted
            }

            board.roundManager.CheckForGameOver();
            StartCoroutine(WaitAMoment());
        }

        
    }

    private IEnumerator WaitAMoment()
    {
        yield return new WaitForSeconds(.75f);
    }
    private void TriggerTNTsInRadius(int radius)
    {
        List<Gem> tntGems = GetTNTsInExplosionRadius(posIndex, radius);

        // trigger each TNT found in the explosion radius
        foreach (Gem tntGem in tntGems)
        {
            // avoid exploding the same TNT more than once
            if (tntGem != null && tntGem != this && !board.explodedTNTs.Contains(tntGem)) 
            {
                // trigger combo explosion for adjacent TNTs
                tntGem.ExplodeTNTCombo();  
            }
        }
    }

    private List<Gem> GetTNTsInExplosionRadius(Vector2Int center, int radius)
    {
        List<Gem> tntGems = new List<Gem>();

        // iterate through the explosion radius 
        for (int x = -radius / 2; x <= radius / 2; x++)
        {
            for (int y = -radius / 2; y <= radius / 2; y++)
            {
                Vector2Int pos = new Vector2Int(center.x + x, center.y + y);
                if (board.IsWithinBounds(pos))  
                {
                    Gem gem = board.allGems[pos.x, pos.y];
                    if (gem != null && gem.type == GemType.TNT)  
                    {
                        tntGems.Add(gem);
                    }
                }
            }
        }

        return tntGems;
    }
}

