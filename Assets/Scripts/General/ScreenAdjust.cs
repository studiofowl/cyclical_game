using UnityEngine;
using System.Collections;

public class ScreenAdjust : MonoBehaviour {

	void Start () {
        // set the Camera aspect to this ratio no matter what the screen dimensions are
        Camera.main.aspect = 1366f / 768f;
    }
}
