using System.Collections;
using System.Collections.Generic;

using UnityEngine;

// struggled with the implementation, surely there is a better way
public abstract class Obstacle : MonoBehaviour
{
    public bool isDestroyed = false;

    public GameObject[] destroyEffect;

    protected Board board;

    // default health for obstacles
    public int health = 1;  

    public abstract void TakeDamage(int amount);

    private void Awake()
    {
        board = FindObjectOfType<Board>();
    }

    public void DestroyObstacle()
    {
        if (this != null && !isDestroyed)
        {
            isDestroyed = true;
            for(int i = 0; i < destroyEffect.Length; i++)
            {
                if (destroyEffect[i] != null) 
                {
                    Instantiate(destroyEffect[i], transform.position, Quaternion.identity);
                }
            }
            Destroy(gameObject);  
        }
    }


}



