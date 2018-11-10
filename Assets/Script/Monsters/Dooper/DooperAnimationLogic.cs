using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DooperAnimationLogic : MonoBehaviour {
    private Dooper mainScript;
    private DooperController myController;
	void Start () {
        mainScript = transform.parent.parent.parent.GetComponent<Dooper>();
        myController = transform.parent.parent.parent.GetComponent<DooperController>();

    }
    public void Kick() {
        myController.CheckForPlayer();
    }
    public void StopKicking() {
        mainScript.StopKicking();
    }
}
