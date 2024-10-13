using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Box : Obstacle
{
    private void Start()
    {
        health = 1;  
    }

    public override void TakeDamage(int amount)
    {
            health -= amount;
            if (health <= 0)
            {
                // TODO: ADD EXPLOSION EFFECT
                DestroyObstacle();
                
            }
    }


}

