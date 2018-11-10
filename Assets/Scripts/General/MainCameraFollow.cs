using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraFollow : MonoBehaviour {
    // decides if the camera will move over to player or just appear at the player
    // when the script is initialized
    public bool initialSnapToPlayer = false;
    // decides if the Y axis is moved
    public bool moveYAxis = false;
    // the lowest point the camera can go to
    // can be set manually or will be automatically set to the camera's initial y position
    public float yPositionMin = 0;
    // decides how quickly the camera will smoothly move to player
    public float dampTime = 0.15f;
    // the lowest point the camera can go to / the only y position if moveYAxis is off
    private float initialYPosition;
    // used to keep track of camera's velocity
    private Vector3 velocity = Vector3.zero;
    // target for camera to follow
    private Transform target;
    private Camera cam;

    void Start() {
        // set the target of the camera as the Player
        target = GameObject.FindGameObjectWithTag("Player").transform;
        cam = GetComponent<Camera>();
        // sets the camera at the player's location rather than having it move over to it
        if (initialSnapToPlayer) {
            transform.position = new Vector3 (target.position.x, target.position.y, transform.position.z);
        }
        // if the yPositionMin value was left at 0, set the minimum y position to the camera's initial y position 
        if (yPositionMin == 0) {
            initialYPosition = transform.position.y;
        }
        // if the yPositionMin value adjusted, set the minimum y position to manually set value 
        else {
            initialYPosition = yPositionMin;
        }
    }
    void Update() {
        if (target) {
            // lol I'm not quite sure what Viewport means, I got this online
            Vector3 point = cam.WorldToViewportPoint(target.position);
            // camera is currently set to move to the dead center of the Player
            Vector3 delta = target.position - cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z));
            Vector3 destination = transform.position + delta;
            // if not moving y axis then set the y position of the camera to the set position
            if (!moveYAxis) {
                destination.y = initialYPosition;
            }
            else {
                // if the y position ever drops below the minimum, set the y position to the minimum
                if (destination.y < initialYPosition) {
                    destination.y = initialYPosition;
                }
            }
            // set the position of the camera using damping for a smooth velocity
            transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
        }
    }
}
