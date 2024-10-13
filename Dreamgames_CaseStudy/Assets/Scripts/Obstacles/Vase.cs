using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vase : Obstacle
{
    public GameObject damagedVase;

    [HideInInspector]
    public Vector2Int posIndex;

    public bool hasTakenDamageThisAction = false;

    private void Start()
    {
        health = 2; 
    }

    private void Update()
    {
        // Smooth vase movement (similar to how gems move)
        if (Vector2.Distance(transform.position, posIndex) > .01f)
        {
            transform.position = Vector2.Lerp(transform.position, posIndex, board.gemSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = new Vector3(posIndex.x, posIndex.y, 0f);
            board.allObstacles[posIndex.x, posIndex.y] = this;  
        }
    }

    public void SetupVase(Vector2Int pos, Board theBoard)
    {
        posIndex = pos;
        board = theBoard;
    }

    public override void TakeDamage(int amount)
    {
        // only apply damage if the vase hasn't already taken damage this action
        if (!hasTakenDamageThisAction && amount > 0)
        {
            health -= 1;  

            // mark that the vase has taken damage this action
            hasTakenDamageThisAction = true;  

            if (health <= 0)
            {
                board.vaseRemaining--;
                DestroyObstacle();  
            }
            else
            {
                damagedVase.SetActive(true);
                for (int i = 0; i < destroyEffect.Length; i++)
                {
                    if (destroyEffect[i] != null)
                    {
                        Instantiate(destroyEffect[i], transform.position, Quaternion.identity);
                    }
                }
            }
        }
    }

    public void ResetDamageFlag()
    {
        hasTakenDamageThisAction = false;
    }

}