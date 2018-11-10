using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEdgeGrab : MonoBehaviour {
    public float timeDelayAfterEdgeJump = 0.5f;
    private PlayerController myController;
    private PlayerSwordAttack swordScript;
    private Animator anim, legsAnim;
	void Start () {
        anim = transform.GetChild(0).GetChild(0).GetComponent<Animator>();
        legsAnim = transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Animator>();
        myController = GetComponent<PlayerController>();
        swordScript = GetComponent<PlayerSwordAttack>();
    }
	
	public void TakeOutOrPutAwayArms(bool takeOut) {
        if (!takeOut) {
            myController.myCollisions.underArm = false;
        }
        myController.myCollisions.hasArmsOut = takeOut;
        anim.SetBool("armsOut", takeOut);
        legsAnim.SetBool("armsOut", takeOut);
    }
    public void RemoveSword() {
        if (myController.myCollisions.swordOut || myController.myCollisions.swordSwinging) {
            swordScript.TakeOutOrRemoveSword(false, true);
        }
    }
    public IEnumerator DelayAfterEdgeJump() {
        myController.myCollisions.delayAfterEdgeJump = true;
        yield return new WaitForSeconds(timeDelayAfterEdgeJump);
        myController.myCollisions.delayAfterEdgeJump = false;
    }
}
