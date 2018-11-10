using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSwordAnimationLogic : MonoBehaviour {

    private PlayerController myController;
    private Animator anim;
	void Start () {
        myController = transform.parent.parent.GetComponent<PlayerController>();
        anim = GetComponent<Animator>();
    }

    public void StopSwordSwinging() {
        myController.myCollisions.swordSwinging = false;
        anim.SetBool("swordSwinging", false);
    }
}
