using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Stone : Obstacle
{
    private void Start()
    {
        health = 1;  
    }

    public override void TakeDamage(int amount)
    {
            if (amount > 0)
            {
                health -= amount;
                if (health <= 0)
                {
                    // TODO: ADD EXPLOSION EFFECT
                    DestroyObstacle();
                }
            }
    }


}

