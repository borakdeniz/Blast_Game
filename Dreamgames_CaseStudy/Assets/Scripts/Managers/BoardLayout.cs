using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardLayout : MonoBehaviour
{
    public LayoutRowGems[] allRowsGems;

    public LayoutRowObstacles[] allRowsObstacles;

    public Gem[,] GetLayoutGems()
    {
        Gem[,] theLayout = new Gem[allRowsGems[0].gemsInRows.Length, allRowsGems.Length];

        for(int y =0; y < allRowsGems.Length;y++)
        {
            for(int x = 0; x < allRowsGems[y].gemsInRows.Length;x++)
            {
                if(x < theLayout.GetLength(0))
                {
                    if (allRowsGems[y].gemsInRows[x] != null)
                    {
                        theLayout[x, allRowsGems.Length - 1 - y] = allRowsGems[y].gemsInRows[x];
                    }
                }
            }
        }
        return theLayout;
    }
    public Obstacle[,] GetLayoutObstacles()
    {
        Obstacle[,] theLayout = new Obstacle[allRowsObstacles[0].obstaclesInRows.Length, allRowsObstacles.Length];

        for (int y = 0; y < allRowsObstacles.Length; y++)
        {
            for (int x = 0; x < allRowsObstacles[y].obstaclesInRows.Length; x++)
            {
                if (x < theLayout.GetLength(0))
                {
                    if (allRowsObstacles[y].obstaclesInRows[x] != null)
                    {
                        theLayout[x, allRowsObstacles.Length - 1 - y] = allRowsObstacles[y].obstaclesInRows[x];
                    }
                }
            }
        }
        return theLayout;
    }
}

[System.Serializable]
public class LayoutRowObstacles
{
    public Obstacle[] obstaclesInRows;
}


[System.Serializable]
public class LayoutRowGems
{
    public Gem[] gemsInRows;
}
