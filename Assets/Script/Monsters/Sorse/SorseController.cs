using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RaycastController))]
public class SorseController : MonoBehaviour {

    public LayerMask collisionMask;

    internal CollisionsController genericController;
    internal Sorse mainScript;

    private RaycastController rc1;

    void Start() {
        genericController = GetComponent<CollisionsController>();
        mainScript = GetComponent<Sorse>();
        rc1 = GetComponent<RaycastController>();
        rc1.Start();
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

        transform.Translate(moveAmount);

        //used to allow jumping when platform is moving up with passenger on it
        if (standingOnPlatform) {
            genericController.collisions.below = true;
        }
    }

    void HorizontalCollisions(ref Vector2 moveAmount, RaycastController rc) {

        float directionX = genericController.collisions.faceDir;
        float rayLength = Mathf.Abs(moveAmount.x) + rc.skinWidth;

        bool hitOnce = false;

        if (Mathf.Abs(moveAmount.x) < rc.skinWidth) {
            rayLength = 2 * rc.skinWidth;
        }

        for (int i = 0; i < rc.horizontalRayCount; i++) {
            Vector2 rayOrigin = (directionX == -1) ? rc.raycastOrigins.bottomLeft : rc.raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (rc.horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (hit) {
                if (!hitOnce) {
                    hitOnce = true;
                }

                genericController.DefaultHorizontalCollisionLogic(hit, ref moveAmount, i, rc, directionX, ref rayLength);
            }
        }
        if (hitOnce) {
            mainScript.SwapDirections(true);
        }
    }

    void VerticalCollisions(ref Vector2 moveAmount, RaycastController rc) {
        float directionY = Mathf.Sign(moveAmount.y);
        float rayLength = Mathf.Abs(moveAmount.y) + rc.skinWidth;

        for (int i = 0; i < rc.verticalRayCount; i++) {
            Vector2 rayOrigin = (directionY == -1) ? rc.raycastOrigins.bottomLeft : rc.raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (rc.verticalRaySpacing * i + moveAmount.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            if (hit) {
                genericController.DefaultVerticalCollisionLogic(hit, ref moveAmount, rc, directionY, ref rayLength);
            }
        }
    }
}