using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRolling : MonoBehaviour {
    public float rotSpeed = 90;
    public float timeBeforeStaminaDrop = 0.5f;
    private Vector3 initialRot, initialLocalPos;
    private CollisionsController genericController;
    private PlayerController myController;
    private Player mainScript;
    private PlayerSwordAttack swordScript;
    private Coroutine stopRollCo;
    private Transform upperBodySprite;
    private Animator anim, legsAnim;
    private bool xInputReleased = false;
    private bool stopRollCoOn = false;
    private bool needsToStopDucking = false;

    void Start () {
        genericController = GetComponent<CollisionsController>();
        myController = GetComponent<PlayerController>();
        mainScript = GetComponent<Player>();
        swordScript = GetComponent<PlayerSwordAttack>();
        upperBodySprite = transform.GetChild(0).GetChild(0);
        initialRot = upperBodySprite.eulerAngles;
        initialLocalPos = upperBodySprite.localPosition;
        anim = upperBodySprite.GetComponent<Animator>();
        legsAnim = upperBodySprite.GetChild(0).GetComponent<Animator>();
    }

    void Update() {
        float xAxisInput = mainScript.directionalInput.x;
        if (!xInputReleased && !myController.myCollisions.isRolling && xAxisInput == 0) {
            xInputReleased = true;
        }
        if (myController.myCollisions.isRolling && xAxisInput != 0) {
            /*if (xAxisInput > 0 && genericController.collisions.faceDir < 0 || xAxisInput < 0 && genericController.collisions.faceDir > 0) {
                genericController.collisions.faceDir *= -1;
            }*/
            upperBodySprite.Rotate(-Vector3.forward * Time.deltaTime * rotSpeed * 100);
        }
        if (stopRollCoOn && !myController.myCollisions.isRolling) {
            StopCoroutine(stopRollCo);
            stopRollCoOn = false;
        }
        if (myController.myCollisions.isRolling && Mathf.Abs(xAxisInput) == 0 && !stopRollCoOn) {
            stopRollCo = StartCoroutine(StopRollingIfNoInput());
        }
        if (myController.myCollisions.isDucking && Mathf.Abs(xAxisInput) > 0 && !myController.myCollisions.isRolling && xInputReleased) {
            StartRollingFromDuck(xAxisInput);
        }
        if (needsToStopDucking) {
            if (myController.myCollisions.roomToStopDucking) StopDuckingOrRolling();
        }
    }

    public void OnRightClickInputDown() {
        if (myController.myCollisions.hasArmsOut) {
            myController.edgeGrabScript.TakeOutOrPutAwayArms(false);
        }
        if (mainScript.directionalInput.x == 0) {
            StartRollOrDuckIfAble(false);
        }
        else {
            StartRollOrDuckIfAble(true);
        }
    }
    public void OnRightClickInputUp() {
        StopDuckingOrRolling();
    }

    void StartRollingFromDuck(float xAxisInput) {
        xInputReleased = false;
        if (myController.myCollisions.isRolling) {
            myController.myCollisions.isRolling = false;
        }
        myController.SwitchCollisionMask(true);
        myController.myCollisions.isRolling = true;
    }

    void StopRolling() {
        myController.SwitchCollisionMask(false);
        myController.myCollisions.isRolling = false;
    }

    void StartRollOrDuckIfAble(bool roll) {
        float xAxisInput = mainScript.directionalInput.x;
        // myController.CheckAreaInFrontOfPlayer(0.3f)
        if (myController.myCollisions.isRolling || myController.myCollisions.isDucking) {
            return;
        }
       needsToStopDucking = false;
       swordScript.TakeOutOrRemoveSword(false, true);
        myController.myCollisions.isDucking = true;
        upperBodySprite.localPosition = new Vector3(upperBodySprite.localPosition.x, -0.6f, upperBodySprite.localPosition.z);
        anim.SetBool("isRolling", true);
        legsAnim.SetBool("isRolling", true);
        if (roll) {
            if (xAxisInput > 0 && genericController.collisions.faceDir < 0 || xAxisInput < 0 && genericController.collisions.faceDir > 0) {
                genericController.collisions.faceDir *= -1;
            }
            myController.SwitchCollisionMask(true);
            myController.myCollisions.isRolling = true;
        }
    }

    public void StopDuckingOrRolling() {
        if (!myController.myCollisions.roomToStopDucking) {
            needsToStopDucking = true;
            return;
        }
        if (genericController.collisions.faceDir < 0) {
            myController.myCollisions.cameFromLeftFacingDuck = true;
        }
        if (myController.myCollisions.isRolling) {
            myController.SwitchCollisionMask(false);
            myController.myCollisions.isRolling = false;
        }
        ResetFromDuckingOrRolling();
    }

    void ResetFromDuckingOrRolling() {
        upperBodySprite.eulerAngles = initialRot;
        upperBodySprite.localPosition = initialLocalPos;
        anim.SetBool("isRolling", false);
        legsAnim.SetBool("isRolling", false);
        myController.myCollisions.isDucking = false;
    }

    IEnumerator StopRollingIfNoInput() {
        stopRollCoOn = true;
        yield return new WaitForSeconds(0.05f);
        if (Mathf.Abs(mainScript.directionalInput.x) == 0) {
            StopRolling();
        }
        stopRollCoOn = false;
    }
}