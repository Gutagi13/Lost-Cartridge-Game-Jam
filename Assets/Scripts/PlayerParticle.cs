using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParticle : MonoBehaviour
{
    float velX,pVelX,timeSinceChange,velY;
    bool col;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        bool pCol = col;
        col = GetComponentInParent<Controller2D>().collisions.below;
        if (Mathf.Sign(velX) == -Mathf.Sign(pVelX) && Mathf.Abs(velX) >0.05f)
        {
            if (timeSinceChange > 0.3f && col)
            {
                GetComponent<ParticleSystem>().Play();
            }
            timeSinceChange = 0;
        }

        timeSinceChange += Time.deltaTime;

        pVelX = Mathf.Lerp(velX,pVelX,0.5f);
        velX = gameObject.GetComponentInParent<Player>().velocity.x;
        float pVelY = velY;
        velY = gameObject.GetComponentInParent<Player>().velocity.y;
        transform.localScale = new Vector3(Mathf.Sign(velX), 1, 1);

        if (pCol != col && (!col || pVelY<-17))
        {
            GetComponent<ParticleSystem>().Play();
        }
        
    }
}
