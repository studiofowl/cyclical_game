using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RaycastController))]
public class PlayerController : MonoBehaviour {
    public LayerMask collisionMask, rollCollisionMask;
    private LayerMask justEnemy, defaultCollisionMask;

    private BoxCollider2D swordHitBox;

    internal Vector2 playerInput;
    internal CollisionsController genericController;
    internal playerCollisionInfo myCollisions;
    internal PlayerEdgeGrab edgeGrabScript;
    private Player mainScript;

    internal RaycastController rc1, rc2, rc5;

    void Start() {
        myCollisions.Init();
        justEnemy = LayerMask.GetMask("Enemy");
        defaultCollisionMask = collisionMask;
        swordHitBox = transform.GetChild(0).GetChild(3).GetComponent<BoxCollider2D>();
        genericController = GetComponent<CollisionsController>();
        edgeGrabScript = GetComponent<PlayerEdgeGrab>();
        mainScript = GetComponent<Player>();
        rc1 = GetComponent<RaycastController>();
        rc1.Start();
        rc2 = transform.GetChild(0).GetChild(1).GetComponent<RaycastController>();
        rc2.Start();
        rc5 = transform.GetChild(0).GetChild(2).GetComponent<RaycastController>();
        rc5.Start();
        genericController.collisions.faceDir = 1;
    }

    public void Move(Vector2 moveAmount, bool standingOnPlatform) {
        Move(moveAmount, Vector2.zero, standingOnPlatform);
    }
    // 8 and 12
    public void DecideWhichCollidersAreActive() {
        if (myCollisions.isDucking) {
            if (rc1.CheckLayer() == 8) {
                rc1.ChangeCollLayer(12);
            }
            if (rc2.CheckLayer() == 8) {
                rc2.ChangeCollLayer(12);
            }
            if (rc5.CheckLayer() == 12) {
                rc5.ChangeCollLayer(8);
            }
            return;
        }
        if (rc1.CheckLayer() == 12) {
            rc1.ChangeCollLayer(8);
        }
        if (myCollisions.hasArmsOut && rc2.CheckLayer() == 12) {
            rc2.ChangeCollLayer(8);
        }
        else if (!myCollisions.hasArmsOut && rc2.CheckLayer() == 8) {
            rc2.ChangeCollLayer(12);
        }
    }

    public void Move(Vector2 moveAmount, Vector2 input, bool standingOnPlatform = false) {

        DecideWhichCollidersAreActive();
        rc1.UpdateRaycastOrigins();
        if (myCollisions.hasArmsOut) rc2.UpdateRaycastOrigins();
        if (myCollisions.isDucking) rc5.UpdateRaycastOrigins();
        genericController.collisions.Reset();
        //used to reset moveAmount when going from descending (which decreases the x move) to climbing
        genericController.collisions.moveAmountOld = moveAmount;
        playerInput = input;

        if (moveAmount.y < 0) {
            genericController.DescendSlope(ref moveAmount, rc1, collisionMask);
        }

        if (moveAmount.x != 0) {
            if (input.x <= 0 && myCollisions.cameFromLeftFacingDuck) {
                genericController.collisions.faceDir = -1;
                myCollisions.cameFromLeftFacingDuck = false;
            }
            else if (myCollisions.cameFromLeftFacingDuck) {
                genericController.collisions.faceDir = (int)Mathf.Sign(moveAmount.x);
                myCollisions.cameFromLeftFacingDuck = false;

            }
            else {
                genericController.collisions.faceDir = (int)Mathf.Sign(moveAmount.x);
            }
            mainScript.DecideIfFlipSprite(moveAmount.x);
        }

        if (moveAmount.x != 0) {
            if (!myCollisions.isDucking) HorizontalCollisions(ref moveAmount, rc1);
            if (myCollisions.hasArmsOut) HorizontalCollisions(ref moveAmount, rc2);
            if (myCollisions.isDucking) HorizontalCollisions(ref moveAmount, rc5);
        }
        if (moveAmount.y != 0) {
            if (!myCollisions.isDucking) VerticalCollisions(ref moveAmount, rc1);
            if (myCollisions.hasArmsOut) VerticalCollisions(ref moveAmount, rc2);
            if (myCollisions.isDucking) VerticalCollisions(ref moveAmount, rc5);
            CheckForGround(moveAmount);
        }

        if (myCollisions.isDucking) CheckForRoomToStopDuck(moveAmount);

        if (myCollisions.hasArmsOut) UnderArmCheck(moveAmount);
        if (myCollisions.hasArmsOut && genericController.collisions.grounded) edgeGrabScript.TakeOutOrPutAwayArms(false);
        // could use myCollisions.delayAfterEdgeJump
        if (!myCollisions.isDucking && !genericController.collisions.grounded) CheckForEdge(moveAmount);

        transform.Translate(moveAmount);

        //used to allow jumping when platform is moving up with passenger on it
        if (standingOnPlatform) {
            genericController.collisions.below = true;
        }
    }

    public void SwitchCollisionMask(bool switchFromDefault) {
        if (switchFromDefault) {
            collisionMask = rollCollisionMask;
        }
        else {
            collisionMask = defaultCollisionMask;
        }
    }

    public void CheckForEnemy() {
        Bounds hbBounds = swordHitBox.bounds;
        Vector2 bottomLeft = new Vector2(hbBounds.min.x, hbBounds.min.y);
        Vector2 bottomRight = new Vector2(hbBounds.max.x, hbBounds.min.y);
        Vector2 topLeft = new Vector2(hbBounds.min.x, hbBounds.max.y);
        Vector2 topRight = new Vector2(hbBounds.max.x, hbBounds.max.y);

        Vector2 hits1Direction = bottomRight - topLeft;
        float hits1Distance = Vector2.Distance(topLeft, bottomRight);

        Vector2 hits2Direction = bottomLeft - topRight;
        float hits2Distance = Vector2.Distance(topRight, bottomLeft);

        RaycastHit2D[] hits1 = Physics2D.RaycastAll(topLeft, hits1Direction, hits1Distance, justEnemy);
        RaycastHit2D[] hits2 = Physics2D.RaycastAll(topRight, hits2Direction, hits2Distance, justEnemy);
        Debug.DrawRay(topLeft, hits1Direction, Color.red);
        Debug.DrawRay(topRight, hits2Direction, Color.red);

        if (hits1.Length > 0) {
            for (int i = 0; i < hits1.Length; i++) {
                GenericEnemy enemyScript = hits1[i].collider.gameObject.GetComponent<GenericEnemy>();
                enemyScript.lastHitDamageAmount = 1;
                enemyScript.isHit = true;
            }
        }
        else if (hits2.Length > 0) {
            for (int i = 0; i < hits2.Length; i++) {
                GenericEnemy enemyScript = hits2[i].collider.gameObject.GetComponent<GenericEnemy>();
                enemyScript.lastHitDamageAmount = 1;
                enemyScript.isHit = true;
            }
        }
    }

    void CheckForRoomToStopDuck(Vector2 moveAmount) {
        float rayLength = rc1.raycastOrigins.topLeft.y - rc5.raycastOrigins.topLeft.y;

        bool rayHitObject = false;

        for (int i = 0; i < rc5.verticalRayCount; i++) {
            Vector2 rayOrigin = rc5.raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (rc5.verticalRaySpacing * i + moveAmount.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * rayLength, Color.blue);

            if (hit) {
                rayHitObject = true;
            }
        }
        myCollisions.roomToStopDucking = !rayHitObject;
    }

    void CheckForEdge(Vector2 moveAmount) {
        if (ArmRayCast(true, true, moveAmount) || ArmRayCast(false, true, moveAmount)) {
            if (myCollisions.hasArmsOut) {
                edgeGrabScript.TakeOutOrPutAwayArms(false);
            }
            return;
        }
        float directionX = genericController.collisions.faceDir;
        float rayLength = rc2.coll.bounds.size.x + rc2.skinWidth;
        int cutOffRayForEdgeGrab = Mathf.RoundToInt(0.5f * (rc1.horizontalRayCount-1));
        //bool rayHitAboveCutOff = false;
        bool rayHitObstacleBelowCutOff = false;

        for (int i = 0; i < rc1.horizontalRayCount; i++) {
            Vector2 rayOrigin = (directionX == -1) ? rc1.raycastOrigins.bottomLeft : rc1.raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (rc1.horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.magenta);
            if (hit) {
                /*if (i >= cutOffRayForEdgeGrab && !rayHitAboveCutOff) {
                    rayHitAboveCutOff = true;
                }*/
                if (i < cutOffRayForEdgeGrab && hit.collider.gameObject.layer == 9 && !rayHitObstacleBelowCutOff) {
                    if (Mathf.RoundToInt(Vector2.Angle(hit.normal, Vector2.up)) == 90) {
                        rayHitObstacleBelowCutOff = true;
                    }
                }
            }
        }
        if (myCollisions.hasArmsOut && !rayHitObstacleBelowCutOff) {
            edgeGrabScript.TakeOutOrPutAwayArms(false);
        }
        else if (rayHitObstacleBelowCutOff && !myCollisions.hasArmsOut) {
            edgeGrabScript.TakeOutOrPutAwayArms(true);
        }
    }
    public RaycastHit2D ArmRayCast(bool upper, bool forward, Vector2 moveAmount) {
        float collWidth = (rc2.coll.bounds.size.x + rc2.skinWidth);
        float collHeight = (rc2.coll.bounds.size.y - rc2.skinWidth);
        float playerWidth = rc1.coll.bounds.size.x;
        Transform arms = rc2.transform;
        Vector2 location = new Vector2(transform.position.x, arms.position.y);
        int rayDir = genericController.collisions.faceDir;
        if (forward) {
            location.x += (((playerWidth / 2) - rc2.skinWidth) * genericController.collisions.faceDir);
        }
        else {
            location.x -= (((playerWidth / 2) - rc2.skinWidth) * genericController.collisions.faceDir);
            rayDir *= -1;
        }
        if (upper) {
            location.y += ((collHeight / 2) + moveAmount.y);
        }
        else {
            location.y -= ((collHeight / 2) - moveAmount.y);
        }
        if (forward) Debug.DrawRay(location, Vector2.right * rayDir * collWidth, Color.yellow);
        else Debug.DrawRay(location, Vector2.right * rayDir * collWidth, Color.green);
        return Physics2D.Raycast(location, Vector2.right * rayDir, collWidth, collisionMask);
    }

    void HorizontalCollisions(ref Vector2 moveAmount, RaycastController rc) {
        genericController.otherCollider = null;

        float directionX = genericController.collisions.faceDir;
        float rayLength = Mathf.Abs(moveAmount.x) + rc.skinWidth;

        bool rollHitObstacle = false;

        if (Mathf.Abs(moveAmount.x) < rc.skinWidth) {
            rayLength = 2 * rc.skinWidth;
        }

        for (int i = 0; i < rc.horizontalRayCount; i++) {
            Vector2 rayOrigin = (directionX == -1) ? rc.raycastOrigins.bottomLeft : rc.raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (rc.horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (hit) {

                if (genericController.otherCollider == null && hit.collider.tag == "Pushable") {
                    genericController.otherCollider = hit.collider;
                }

                if (!rollHitObstacle && Vector2.Angle(hit.normal, Vector2.up) >= genericController.maxSlopeAngle && hit.collider.gameObject.layer == 9 && myCollisions.isRolling) {
                    rollHitObstacle = true;
                }

                genericController.DefaultHorizontalCollisionLogic(hit, ref moveAmount, i, rc, directionX, ref rayLength);
            }
        }
        if (rollHitObstacle) {
            //Debug.Log("hit obstacle so stop rolling");
            //myCollisions.isRolling = false;
        }
    }

    void UnderArmCheck(Vector2 moveAmount) {
        float rayLength = rc2.skinWidth * 2;

        bool hitObstacle = false;

        for (int i = 0; i < rc2.verticalRayCount; i++) {
            Vector2 rayOrigin = rc2.raycastOrigins.bottomLeft;
            rayOrigin += Vector2.right * (rc2.verticalRaySpacing * i + moveAmount.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, -Vector2.up * rayLength, Color.cyan);

            if (hit) {
                if (hit.collider.gameObject.layer == 9) hitObstacle = true;
            }
        }
        if (hitObstacle) {
            edgeGrabScript.RemoveSword();
        }
        myCollisions.underArm = hitObstacle;
    }

    void VerticalCollisions(ref Vector2 moveAmount, RaycastController rc) {
        float directionY = Mathf.Sign(moveAmount.y);
        float rayLength = Mathf.Abs(moveAmount.y) + rc.skinWidth;

        bool rollHitObstacle = false;

        for (int i = 0; i < rc.verticalRayCount; i++) {
            Vector2 rayOrigin = (directionY == -1) ? rc.raycastOrigins.bottomLeft : rc.raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (rc.verticalRaySpacing * i + moveAmount.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            if (hit) {
                if (hit.collider.tag == "Through") {
                    if (directionY == 1 || hit.distance == 0) {
                        continue;
                    }
                    if (myCollisions.fallingThroughPlatform) {
                        continue;
                    }
                    if (playerInput.y == -1) {
                        myCollisions.fallingThroughPlatform = true;
                        Invoke("ResetFallingThroughPlatform", .5f);
                        continue;
                    }
                }
                if (!rollHitObstacle && directionY > 0 && hit.collider.gameObject.layer == 9 && myCollisions.isRolling) {
                    rollHitObstacle = true;
                }
                genericController.DefaultVerticalCollisionLogic(hit, ref moveAmount, rc, directionY, ref rayLength);
            }
        }
        if (rollHitObstacle) {
            //Debug.Log("hit obstacle so stop rolling");
            //myCollisions.isRolling = false;
        }
        genericController.DefaultVerticalCollisionsSlopeLogic(ref moveAmount, rc, ref rayLength, collisionMask);
    }

    public void ResetFallingThroughPlatform() {
        myCollisions.fallingThroughPlatform = false;
    }

    void CheckForGround(Vector2 moveAmount) {

        Vector2 topLeftPoint = new Vector2((rc1.raycastOrigins.bottomLeft.x + rc1.skinWidth * 2) + moveAmount.x, rc1.raycastOrigins.bottomLeft.y - rc1.skinWidth + moveAmount.y);
        Vector2 bottomRightPoint = new Vector2((rc1.raycastOrigins.bottomRight.x - rc1.skinWidth * 2) + moveAmount.x, rc1.raycastOrigins.bottomRight.y - (rc1.skinWidth * 2) + moveAmount.y);

        genericController.collisions.grounded = Physics2D.OverlapArea(topLeftPoint, bottomRightPoint, collisionMask);
        Debug.DrawLine(topLeftPoint, bottomRightPoint, Color.green);
    }

    public struct playerCollisionInfo {
        public bool hasArmsOut;
        public bool underArm;
        public bool delayAfterEdgeJump;

        public bool isRolling;
        public bool isDucking;
        public bool roomToStopDucking;
        public bool cameFromLeftFacingDuck;

        public bool swordOut;
        public bool swordSwinging;

        public bool fallingThroughPlatform;
        public void Init() {
            roomToStopDucking = true;
        }
    }
}