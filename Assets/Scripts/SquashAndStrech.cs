using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquashAndStrech : MonoBehaviour
{
    float velY;
    bool onGround;
    float pRotZ;
    void Update()
    {
        SnS();
        Rotate();
        Bop();
    }


    void Bop()
    {
        float velX = gameObject.GetComponentInParent<Player>().velocity.x;
        if(onGround)
        {
            transform.position += Vector3.up * Mathf.Abs(velX) * Mathf.Pow((1 + Mathf.Sin(30 * Time.time)) / 2 , 0.4f) / 60;
        }
    }

    void Rotate()
    {
        float velX = gameObject.GetComponentInParent<Player>().velocity.x;
        Vector3 rot = transform.rotation.eulerAngles;
        rot.z = Mathf.Clamp(-velX*2f, -10, 10);
        rot.z = Mathf.Lerp(rot.z, pRotZ, 0.9f);
        pRotZ = rot.z;
        transform.rotation = Quaternion.Euler(rot);
    }

    void SnS()
    {
        bool pOnGround = onGround;
        float pVelY = velY;
        velY = gameObject.GetComponentInParent<Player>().velocity.y;
        float velYM = Mathf.Abs(velY);
        onGround = gameObject.GetComponentInParent<Controller2D>().collisions.below;

        Vector3 scale = Vector3.one;
        scale.y = 1 + velYM / 100;
        scale.x = 1 / scale.y;

        if (!pOnGround && onGround && pVelY < 0)
        {

            scale.y = Mathf.Max(1 + pVelY / 40, 0.3f);
            scale.x = 1 / scale.y;
            transform.localScale = scale;
        }

        scale = Vector3.Lerp(scale, transform.localScale, 0.97f);
        Vector3 pos = new Vector3(0, (scale.y - 1) / 2, 0);

        transform.localScale = scale;
        transform.localPosition = pos;
    }
}
