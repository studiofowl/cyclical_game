using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RaycastController))]
public class PlatformController : MonoBehaviour {

    public LayerMask passengerMask;

    public Vector3[] localWaypoints;
    Vector3[] globalWaypoints;

    public float speed;
    public bool cyclic;
    public float waitTime;
    [Range(0,2)]
    public float easeAmount;

    int fromWaypointIndex;
    float percentBetweenWaypoints;
    float nextMoveTime;

    List<PassengerMovement> passengerMovement;
    PlayerController playerScript;

    private RaycastController rc;
    // Use this for initialization
    void Start() {
        rc = GetComponent<RaycastController>();
        rc.Start();

        globalWaypoints = new Vector3[localWaypoints.Length];
        for (int i = 0; i < localWaypoints.Length; i++) {
            globalWaypoints[i] = localWaypoints[i] + transform.position;
        }
    }
	
	// Update is called once per frame
	void Update () {

        rc.UpdateRaycastOrigins();

        Vector3 velocity = CalculatePlatformMovement();

        CalculatePassengerMovement(velocity);

        //move passenger before platform when platform moving down to prevent bouncing
        MovePassengers(true);
        transform.Translate(velocity);
        //move passenger after platform when platform is going up to avoid passenger clipping down
        MovePassengers(false);

    }

    float Ease(float x) {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    Vector3 CalculatePlatformMovement() {

        if (Time.time < nextMoveTime) {
            return Vector3.zero;
        }
        fromWaypointIndex %= globalWaypoints.Length;
        int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
        float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
        percentBetweenWaypoints += Time.deltaTime * speed/distanceBetweenWaypoints;
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
        float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);

        Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

        if (percentBetweenWaypoints >= 1) {
            percentBetweenWaypoints = 0;
            fromWaypointIndex++;
            if (!cyclic) {
                if (fromWaypointIndex >= globalWaypoints.Length - 1) {
                    fromWaypointIndex = 0;
                    System.Array.Reverse(globalWaypoints);
                }
            }
            nextMoveTime = Time.time + waitTime;
        }

        return newPos - transform.position;
    }

    void MovePassengers(bool beforeMovePlatform) {
        foreach (PassengerMovement passenger in passengerMovement) {
            if (playerScript == null) {
                if (passenger.transform.GetComponent<PlayerController>()) {
                    playerScript = passenger.transform.GetComponent<PlayerController>();
                }
            else {
                    playerScript = passenger.transform.parent.parent.GetComponent<PlayerController>();
                }
            }
            if (passenger.moveBeforePlatform == beforeMovePlatform) {
                if (passenger.transform.tag == "Player" || passenger.transform.parent.parent.tag == "Player") {
                    playerScript.Move(passenger.velocity, passenger.standingOnPlatform);
                }
            }
        }
    }

    void CalculatePassengerMovement(Vector3 velocity) {
        //hashsets are apparently faster at adding things and checking if it contains things
        HashSet<Transform> movedPassengers = new HashSet<Transform>();
        passengerMovement = new List<PassengerMovement>();

        float directionX = Mathf.Sign(velocity.x);
        float directionY = Mathf.Sign(velocity.y);

        // Vertically moving platform
        if (velocity.y != 0) {
            float rayLength = Mathf.Abs(velocity.y) + rc.skinWidth;

            for (int i = 0; i < rc.verticalRayCount; i++) {
                Vector2 rayOrigin = (directionY == -1) ? rc.raycastOrigins.bottomLeft : rc.raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (rc.verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

                if (hit && hit.distance != 0) {
                    //each passenger only used one time a frame
                    if (!movedPassengers.Contains(hit.transform)) {
                        movedPassengers.Add(hit.transform);
                        //passenger only affected by X velocity if standing on platform
                        float pushX = (directionY == 1) ? velocity.x : 0;
                        //used to close the gap between object and platform and set velocity
                        float pushY = velocity.y - (hit.distance - rc.skinWidth) * directionY;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX,pushY), directionY == 1, true));
                    }
                }
            }
        }

        // Horizontally moving platform from the side
        if (velocity.x != 0) {
            float rayLength = Mathf.Abs(velocity.x) + rc.skinWidth;

            for (int i = 0; i < rc.horizontalRayCount; i++) {
                Vector2 rayOrigin = (directionY == -1) ? rc.raycastOrigins.bottomLeft : rc.raycastOrigins.bottomRight;
                rayOrigin += Vector2.right * (rc.verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);
                if (hit && hit.distance != 0) {
                    if (!movedPassengers.Contains(hit.transform)) {
                        movedPassengers.Add(hit.transform);
                        //used to close the gap between object and platform and set velocity
                        float pushX = velocity.x - (hit.distance - rc.skinWidth) * directionX;
                        //used so that the passenger checks below itself to allow for jumping
                        float pushY = -rc.skinWidth;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
                    }
                }
            }
        }

        // Passenger is on top of a horizontally or downward moving platform
        if (directionY == -1 || velocity.y == 0 && velocity.x != 0) {
            //one skinWidth to get to the surface and another to detect anything on top
            float rayLength = rc.skinWidth * 2;

            for (int i = 0; i < rc.verticalRayCount; i++) {
                Vector2 rayOrigin = rc.raycastOrigins.topLeft + Vector2.right * (rc.verticalRaySpacing * i);
                //only ever need cast ray up from topLeft corner
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

                if (hit && hit.distance != 0) {
                    if (!movedPassengers.Contains(hit.transform)) {
                        movedPassengers.Add(hit.transform);
                        float pushX = velocity.x;
                        float pushY = velocity.y;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
                    }
                }
            }
        }
    }

    struct PassengerMovement {
        public Transform transform;
        public Vector3 velocity;
        public bool standingOnPlatform;
        public bool moveBeforePlatform;

        public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform) {
            transform = _transform;
            velocity = _velocity;
            standingOnPlatform = _standingOnPlatform;
            moveBeforePlatform = _moveBeforePlatform;
        }
    }

    void OnDrawGizmos() {
        if (localWaypoints != null) {
            Gizmos.color = Color.red;
            float size = .3f;

            for (int i = 0; i < localWaypoints.Length; i ++) {
                Vector3 globalWayPointPos = (Application.isPlaying)?globalWaypoints[i] : localWaypoints[i] + transform.position;
                Gizmos.DrawLine(globalWayPointPos - Vector3.up * size, globalWayPointPos + Vector3.up * size);
                Gizmos.DrawLine(globalWayPointPos - Vector3.left * size, globalWayPointPos + Vector3.left * size);
            }
        }
    }
}
