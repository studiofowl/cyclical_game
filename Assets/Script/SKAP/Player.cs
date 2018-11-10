using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent (typeof (PlayerController))]
public class Player : MonoBehaviour {

    public float health = 15;
    // maximum move speed (velocity builds up)
    public float moveSpeed = 6;
    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
    public float timeToJumpApex = 0.4f;
    private float accelerationTimeAirborne = .2f;
    private float accelerationTimeGrounded = .1f;

    private float gravity;
    private float maxJumpVelocity;
    private float minJumpVelocity;
    private Vector3 velocity;
    private float velocityXSmoothing;

    private CollisionsController genericController;
    private PlayerController myController;
    private PlayerRolling rollingScript;

    private Transform flipTransform;

    private Text healthText;

    private Animator anim, legsAnim;
    private SpriteRenderer upperSr, lowerSr;
    private Color defaultColor;

    internal Vector2 directionalInput;

	void Start () {
        // conditional to make health text optional
        if (GameObject.Find("Health Text")) {
            healthText = GameObject.Find("Health Text").GetComponent<Text>();
        }
        else {
            Debug.Log("Can't find 'Health Text'");
        }
        if (healthText != null) healthText.text = "Health: " + health;
        flipTransform = transform.GetChild(0);
        anim = flipTransform.GetChild(0).GetComponent<Animator>();
        legsAnim = flipTransform.GetChild(0).GetChild(0).GetComponent<Animator>();
        upperSr = flipTransform.GetChild(0).GetComponent<SpriteRenderer>();
        lowerSr = flipTransform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        defaultColor = upperSr.color;

        genericController = GetComponent<CollisionsController>();
        myController = GetComponent<PlayerController>();
        rollingScript = GetComponent<PlayerRolling>();

        //kinematics
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
        print("Gravity: " + gravity + " Jump Velocity: " + maxJumpVelocity);
	}
    void Update() {
        if (health <= 0) {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }

        CalculateVelocity();

        myController.Move(velocity * Time.deltaTime, directionalInput);
        if (genericController.collisions.above || genericController.collisions.below) {
            if (genericController.collisions.slidingDownMaxSlope) {
                //player slides down steeper slope faster
                //lower slopeNormal.y means less resistance to gravity
                velocity.y += genericController.collisions.slopeNormal.y * -gravity * Time.deltaTime;
            }
            else {
                velocity.y = 0;
            }
        }

        if (!genericController.collisions.slidingDownMaxSlope) {
            anim.SetFloat("horizontalVelocity", Mathf.Abs(directionalInput.x));
            legsAnim.SetFloat("horizontalVelocity", Mathf.Abs(directionalInput.x));
        }
        else {
            anim.SetFloat("horizontalVelocity", 0);
            legsAnim.SetFloat("horizontalVelocity", 0);
        }
        anim.SetFloat("verticalVelocity", Mathf.Abs(velocity.y));
        legsAnim.SetFloat("verticalVelocity", Mathf.Abs(velocity.y));
        anim.SetBool("grounded", genericController.collisions.grounded);
        legsAnim.SetBool("grounded", genericController.collisions.grounded);
    }

    void CalculateVelocity() {
        if (!myController.myCollisions.isRolling && !myController.myCollisions.isDucking) {
            float targetVelocityX = directionalInput.x * moveSpeed;
            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (genericController.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        }
        else if (myController.myCollisions.isRolling) {
            velocity.x = directionalInput.x * (moveSpeed * 2);
        }
        else {
            velocity.x = 0;
        }
        velocity.y += gravity * Time.deltaTime;
    }

    public void SetDirectionalInput (Vector2 input) {
        directionalInput = input;
    }
    public void OnJumpInputDown() {
        if (!myController.myCollisions.roomToStopDucking && myController.myCollisions.isDucking) return;
        if (myController.myCollisions.isDucking && genericController.collisions.grounded) {
            rollingScript.StopDuckingOrRolling();
        }
        if (genericController.collisions.below) {
            /*if (myController.myCollisions.hasArmsOut) {
                myController.edgeGrabScript.TakeOutOrPutAwayArms(false);
                if (!myController.myCollisions.delayAfterEdgeJump) {
                    myController.edgeGrabScript.StartCoroutine(myController.edgeGrabScript.DelayAfterEdgeJump());
                }
            }*/
            if (genericController.collisions.slidingDownMaxSlope) {
                if (directionalInput.x != -Mathf.Sign(genericController.collisions.slopeNormal.x)) {  // not jumping against max slope
                    velocity.y = maxJumpVelocity * genericController.collisions.slopeNormal.y;
                    velocity.x = maxJumpVelocity * genericController.collisions.slopeNormal.x;
                }
            }
            else {
                velocity.y = maxJumpVelocity;
            }
        }
    }

    public void OnJumpInputUp() {
        if (!myController.myCollisions.roomToStopDucking && myController.myCollisions.isDucking) return;
        if (velocity.y > minJumpVelocity) {
            velocity.y = minJumpVelocity;
        }
    }

    public void OnRightClickInputUp() {
        if (myController.myCollisions.isDucking) {
            velocity.x *= 0.5f;
        }
    }

    public void DecideIfFlipSprite(float moveAmountX) {
        if (genericController.collisions.faceDir > 0 && flipTransform.localScale.x < 0) {
            FlipSprite();
        }
        else if (genericController.collisions.faceDir < 0 && flipTransform.localScale.x > 0) {
            FlipSprite();
        }
    }

    void FlipSprite() {
        Vector2 newScale = flipTransform.localScale;
        newScale.x *= -1;
        flipTransform.localScale = newScale;
    }

    public void PlayerHit(float damageAmount) {
        if (upperSr.color == defaultColor) {
            health -= damageAmount;
            if (healthText != null) healthText.text = "Health: " + health;
            StartCoroutine(HurtCo());
        }
    }

    IEnumerator HurtCo() {
        upperSr.color = Color.red;
        lowerSr.color = Color.red;
        yield return new WaitForSeconds(0.05f);
        upperSr.color = defaultColor;
        lowerSr.color = defaultColor;
    }
}
