using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class FrameRateSet : MonoBehaviour {
    public int frameRate = 60;

    void Start()
    {
        // I believe the vSyncCount is set to 0 to make the framerate adjust in editor
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = frameRate;
    }
}
