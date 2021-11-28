using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shooterAI : MonoBehaviour
{
    public GameObject player;
    public GameObject bullet;
    public float shootDistance;
    public float hideDistance;
    SpriteRenderer renderer;
    public Sprite[] sprites;
    public bool hidden=false;
    public int lifes = 2;
    public bool gotHit;
    int hitTime;
    public AudioClip deathAC;
    public AudioClip shootAC;
    float shootTime;

    void Start()
    {
        renderer = gameObject.GetComponent<SpriteRenderer>();
    }

    IEnumerator Hide()
    {
        renderer.sprite = sprites[1];
        yield return new WaitForSeconds(.12f);
        renderer.sprite = sprites[2];
        hidden = true;
    }

    IEnumerator Show()
    {
        hidden = false;
        renderer.sprite = sprites[1];
        yield return new WaitForSeconds(.12f);
        renderer.sprite = sprites[0];
    }

    void Update()
    {
        float dir = Mathf.Sign(transform.position.x - player.transform.position.x);
        transform.localScale = Vector3.one + Vector3.right * (dir-1);
        if (lifes <= 0)
        {
            Destroy(gameObject, 0.65f);
        }

        if (!hidden)
        {
            shootTime--;
            if (shootTime <= 0)
            {
                shootTime = 100;
                Debug.Log("shoot");
                GameObject b = Instantiate(bullet,transform);
                b.GetComponent<bullet>().dir = (int)-dir;
            }
        }

        float dist = (player.transform.position - transform.position).magnitude;
        if ((dist > shootDistance || dist < hideDistance))
        {
            if (!hidden)
            {
                StartCoroutine(Hide());
            }
        }
        if((dist < shootDistance && dist > hideDistance) && hidden)
        {
            StartCoroutine(Show());
        }

        //HIT
        if (gotHit)
        {
            hitTime++;
            if (hitTime % 8 == 0)
            {
                GetComponentInChildren<SpriteRenderer>().enabled ^= true;
            }
            if (hitTime > 32)
            {
                gotHit = false;
                GetComponentInChildren<SpriteRenderer>().enabled = true;
            }
            if (hitTime == 1)
            {
                GetComponent<AudioSource>().PlayOneShot(deathAC);
                lifes--;
            }
        }
        else
        {
            hitTime = 0;
        }
    }
}
