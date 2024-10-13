using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq; // for extra list functionality

public class MatchFinder : MonoBehaviour
{
    private Board board;
    
    // track matches
    public List<Gem> currentMatches = new List<Gem>();

    // track TNT logo displays
    public List<Gem> currentBombPotentialMatches = new List<Gem>();

    [HideInInspector]
    public Gem clicked;

    private void Awake()
    {
        board = FindObjectOfType<Board>();
    }

    // second implementation attempt, works fine for now :D
    public void FindMatchesFromGem(Gem startGem)
    {
        // clear previous matches
        currentMatches.Clear();

        // use a queue to find all connected gems of the same type
        Queue<Gem> gemsToCheck = new Queue<Gem>();
        HashSet<Gem> checkedGems = new HashSet<Gem>();

        gemsToCheck.Enqueue(startGem);
        checkedGems.Add(startGem);

        while (gemsToCheck.Count > 0)
        {
            Gem currentGem = gemsToCheck.Dequeue();
            currentMatches.Add(currentGem);
            currentGem.isMatched = true;

            // check all adjacent positions (up, down, left, right)
            Vector2Int[] directions = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

            foreach (Vector2Int direction in directions)
            {
                Vector2Int neighborPos = currentGem.posIndex + direction;

                // avoid out of bounds error
                if (board.IsWithinBounds(neighborPos))
                {
                    Gem neighborGem = board.allGems[neighborPos.x, neighborPos.y];

                    // check if the neighbor exists, is of the same type, and hasn't been checked yet
                    if (neighborGem != null && neighborGem.type == startGem.type && !checkedGems.Contains(neighborGem))
                    {
                        gemsToCheck.Enqueue(neighborGem);
                        checkedGems.Add(neighborGem);
                    }
                }
            }
        }


        if (currentMatches.Count <= 1)
        {
            // if no match, set isMatched to false for all
            foreach (Gem gem in currentMatches)
            {
                gem.isMatched = false;
            }
        }
        else
        {
            // if there is a valid match, trigger obstacle damage near the matched gems
            foreach (Gem matchedGem in currentMatches)
            {
                DamageAdjacentBoxes(matchedGem.posIndex);  
            }
        }

        // Handle spawning TNT if applicable
        if (currentMatches.Count >= 5)
        {
            CreateTNT(startGem.posIndex);
            Destroy(startGem.gameObject);
        }

        // remove duplicates
        currentMatches = currentMatches.Distinct().ToList();
    }


    // handle box/vase destruction on matches
    private void DamageAdjacentBoxes(Vector2Int gemPos)
    {
        Vector2Int[] adjacentPositions = new Vector2Int[]
        {
        new Vector2Int(gemPos.x, gemPos.y + 1),  // Up
        new Vector2Int(gemPos.x, gemPos.y - 1),  // Down
        new Vector2Int(gemPos.x + 1, gemPos.y),  // Right
        new Vector2Int(gemPos.x - 1, gemPos.y)   // Left
        };

        // Loop through adjacent positions and check if there's a box or vase
        foreach (Vector2Int adjacentPos in adjacentPositions)
        {
            if (board.IsWithinBounds(adjacentPos))
            {
                Obstacle obstacle = board.allObstacles[adjacentPos.x, adjacentPos.y];
                if (obstacle != null)
                {
                    if (obstacle is Box)
                    {
                        // Apply damage only from actual matches
                        obstacle.TakeDamage(1);

                        // If the obstacle is destroyed, remove it from the board
                        if (obstacle.isDestroyed)
                        {
                            board.boxRemaining--;
                            board.allObstacles[adjacentPos.x, adjacentPos.y] = null;
                        }
                    }
                    else if (obstacle is Vase)
                    {
                        obstacle.TakeDamage(1);

                        // If the obstacle is destroyed, remove it from the board
                        if (obstacle.isDestroyed)
                        {
                            board.allObstacles[adjacentPos.x, adjacentPos.y] = null;
                        }
                    }

                }

            }
        }
        board.roundManager.CheckForGameOver();
    }


    //---------------------------------------------------Start of TNT-------------------------------------------------------------------
    private void CreateTNT(Vector2Int pos)
    {
        Gem tntGem = Instantiate(board.TNT, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);

        // set its position on the board
        tntGem.SetupGem(pos, board);
        board.allGems[pos.x, pos.y] = tntGem;

        // set its type to TNT
        tntGem.type = Gem.GemType.TNT;
    }

    public void Boom(Gem tntGem)
    {
        if (tntGem != null) 
        {
            Vector2Int pos = tntGem.posIndex;

            // explode the required area
            board.ExplodeArea(tntGem.posIndex, 5);

            // destroy the current gem after explosion to avoid a bug
            Destroy(tntGem.gameObject);
        }

    }


    public void TNTCombo(Gem tntGem)
    {
        Vector2Int pos = tntGem.posIndex;

        // explode the required area
        board.ExplodeArea(tntGem.posIndex, 7);

        // destroy the current gem after explosion to avoid a bug
        Destroy(tntGem.gameObject);
    }


    public bool CheckForAdjacentTNT(Gem tntGem)
    {
        Vector2Int pos = tntGem.posIndex;

        List<Vector2Int> adjacentPositions = new List<Vector2Int>
        {
        new Vector2Int(pos.x + 1, pos.y),  // right
        new Vector2Int(pos.x - 1, pos.y),  // left
        new Vector2Int(pos.x, pos.y + 1),  // above
        new Vector2Int(pos.x, pos.y - 1)   // below
        };

        foreach (Vector2Int adjacentPos in adjacentPositions)
        {
            if (board.IsWithinBounds(adjacentPos))
            {
                Gem adjacentGem = board.allGems[adjacentPos.x, adjacentPos.y];
                if (adjacentGem != null && adjacentGem.type == Gem.GemType.TNT)
                {
                    // found a nearby TNT
                    return true;
                }
            }
        }

        return false;
    }




    //---------------------------------------------------End of TNT-------------------------------------------------------------------

    //---------------------------------------------------Start of Potential Matches---------------------------------------------------
    public void FindBombPotentialMatches()
    {
        // clear previous potential matches
        ResetBombPotentialMatches();

        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                Gem currentGem = board.allGems[i, j];
                if (currentGem != null)
                {
                    List<Gem> connectedGems = FindConnectedMatches(currentGem);

                    // is qualified for tnt?
                    if (connectedGems.Count >= 5)
                    {
                        // adjust the logo and track the potential matches
                        foreach (Gem gem in connectedGems)
                        {
                            gem.isPotentialMatch = true;  
                            gem.ShowTNTLogo();            
                            currentBombPotentialMatches.Add(gem);  
                        }
                    }
                }
            }
        }
    }

    // helper function to find all connected matching gems starting from a gem
    private List<Gem> FindConnectedMatches(Gem startGem)
    {
        List<Gem> connectedGems = new List<Gem>();
        Queue<Gem> gemsToCheck = new Queue<Gem>();
        gemsToCheck.Enqueue(startGem);
        connectedGems.Add(startGem);

        // similar logic to finding matches
        while (gemsToCheck.Count > 0)
        {
            Gem currentGem = gemsToCheck.Dequeue();

            foreach (Gem neighbor in GetNeighbors(currentGem))
            {
                if (neighbor != null && !connectedGems.Contains(neighbor) && neighbor.type == startGem.type)
                {
                    connectedGems.Add(neighbor);
                    gemsToCheck.Enqueue(neighbor);
                }
            }
        }
        return connectedGems;
    }

    // helper function to get neighbors of a gem 
    private List<Gem> GetNeighbors(Gem gem)
    {
        List<Gem> neighbors = new List<Gem>();
        Vector2Int pos = gem.posIndex;

        // left
        if (pos.x > 0)
        {
            neighbors.Add(board.allGems[pos.x - 1, pos.y]);
        }

        // right
        if (pos.x < board.width - 1)
        {
            neighbors.Add(board.allGems[pos.x + 1, pos.y]);
        }

        // above
        if (pos.y < board.height - 1)
        {
            neighbors.Add(board.allGems[pos.x, pos.y + 1]);
        }

        // below
        if (pos.y > 0)
        {
            neighbors.Add(board.allGems[pos.x, pos.y - 1]);
        }

        return neighbors;
    }

    // resets the potential bomb matches, hiding TNT logos
    public void ResetBombPotentialMatches()
    {
        foreach (Gem gem in currentBombPotentialMatches)
        {
            gem.isPotentialMatch = false;   
            gem.HideTNTLogo();              
        }
        currentBombPotentialMatches.Clear();
    }

    //---------------------------------------------------End of Potential Matches---------------------------------------------------

}
