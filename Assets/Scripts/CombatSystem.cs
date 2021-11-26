using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void hit(int sign, Vector2 knockback)
    {
        Vector3 vel = new Vector3(sign*knockback.x,knockback.y,0);
        if (TryGetComponent(out Player player))
        {
            player.velocity = vel;
            player.gotHit = true;
        }else if (TryGetComponent(out GoblinAI goblin))
        {
            goblin.velocity = vel;
            goblin.gotHit = true;
        }

    }
}
