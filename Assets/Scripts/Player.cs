using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    // Fix Wall Jump
    // Coyote Time
    //Camera Vertical smooth depending on velocity/dir

    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
    public float timeToJumpApex = .4f;
    public float accelerationTimeAirborne = .1f;
    public float accelerationTimeGrounded = .05f;
    public float moveSpeed = 6;

    public float wallSlideSpeedMax = 3;
    public float wallStickTime = .1f;
    float timeToWallUnstick;
    public bool wallJumpAbility;

    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;

    [HideInInspector]
    public float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;
    public Vector3 velocity;
    float velocityXSmoothing;

    Vector2 directionalInput;
    bool wallSliding;
    int wallDirX;

    Controller2D controller;

    float timeSinceGround;
    float timeSinceWall;
    public float coyoteTime;

    int dir = 1;
    public ContactFilter2D cF;
    List<Collider2D> c = new List<Collider2D>();
    public Vector2 knockback;

    public bool gotHit;
    int hitTime;
    public int stunTime;
    public int frameRate;

    public Sprite[] walkF;
    public Sprite[] attackF;
    public Sprite hitF;
    int frameCount;
    bool isAttacking;

    public AudioClip[] jumpAC;
    public AudioClip[] slashAC;
    public AudioClip deathAC;
    public Tilemap map;

    public TileBase chest;
    public TileBase coin;
    public TileBase key;
    public TileBase spikes;
    public TileBase flag;

    public int lifes = 3;
    public Sprite[] lifesS;
    public Animator transitionAnim;
    int nKeys;
    int nCoins;

    IEnumerator LoadScene()
    {
        Camera.main.GetComponent<AudioSource>().Stop();
        Camera.main.GetComponent<AudioSource>().PlayOneShot(deathAC, 1f);
        transitionAnim.SetTrigger("End");
        yield return new WaitForSeconds(4.25f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Start()
    {
        controller = GetComponent<Controller2D>();

        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);

        print("Gravity: " + gravity + "; Jump Velocity: " + maxJumpVelocity);

        Application.targetFrameRate = 60;
    }

    void CalculateTilemap()
    {
        Vector3Int pos = map.WorldToCell(transform.position+Vector3.down*0.3f);
        Vector3Int pos2 = map.WorldToCell(transform.position + Vector3.down*-0.1f);
        TileBase cTile = map.GetTile(pos);
        TileBase cTile2 = map.GetTile(pos2);
        if (cTile == chest && nKeys>=1)
        {
            nKeys--;
            lifes = 3;
            map.SetTile(pos, null);

        }
        else if (cTile == coin)
        {
            map.SetTile(pos, null);
            nCoins++;
        }
        else if (cTile == spikes && lifes>=0)
        {
            StartCoroutine(LoadScene());
            lifes = -1;
        }
        else if (cTile == key)
        {
            map.SetTile(pos, null);
            nKeys++;
        }
        else if (cTile == flag)
        {
            SceneManager.LoadScene(2);
        }

        if (cTile2 == chest && nKeys >= 1)
        {
            nKeys--;
            lifes = 3;
            map.SetTile(pos2, null);

        }
        else if (cTile2 == coin)
        {
            map.SetTile(pos2, null);
            nCoins++;
        }
        else if (cTile2 == key)
        {
            map.SetTile(pos2, null);
            nKeys++;
        }
        else if (cTile2 == flag)
        {
            SceneManager.LoadScene(2);
        }
    }

    void Update()
    {
        GameObject.Find("KeyText").GetComponent<Text>().text = nKeys.ToString();
        GameObject.Find("CoinText").GetComponent<Text>().text = nCoins.ToString();
        if (lifes==0)
        {
            StartCoroutine(LoadScene());
            lifes--;
        }
        GameObject.Find("Health").GetComponent<SpriteRenderer>().sprite = lifesS[Mathf.Max(lifes-1,0)];
        CalculateTilemap();
        CalculateVelocity();
        if (wallJumpAbility)
        {
            HandleWallSliding();
        }

        controller.Move(velocity * Time.deltaTime, directionalInput);
        if (directionalInput.x != 0)
        {
            dir = (int)Mathf.Sign(directionalInput.x);
        }
        transform.localScale = new Vector3(dir, 1, 1);

        if (controller.collisions.above || controller.collisions.below)
        {
            if (!controller.collisions.slidingDownMaxSlope)
            {
                velocity.y = 0;
                //velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
            }
            
        }

        timeSinceGround = controller.collisions.below ? 0 : timeSinceGround +Time.deltaTime;
        timeSinceWall = wallSliding ? 0 : timeSinceWall + Time.deltaTime;

        frameCount++;
        int frame = Mathf.RoundToInt((float)frameCount / 30 * frameRate);
        Sprite currentFrame = walkF[frame % walkF.Length];
        if (directionalInput.x==0)
        {
            currentFrame = walkF[1];

        }
        if(isAttacking)
        {
            currentFrame = attackF[frame % attackF.Length];
            if (frame == 5)
            {
                isAttacking = false;
            }
        }


        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            GetComponentInChildren<CapsuleCollider2D>().OverlapCollider(cF, c);
            isAttacking = true;
            frameCount = -1;
            gameObject.GetComponent<AudioSource>().PlayOneShot(slashAC[Mathf.FloorToInt(Random.Range(0, slashAC.Length))]);
            foreach (Collider2D col in c)
            {
                if (col.gameObject.TryGetComponent(out CombatSystem combat))
                {
                    combat.hit((int)Mathf.Sign(col.transform.position.x - transform.position.x), knockback);
                }
            }
        }

        GetComponentInChildren<SpriteRenderer>().sprite = currentFrame;

        if (gotHit)
        {
            hitTime++;
            if (hitTime % frameRate == 0)
            {
                GetComponentInChildren<SpriteRenderer>().enabled ^= true;
            }
            if (hitTime > stunTime)
            {
                gotHit = false;
                GetComponentInChildren<SpriteRenderer>().enabled = true;
            }
            if (hitTime == 1)
            {
                lifes--;
            }
        }
        else
        {
            hitTime = 0;
        }
    }

    public void SetDirectionalInput(Vector2 input)
    {
        directionalInput = input;
    }

    public void OnJumpInputDown()
    {
        if (timeSinceWall <= coyoteTime)
        {
            if (wallDirX == directionalInput.x)
            {
                velocity.x = -wallDirX * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            }
            else if (directionalInput.x == 0)
            {
                velocity.x = -wallDirX * wallJumpOff.x;
                velocity.y = wallJumpOff.y;
            }
            else
            {
                velocity.x = -wallDirX * wallLeap.x;
                velocity.y = wallLeap.y;
            }
            timeSinceWall += 1+coyoteTime;
        }
        if (timeSinceGround <= coyoteTime)
        {

            velocity.y = maxJumpVelocity;
            timeSinceGround += 1 + coyoteTime;
            //gameObject.GetComponent<AudioSource>().PlayOneShot(jumpAC[Mathf.FloorToInt(Random.Range(0,jumpAC.Length))]);
            gameObject.GetComponent<AudioSource>().PlayOneShot(jumpAC[2]);
        }
    }

    public void OnJumpInputUp()
    {
        if (velocity.y > minJumpVelocity)
        {
            velocity.y = minJumpVelocity;
        }
    }

    void HandleWallSliding()
    {
        wallDirX = (controller.collisions.left) ? -1 : 1;
        wallSliding = false;
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0)
        {
            wallSliding = true;
            if (velocity.y < -wallSlideSpeedMax)
            {
                velocity.y = -wallSlideSpeedMax;
            }

            if (timeToWallUnstick > 0)
            {
                if (directionalInput.x != wallDirX && directionalInput.x != 0)
                {
                    velocityXSmoothing = 0;
                    velocity.x = 0;
                    timeToWallUnstick -= Time.deltaTime;
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }
            }
            else
            {
                timeToWallUnstick = wallStickTime;
            }
        }
    }

    void  CalculateVelocity()
    {
        float targetvelocityX = directionalInput.x * moveSpeed;
        if (!gotHit)
        {
            velocity.x = Mathf.SmoothDamp(velocity.x, targetvelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        }
        else if (controller.collisions.below && velocity.y <= 0)
        {
            velocity.x = 0;
        }
        velocity.y += gravity * Time.deltaTime;
    }


}
