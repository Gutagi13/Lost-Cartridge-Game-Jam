using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinAI : MonoBehaviour
{
    Controller2D controller;

    Vector3 velocity;
    Vector2 directionalInput;

    public float accelerationTimeGrounded;
    public float accelerationTimeAirborne;
    public float moveSpeed;
    float gravity;
    float velocityXSmoothing;

    public LayerMask obstacles;
    public Transform groundDetection;
    public float distance;

    public Sprite[] walkF;
    public Sprite[] attackF;
    public Sprite hitF;
    public Sprite dieF;
    public int frameRate;
    int frameCount;
    bool isAttacking;

    public Transform player;
    public float attackDistance;
    public float attackTime;

    void Start()
    {
        //Setup
        controller = GetComponent<Controller2D>();
        directionalInput = Vector2.right;
        gravity = -50;
    }

    void Update()
    {
        //Change Direction
        RaycastHit2D groundInfo = Physics2D.Raycast(groundDetection.position, Vector2.down, distance, obstacles);
        if (directionalInput.x>0 ? controller.collisions.right : controller.collisions.left || groundInfo.collider==false)
        {
            directionalInput *= new Vector2(-1,1);
        }

        //Update Velocity
        CalculateVelocity();
        if (!isAttacking)
        {
            controller.Move(velocity * Time.deltaTime, directionalInput);
        }

        //Check grounded
        if (controller.collisions.above || controller.collisions.below)
        {
            if (!controller.collisions.slidingDownMaxSlope)
            {
                velocity.y = 0;
                //velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
            }

        }

        //Change dir scale
        transform.localScale = new Vector3(isAttacking ? Mathf.Sign(player.position.x- transform.position.x) : directionalInput.x, 1, 1);

        //Animations:
        frameCount++;
        int frame = Mathf.RoundToInt((float)frameCount / 30 * frameRate);
        Sprite currentFrame = walkF[frame % walkF.Length];
        if ((transform.position - player.position).magnitude < attackDistance)
        {
            if (!isAttacking)
            {
                frame = 0;
                frameCount = 0;
            }
            currentFrame = attackF[frame % attackF.Length];
            isAttacking = true;
        }
        else
        {
            isAttacking = false;
        }
        GetComponentInChildren<SpriteRenderer>().sprite = currentFrame;
    }

    //Calculate velocity
    void CalculateVelocity()
    {
        float targetvelocityX = directionalInput.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetvelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
    }
}
