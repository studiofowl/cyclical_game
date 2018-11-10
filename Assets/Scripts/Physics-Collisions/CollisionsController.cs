using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RaycastController))]
public class CollisionsController : MonoBehaviour {

    public float maxSlopeAngle = 80;

    public genericCollisionInfo collisions;

    internal Collider2D otherCollider;


    public void DefaultHorizontalCollisionLogic(RaycastHit2D hit, ref Vector2 moveAmount, int i, RaycastController rc, float directionX, ref float rayLength) {

        //used to skip to the next ray when ray is inside collider to determine collisions (for moving platforms)
        if (hit.distance == 0) {
            return;
        }

        float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

        if (i == 0 && slopeAngle <= maxSlopeAngle) {
            //used to reset moveAmount when going from descending (which decreases the x move) to climbing and to say that object is not descending
            if (collisions.descendingSlope) {
                collisions.descendingSlope = false;
                moveAmount = collisions.moveAmountOld;
            }
            //used to make sure player sticks to a new slope when first hit
            float distanceToSlopeStart = 0;
            //if ray hit a different slope than last frame
            if (slopeAngle != collisions.slopeAngleOld) {
                //calculate horizontal distance to new slope
                distanceToSlopeStart = hit.distance - rc.skinWidth;
                //not sure lol
                moveAmount.x -= distanceToSlopeStart * directionX;
            }
            ClimbSlope(ref moveAmount, slopeAngle, hit.normal);
            //not sure lol
            moveAmount.x += distanceToSlopeStart * directionX;
        }

        //only check the rest of the rays for collisions if not climbing / climbing max slope
        if (!collisions.climbingSlope || slopeAngle > maxSlopeAngle) {
            //normal horizontal adjustment
            moveAmount.x = (hit.distance - rc.skinWidth) * directionX;

            rayLength = hit.distance;

            //if player was climbing slope and is running into maxSlope (x is changed so y needs to be recalculated)
            if (collisions.climbingSlope) {
                //basic trig to avoid bouncing
                moveAmount.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x);
            }

            collisions.left = directionX == -1;
            collisions.right = directionX == 1;
        }
    }

    public void DefaultVerticalCollisionLogic(RaycastHit2D hit, ref Vector2 moveAmount, RaycastController rc, float directionY, ref float rayLength) {
        //move object vertically to where ray hit collider accounting for skinWidth
        moveAmount.y = (hit.distance - rc.skinWidth) * directionY;
        //sets the rayLength based on the current hit so that it can't clip through if another hit detects a lower object
        rayLength = hit.distance;

        if (collisions.climbingSlope) {
            //basic trig to recalculate x amount because velocity y is reduced
            moveAmount.x = moveAmount.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(moveAmount.x);
        }

        collisions.below = directionY == -1;
        collisions.above = directionY == 1;
    }

    public void DefaultVerticalCollisionsSlopeLogic(ref Vector2 moveAmount, RaycastController rc, ref float rayLength, LayerMask collisionMask) {
        //used when transitioning between slopes to adjust
        if (collisions.climbingSlope) {
            float directionX = Mathf.Sign(moveAmount.x);
            rayLength = Mathf.Abs(moveAmount.x) + rc.skinWidth;
            Vector2 rayOrigin = ((directionX == -1) ? rc.raycastOrigins.bottomLeft : rc.raycastOrigins.bottomRight) + Vector2.up * moveAmount.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit) {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                //if current slope does not equal slope from last frame
                if (slopeAngle != collisions.slopeAngle) {
                    //normal horizontal adjustment
                    moveAmount.x = (hit.distance - rc.skinWidth) * directionX;
                    collisions.slopeAngle = slopeAngle;
                    collisions.slopeNormal = hit.normal;
                }
            }
        }
    }

    void ClimbSlope(ref Vector2 moveAmount, float slopeAngle, Vector2 slopeNormal) {
        float moveDistance = Mathf.Abs(moveAmount.x);
        float climbmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
        if (moveAmount.y <= climbmoveAmountY) {
            moveAmount.y = climbmoveAmountY;
            moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
            collisions.below = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
            collisions.slopeNormal = slopeNormal;
        }
    }

    public void DescendSlope(ref Vector2 moveAmount, RaycastController rc, LayerMask collisionMask) {

        RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(rc.raycastOrigins.bottomLeft, Vector2.down, Mathf.Abs(moveAmount.y) + rc.skinWidth, collisionMask);
        RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(rc.raycastOrigins.bottomRight, Vector2.down, Mathf.Abs(moveAmount.y) + rc.skinWidth, collisionMask);
        if (maxSlopeHitLeft ^ maxSlopeHitRight) {
            SlideDownMaxSlope(maxSlopeHitLeft, ref moveAmount);
            SlideDownMaxSlope(maxSlopeHitRight, ref moveAmount);
        }

        if (!collisions.slidingDownMaxSlope) {
            float directionX = Mathf.Sign(moveAmount.x);
            Vector2 rayOrigin = (directionX == -1) ? rc.raycastOrigins.bottomRight : rc.raycastOrigins.bottomLeft;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

            if (hit) {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle) {
                    if (Mathf.Sign(hit.normal.x) == directionX) {
                        if (hit.distance - rc.skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x)) {
                            float moveDistance = Mathf.Abs(moveAmount.x);
                            float descendmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                            moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
                            moveAmount.y -= descendmoveAmountY;

                            collisions.slopeAngle = slopeAngle;
                            collisions.descendingSlope = true;
                            collisions.below = true;
                            collisions.slopeNormal = hit.normal;
                        }
                    }
                }
            }
        }
    }

   public void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 moveAmount) {
        if (hit) {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle > maxSlopeAngle) {
                moveAmount.x = Mathf.Sign(hit.normal.x) * (Mathf.Abs(moveAmount.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);

                collisions.slopeAngle = slopeAngle;
                collisions.slidingDownMaxSlope = true;
                collisions.slopeNormal = hit.normal;
            }
        }
    }

    public struct genericCollisionInfo {
        public bool above, below;
        public bool left, right;
        public bool grounded;

        public bool climbingSlope;
        public bool descendingSlope;
        public bool slidingDownMaxSlope;

        public float slopeAngle, slopeAngleOld;
        public Vector2 slopeNormal;
        public Vector2 moveAmountOld;
        public int faceDir;

        public void Reset() {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false;
            slidingDownMaxSlope = false;
            slopeNormal = Vector2.zero;

            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }
}
