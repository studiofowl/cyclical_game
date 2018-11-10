using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour {

    //skinWidth is added to the raycast length so that the rays aren't fired from right on the edges but rather from slighly inside the object
    public float skinWidth = .015f;
    public float dstBetweenRays = .25f;
    internal int horizontalRayCount;
    internal int verticalRayCount;

    internal float horizontalRaySpacing;
    internal float verticalRaySpacing;

    internal BoxCollider2D coll;
    internal RaycastOrigins raycastOrigins;

    private BoxCollider2D originalColl;

    public void Start() {
        coll = GetComponent<BoxCollider2D>();
        originalColl = coll;
        CalculateRaySpacing();
    }

    public void ChangeCollLayer (int layerNum) {
        coll.gameObject.layer = layerNum;
    }

    public int CheckLayer() {
        return coll.gameObject.layer;
    }

    public void UpdateRaycastOrigins() {
        Bounds bounds = coll.bounds;
        //shrink the bounds by skinWidth
        bounds.Expand(skinWidth * -2);

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    public void CalculateRaySpacing() {
        Bounds bounds = coll.bounds;
        bounds.Expand(skinWidth * -2);

        float boundsWidth = bounds.size.x;
        float boundsHeight = bounds.size.y;

        horizontalRayCount = Mathf.RoundToInt(boundsHeight / dstBetweenRays);
        verticalRayCount = Mathf.RoundToInt(boundsWidth / dstBetweenRays);

        //minimum of 2 rays
        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    public void ChangeBoxColliderDimensions(float offsetX, float offsetY, float sizeX, float sizeY) {
        coll.offset = new Vector2(offsetX, offsetY);
        coll.size = new Vector2(sizeX, sizeY);
        CalculateRaySpacing();
    }

    public void ResetBoxColliderDimensions() {
        coll = originalColl;
        CalculateRaySpacing();
    }
    public struct RaycastOrigins {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
}
