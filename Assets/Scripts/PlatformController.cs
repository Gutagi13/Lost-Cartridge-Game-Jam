using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RaycastController
{

    public LayerMask passangerMask;

    public Vector3[] localWaypoints;
    Vector3[] globalWaypoints;

    public float speed;
    public bool cyclic;
    public float waitTime;
    [Range(0,2)]
    public float easeAmount;

    int fromWaypointIndex;
    float percentBetweenWaypoints;
    float nextmoveTime;

    List<PassagerMovement> passangerMovement;
    Dictionary<Transform, Controller2D> passagerDictionary = new Dictionary<Transform, Controller2D>();

    public override void Start()
    {
        base.Start();

        globalWaypoints = new Vector3[localWaypoints.Length];
        for (int i = 0; i<localWaypoints.Length;i++)
        {
            globalWaypoints[i] = localWaypoints[i] + transform.position;
        }
    }

    void Update()
    {
        UpdateRaycastOrigins();
        Vector3 velocity = CalculatePlatformMove();
        CalculatePassagerMovement(velocity);
        MovePassagers(true);
        transform.Translate(velocity);
        MovePassagers(false);
    }

    float Ease(float x)
    {
        float a = easeAmount+1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x,a)+Mathf.Pow(1-x,a));
    }

    Vector3 CalculatePlatformMove()
    {
        if (Time.time<nextmoveTime)
        {
            return Vector3.zero;
        }
        fromWaypointIndex %= globalWaypoints.Length;
        int toWaypointindex = (fromWaypointIndex + 1)% globalWaypoints.Length;
        float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex],globalWaypoints[toWaypointindex]);
        percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
        float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);

        Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex],globalWaypoints[toWaypointindex],easedPercentBetweenWaypoints);
        if (percentBetweenWaypoints >=1)
        {
            percentBetweenWaypoints = 0;
            fromWaypointIndex++;
            if (!cyclic)
            {
                if (fromWaypointIndex >= globalWaypoints.Length - 1)
                {
                    fromWaypointIndex = 0;
                    System.Array.Reverse(globalWaypoints);
                }
            }
            nextmoveTime = Time.time + waitTime;

        }
        return newPos - transform.position;
    }

    void MovePassagers(bool beforeMovePlatform)
    {
        foreach(PassagerMovement passager in passangerMovement)
        {
            if (!passagerDictionary.ContainsKey(passager.transform))
            {
                passagerDictionary.Add(passager.transform, passager.transform.GetComponent<Controller2D>());
            }
            if (passager.moveBeforePlatform==beforeMovePlatform)
            {
               passagerDictionary[passager.transform].Move(passager.velocity,passager.standingOnPlatform);
            }
        }
    }

    void CalculatePassagerMovement(Vector3 velocity)
    {
        HashSet<Transform> movedPassanger = new HashSet<Transform>();
        passangerMovement = new List<PassagerMovement>();

        float directionX = Mathf.Sign(velocity.x);
        float directionY = Mathf.Sign(velocity.y);

        // Vertically moving platform
        if (velocity.y != 0)
        {
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passangerMask);
                if (hit && hit.distance!=0)
                {
                    if (!movedPassanger.Contains(hit.transform))
                    {
                        movedPassanger.Add(hit.transform);
                        float pushY = velocity.y - (hit.distance - skinWidth) * directionY;
                        float pushX = (directionY == 1) ? velocity.x : 0;
                        passangerMovement.Add(new PassagerMovement(hit.transform, new Vector3(pushX,pushY),directionY==1,true));
                    }
                   
                }

            }
        }

        //Horizontally moving platforms
        if (velocity.x != 0)
        {
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;

            for (int i = 0; i < horizontalRayCount; i++)
            {
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passangerMask);
                
                if (hit && hit.distance != 0)
                {
                    if (!movedPassanger.Contains(hit.transform))
                    {
                        movedPassanger.Add(hit.transform);
                        float pushY = -skinWidth/10f;
                        float pushX = velocity.x - (hit.distance - skinWidth) * directionX;
                        passangerMovement.Add(new PassagerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
                    }

                }
            }
        }

        //Passanger on top of a horizontally or downard moving platform
        if (directionY==-1 || velocity.y == 0 && velocity.x !=0)
        {
            float rayLength =skinWidth*2;

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = raycastOrigins.topLeft+Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passangerMask);
                if (hit && hit.distance != 0)
                {
                    if (!movedPassanger.Contains(hit.transform))
                    {
                        movedPassanger.Add(hit.transform);
                        float pushY = velocity.y;
                        float pushX = velocity.x;
                        passangerMovement.Add(new PassagerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
                    }

                }

            }
        }
    }

    struct PassagerMovement
    {
        public Transform transform;
        public Vector3 velocity;

        public bool standingOnPlatform;
        public bool moveBeforePlatform;

        public PassagerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform)
        {
            transform = _transform;
            velocity = _velocity;
            standingOnPlatform = _standingOnPlatform;
            moveBeforePlatform = _moveBeforePlatform;
        }
    }

    void OnDrawGizmos()
    {
        if (localWaypoints!=null)
        {
            Gizmos.color = Color.red;
            float size = .3f;
            for (int i = 0; i< localWaypoints.Length; i++)
            {
                Vector3 globalWaypointPos = (Application.isPlaying)?globalWaypoints[i]:localWaypoints[i] + transform.position;
                Gizmos.DrawLine(globalWaypointPos-Vector3.up*size,globalWaypointPos+Vector3.up*size);
                Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
            }
        }
    }
}
