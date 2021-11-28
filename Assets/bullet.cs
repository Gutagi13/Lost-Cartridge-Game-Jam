using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    public int dir;
    float lifeTime = 3;
    public ContactFilter2D cf;
    public LayerMask obstacles;
    public LayerMask  playerL;
    public GameObject player;

    void Start()
    {
        player = GameObject.Find("Player");
    }

    void Update()
    {
        List<Collider2D> c = new List<Collider2D>();
        transform.position += new Vector3(dir * .05f, 0, 0);

        GetComponent<BoxCollider2D>().OverlapCollider(cf, c);
        foreach (Collider2D col in c)
        {
            if (col.gameObject.layer == 9)
            {
                Destroy(gameObject);
            }else if (col.gameObject.layer == 8)
            {
                player.GetComponent<Player>().lifes--;
                Destroy(gameObject);
            }
        }
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }
}
