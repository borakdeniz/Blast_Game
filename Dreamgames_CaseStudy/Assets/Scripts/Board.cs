using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Gem;

public class Board : MonoBehaviour
{
    public int currentLevel;

    public int width;
    public int height;

    public GameObject bgTilePrefab;

    public Gem[] gems;

    public Obstacle[] obstacles;

    public Gem[,] allGems;

    public Obstacle[,] allObstacles;

    private BoardLayout boardLayoutGems;
    private Gem[,] layoutStoreGems;

    private BoardLayout boardLayoutObstacles;
    private Obstacle[,] layoutStoreObstacles;

    public float gemSpeed;

    public int totalObstacles;

    [HideInInspector]
    public RoundManager roundManager;

    // to keep chain explosions chip away turns
    [HideInInspector]
    public bool moveCountedForAction = false;

    [HideInInspector]
    public MatchFinder matchFind;

    [HideInInspector]
    public List<Gem> explodedTNTs = new List<Gem>();

    public int remainingMoves = 20;


    // to keep player from playing while the game takes place
    public enum BoardState
    {
        wait,
        move
    }
    public BoardState currentState = BoardState.move;

    public Gem TNT;

    [HideInInspector]
    public Gem clickedGem;

    [HideInInspector]
    public int stoneRemaining = 0;
    [HideInInspector]
    public int boxRemaining = 0;
    [HideInInspector]
    public int vaseRemaining = 0;

    private float spacingScale = 0.8f;

    private void Awake()
    {
        matchFind = FindObjectOfType<MatchFinder>();
        roundManager= FindObjectOfType<RoundManager>();

        boardLayoutGems = GetComponent<BoardLayout>();
        boardLayoutObstacles = GetComponent<BoardLayout>();
    }

    // start is called before the first frame update
    void Start()
    {
        allGems = new Gem[width, height];

        allObstacles = new Obstacle[width, height];

        layoutStoreGems = new Gem[width, height];

        layoutStoreObstacles = new Obstacle[width, height];

        Setup();
    }

    private void Update()
    {
        // continuously check for TNT potentials
        if (currentState == BoardState.move)
        {
            matchFind.FindBombPotentialMatches();
        }
    }

    private void Setup()
    {
        totalObstacles = 0;

        if(boardLayoutGems != null)
        {
            layoutStoreGems = boardLayoutGems.GetLayoutGems();
        }

        if (boardLayoutGems != null)
        {
            layoutStoreObstacles = boardLayoutObstacles.GetLayoutObstacles();
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {

                // create the board
                Vector2 pos = new Vector2(i, j);

                GameObject bgTile = Instantiate(bgTilePrefab, pos, Quaternion.identity);

                // to keep the hierarchy clean
                bgTile.transform.parent = transform;
                bgTile.name = "BG Tile - " + i + ", " + j;

                // spawn obstacles or gems and track the number of obstacles
                if (layoutStoreObstacles[i,j] != null)
                {
                    AddObstacle(new Vector2Int(i,j), layoutStoreObstacles[i,j]);
                }
                else
                {
                    if (layoutStoreGems[i, j] != null)
                    {
                        SpawnGem(new Vector2Int(i,j), layoutStoreGems[i, j]);
                    }
                    else
                    {
                        // Spawn gems as normal
                        int gemToUse = Random.Range(0, gems.Length);
                        SpawnGem(new Vector2Int(i, j), gems[gemToUse]);
                    }
                }

            }
        }
    }
    private void AddObstacle(Vector2Int pos, Obstacle obstacleToUse)
    {
        
        Obstacle obstacle = Instantiate(obstacleToUse, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);

        obstacle.transform.parent = transform;
        obstacle.name = "Obstacle - " + pos.x + ", " + pos.y;
        allObstacles[pos.x, pos.y] = obstacle; 

        if (obstacle is Vase vase)
        {
            vase.SetupVase(new Vector2Int(pos.x, pos.y), this);  
            vaseRemaining++;
        }
        else if(obstacle is Box)
        {
            boxRemaining++;
        }
        else if(obstacle is Stone)
        {
            stoneRemaining++;
        }

        totalObstacles++; 
    }

    // testing purposes
    //private bool ShouldPlaceObstacle(int x, int y)
    //{
    //    float obstacleChance = 0.2f;
    //    return Random.value < obstacleChance;
    //}

    private void SpawnGem(Vector2Int pos, Gem gemToSpawn)
    {
        Gem gem = Instantiate(gemToSpawn, new Vector3(pos.x, pos.y + height, 0f), Quaternion.identity);

        // to keep the hierarchy clean
        gem.transform.parent = transform;
        gem.name = "Gem - " + pos.x + ", " + pos.y;

        // store the information on gem location
        allGems[pos.x, pos.y] = gem;

        gem.SetupGem(pos, this);
    }

    private void DestroyMatchedGemAt(Vector2Int pos)
    {
        if (allGems[pos.x, pos.y] != null)
        {
            if (allGems[pos.x, pos.y].isMatched && allGems[pos.x, pos.y].type != Gem.GemType.TNT)
            {
                Instantiate(allGems[pos.x, pos.y].destroyEffect,
                    new Vector2(pos.x, pos.y), Quaternion.identity);

                Destroy(allGems[pos.x, pos.y].gameObject);
                allGems[pos.x, pos.y] = null;
            }
        }
    }

    public void DestroyMatches(Gem.GemType color)
    {
        for (int i = 0; i < matchFind.currentMatches.Count; i++)
        {
            if (matchFind.currentMatches[i] != null && matchFind.currentMatches.Count > 1)
            {
                DestroyMatchedGemAt(matchFind.currentMatches[i].posIndex);
            }
        }

        StartCoroutine(DecreaseRowCo());
    }

    private IEnumerator DecreaseRowCo()
    {
        yield return new WaitForSeconds(.5f);

        for (int i = 0; i < width; i++)
        {
            for (int j = 1; j < height; j++)
            {
                if (allGems[i, j] != null)
                {
                    int fallDistance = 0;

                    // look downward and count how far the gem can fall
                    for (int k = j - 1; k >= 0; k--)
                    {
                        // stop falling if there's another gem or an obstacle below
                        if (allGems[i, k] != null || allObstacles[i, k] != null)
                        {
                            break;
                        }

                        // if it's an empty cell, increase fall distance
                        fallDistance++;
                    }

                    // if the gem can fall, move it down
                    if (fallDistance > 0)
                    {
                        allGems[i, j].posIndex.y -= fallDistance;
                        allGems[i, j - fallDistance] = allGems[i, j];
                        allGems[i, j] = null;
                    }
                }

                // handle vase falling, similar logic
                if (allObstacles[i, j] is Vase vase)
                {
                    int vaseFallDistance = 0;

                    for (int k = j - 1; k >= 0; k--)
                    {
                        if (allGems[i, k] != null || allObstacles[i, k] != null)
                        {
                            break;
                        }
                        vaseFallDistance++;
                    }

                    if (vaseFallDistance > 0)
                    {
                        vase.posIndex.y -= vaseFallDistance;
                        allObstacles[i, j - vaseFallDistance] = vase;
                        allObstacles[i, j] = null;
                    }
                }
            }
        }


        StartCoroutine(FillBoardCo());
    }

    private IEnumerator FillBoardCo()
    {
        yield return new WaitForSeconds(.75f);

        // refill the board with gems
        RefillBoard();

        yield return new WaitForSeconds(1f);

        ResetAllVaseDamageStatus();

        // set flag back
        moveCountedForAction = false;

        currentState = Board.BoardState.move;
    }

    private void RefillBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // check if there's an empty slot (no gem) and no obstacle in that cell
                if (allGems[i, j] == null && allObstacles[i, j] == null)
                {
                    int gemToUse = Random.Range(0, gems.Length);

                    SpawnGem(new Vector2Int(i, j), gems[gemToUse]);
                }
            }
        }

        // after refilling, check for any misplaced gems
        CheckMisplacedGems();
    }


    //edge case check - remove the duplicates
    private void CheckMisplacedGems()
    {
        List<Gem> foundGems = new List<Gem>();

        foundGems.AddRange(FindObjectsOfType<Gem>());

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // check the board for gems that exist on board
                if (foundGems.Contains(allGems[i, j]))
                {
                    foundGems.Remove(allGems[i, j]);
                }
            }
        }

        // destroy the extras (bugged ones)
        foreach (Gem g in foundGems)
        {
            Destroy(g.gameObject);
        }
    }

    // bound check for the match finder function
    public bool IsWithinBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    public void ExplodeArea(Vector2Int center, int radius)
    {
        for (int x = -radius / 2; x <= radius / 2; x++)
        {
            for (int y = -radius / 2; y <= radius / 2; y++)
            {
                Vector2Int pos = new Vector2Int(center.x + x, center.y + y);
                if (IsWithinBounds(pos))
                {
                    Gem gem = allGems[pos.x, pos.y];
                    if (gem != null)
                    {
                        // check if this gem is TNT and if so, trigger it
                        if (gem.type == GemType.TNT && !explodedTNTs.Contains(gem))
                        {
                            gem.ExplodeTNT();
                        }
                        else
                        {
                            // destroy the gem and clear the board space
                            Destroy(gem.gameObject);
                            allGems[pos.x, pos.y] = null;
                        }
                    }

                    Obstacle obstacle = allObstacles[pos.x, pos.y];
                    if (obstacle is Vase vase)
                    {
                        vase.TakeDamage(1);  // Apply 1 damage to the vase
                    }
                    else if (obstacle is Box)
                    {
                        obstacle.TakeDamage(1);  // Handle box
                        if (obstacle.isDestroyed)
                        {
                            boxRemaining--;  // Decrease box count
                        }
                    }
                    else if (obstacle is Stone)
                    {
                        obstacle.TakeDamage(1);  // Handle stone
                        if (obstacle.isDestroyed)
                        {
                            stoneRemaining--;  // Decrease box count
                        }
                    }

                    roundManager.CheckForGameOver();

                    // remove destroyed obstacles from the board
                    if (obstacle != null && obstacle.isDestroyed)
                    {
                        allObstacles[pos.x, pos.y] = null;  // clear the obstacle from the board
                        totalObstacles--;                   // decrease the remaining obstacle count
                    }
                }
            }
        }
        //// Check if all obstacles are cleared
        //if (totalObstacles <= 0)
        //{
        //    WinLevel();
        //}

        // after the explosion, refill the board
        StartCoroutine(DecreaseRowCo());
    }

    private void ResetAllVaseDamageStatus()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allObstacles[i, j] is Vase vase)
                {
                    vase.ResetDamageFlag();  // Reset the vase's damage flag
                }
            }
        }
    }
}
