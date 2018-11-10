using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSwordAttack : MonoBehaviour {
    private PlayerController myController;
    private PlayerRolling rollingScript;
    private Animator anim, legsAnim;
    private bool needsToRemoveSword;
    private Coroutine swordOutCo;
    void Start () {
        myController = GetComponent<PlayerController>();
        rollingScript = GetComponent<PlayerRolling>();
        anim = transform.GetChild(0).GetChild(0).GetComponent<Animator>();
        legsAnim = transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Animator>();
    }
	
	void Update () {
        if (myController.myCollisions.underArm) {
            if (myController.myCollisions.swordOut || myController.myCollisions.swordSwinging) {
                TakeOutOrRemoveSword(false, true);
            }
        }
        if (needsToRemoveSword && !myController.myCollisions.swordSwinging) {
            myController.myCollisions.swordOut = false;
            anim.SetBool("swordOut", false);
            legsAnim.SetBool("swordOut", false);
            needsToRemoveSword = false;
        }
		
	}

    public void TakeOutOrRemoveSword (bool takeOut, bool overide = false) {
        if (!takeOut && myController.myCollisions.swordSwinging && !overide) {
            needsToRemoveSword = true;
            return;
        }
        if (myController.myCollisions.swordSwinging) {
            myController.myCollisions.swordSwinging = false;
            anim.SetBool("swordSwinging", false);
        }
        myController.myCollisions.swordOut = takeOut;
        anim.SetBool("swordOut", takeOut);
        legsAnim.SetBool("swordOut", takeOut);
    }

    void SwingSword () {
        myController.CheckForEnemy();
        myController.myCollisions.swordSwinging = true;
        anim.SetBool("swordSwinging", true);
    }

    public void OnRightClickInputDown() {
        if (myController.myCollisions.underArm || myController.myCollisions.swordSwinging || !myController.myCollisions.roomToStopDucking && myController.myCollisions.isDucking) return;

        if (!myController.myCollisions.swordOut) {
            if (myController.myCollisions.isDucking) {
                rollingScript.StopDuckingOrRolling();
            }
            TakeOutOrRemoveSword(true);
            SwingSword();
            if (swordOutCo != null) {
                StopCoroutine(swordOutCo);
            }
            swordOutCo = StartCoroutine(SwordOutCoroutine());
        }

        else {
            if (swordOutCo != null) {
                StopCoroutine(swordOutCo);
            }
            swordOutCo = StartCoroutine(SwordOutCoroutine());
            SwingSword();
        }
    }

    IEnumerator SwordOutCoroutine() {
        yield return new WaitForSeconds(8f);
        TakeOutOrRemoveSword(false);
    }
}
