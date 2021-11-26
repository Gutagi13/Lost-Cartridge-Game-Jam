using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    float moveSpeed = 6;

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

    void Start()
    {
        controller = GetComponent<Controller2D>();

        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);

        print("Gravity: " + gravity + "; Jump Velocity: " + maxJumpVelocity);

        Application.targetFrameRate = 60;
    }

    void Update()
    {
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


        if (Input.GetMouseButtonDown(0))
        {
            GetComponentInChildren<CapsuleCollider2D>().OverlapCollider(cF, c);
            isAttacking = true;
            frameCount = -1;
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
