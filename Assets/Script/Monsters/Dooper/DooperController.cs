using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RaycastController))]
public class DooperController : MonoBehaviour {

    public LayerMask collisionMask;
    private LayerMask justPlayer;
    private Player skapScript;

    internal CollisionsController genericController;
    internal Dooper mainScript;
    internal dooperCollisionInfo myCollisions;

    private RaycastController rc1;
    private BoxCollider2D kickHitBox;

    void Start() {
        myCollisions.Init();
        justPlayer = LayerMask.GetMask("Player");
        genericController = GetComponent<CollisionsController>();
        mainScript = GetComponent<Dooper>();
        rc1 = GetComponent<RaycastController>();
        rc1.Start();
        kickHitBox = transform.GetChild(0).GetChild(1).GetComponent<BoxCollider2D>();
        if (FindObjectOfType<Player>()) {
            skapScript = FindObjectOfType<Player>().GetComponent<Player>();
        }
        genericController.collisions.faceDir = 1;
    }

    public void Move(Vector2 moveAmount, bool standingOnPlatform) {
        Move(moveAmount, Vector2.zero, standingOnPlatform);
    }

    public void Move(Vector2 moveAmount, Vector2 input, bool standingOnPlatform = false) {
        rc1.UpdateRaycastOrigins();
        genericController.collisions.Reset();
        //used to reset moveAmount when going from descending (which decreases the x move) to climbing
        genericController.collisions.moveAmountOld = moveAmount;

        if (moveAmount.y < 0) {
            genericController.DescendSlope(ref moveAmount, rc1, collisionMask);
        }
        if (moveAmount.x != 0) {
            genericController.collisions.faceDir = (int)Mathf.Sign(moveAmount.x);
        }

        if (moveAmount.x != 0) {
            HorizontalCollisions(ref moveAmount, rc1);
        }

        if (moveAmount.y != 0) {
            VerticalCollisions(ref moveAmount, rc1);
        }
        //Debug.Log("moveAmount.x: " + moveAmount.x);

        transform.Translate(moveAmount);

        //used to allow jumping when platform is moving up with passenger on it
        if (standingOnPlatform) {
            genericController.collisions.below = true;
        }
    }

    public void CheckForPlayer() {
        Bounds hbBounds = kickHitBox.bounds;
        Vector2 bottomLeft = new Vector2(hbBounds.min.x, hbBounds.min.y);
        Vector2 bottomRight = new Vector2(hbBounds.max.x, hbBounds.min.y);
        Vector2 topLeft = new Vector2(hbBounds.min.x, hbBounds.max.y);
        Vector2 topRight = new Vector2(hbBounds.max.x, hbBounds.max.y);

        RaycastHit2D hit1 = Physics2D.Linecast(topLeft, bottomRight, justPlayer);
        RaycastHit2D hit2 = Physics2D.Linecast(topRight, bottomLeft, justPlayer);
        Debug.DrawLine(topLeft, bottomRight, Color.red);
        Debug.DrawLine(topRight, bottomLeft, Color.red);

        if (hit1 || hit2) {
            skapScript.PlayerHit(mainScript.damage);
        }
    }

    void HorizontalCollisions(ref Vector2 moveAmount, RaycastController rc) {

        float directionX = genericController.collisions.faceDir;
        float rayLength = Mathf.Abs(moveAmount.x) + rc.skinWidth;

        bool hitObstacle = false;

        if (Mathf.Abs(moveAmount.x) < rc.skinWidth) {
            rayLength = 2 * rc.skinWidth;
        }

        for (int i = 0; i < rc.horizontalRayCount; i++) {
            Vector2 rayOrigin = (directionX == -1) ? rc.raycastOrigins.bottomLeft : rc.raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (rc.horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (hit) {
                if (!hitObstacle && Vector2.Angle(hit.normal, Vector2.up) >= genericController.maxSlopeAngle && hit.collider.gameObject.layer == 9) {
                    hitObstacle = true;
                }
                genericController.DefaultHorizontalCollisionLogic(hit, ref moveAmount, i, rc, directionX, ref rayLength);
            }
        }
        if (hitObstacle && !myCollisions.isKicking) {
            if (!myCollisions.ableToAttack && !myCollisions.isKicking) myCollisions.ableToAttack = true;
            mainScript.SwapDirections(true);
        }
    }

    void VerticalCollisions(ref Vector2 moveAmount, RaycastController rc) {
        float directionY = Mathf.Sign(moveAmount.y);
        float rayLength = Mathf.Abs(moveAmount.y) + rc.skinWidth;

        int hitObstacleCount = 0;


        for (int i = 0; i < rc.verticalRayCount; i++) {
            Vector2 rayOrigin = (directionY == -1) ? rc.raycastOrigins.bottomLeft : rc.raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (rc.verticalRaySpacing * i + moveAmount.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            if (hit) {
                if (hit.collider.gameObject.layer == 9 && directionY == -1) {
                    hitObstacleCount++;
                }
                genericController.DefaultVerticalCollisionLogic(hit, ref moveAmount, rc, directionY, ref rayLength);
            }
        }
        genericController.DefaultVerticalCollisionsSlopeLogic(ref moveAmount, rc, ref rayLength, collisionMask);
        if (directionY == -1) genericController.collisions.grounded = hitObstacleCount > 0;
    }

    public bool CheckAreaInFrontOfDooper (float length) {

        float offset = rc1.skinWidth;
        float dir = genericController.collisions.faceDir;
        length *= dir;

        Vector2 topLeftPoint = (dir == 1) ? new Vector2(rc1.raycastOrigins.topRight.x - offset, rc1.raycastOrigins.topRight.y - offset) : new Vector2(rc1.raycastOrigins.topLeft.x + offset, rc1.raycastOrigins.topLeft.y - offset);
        Vector2 bottomRightPoint = (dir == 1) ? new Vector2 (rc1.raycastOrigins.bottomRight.x + length, rc1.raycastOrigins.bottomRight.y - offset) : new Vector2(rc1.raycastOrigins.bottomLeft.x + length, rc1.raycastOrigins.bottomLeft.y + offset);

        Debug.DrawLine(topLeftPoint, bottomRightPoint, Color.magenta);
        return Physics2D.OverlapArea(topLeftPoint, bottomRightPoint, 1 << LayerMask.NameToLayer("Player"));
    }

    public struct dooperCollisionInfo {
        public bool isWalking;
        public bool isKicking;
        public bool ableToAttack;

        public void Init() {
            isWalking = true;
        }
    }
}