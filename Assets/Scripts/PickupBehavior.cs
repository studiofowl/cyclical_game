using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupBehavior : MonoBehaviour {

    public LayerMask justPlayer;
    public CircleCollider2D collider;
    MainManager mainManager;

    // Use this for initialization
    void Start()
    {
        mainManager = GameObject.Find("MainManager").GetComponent<MainManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Physics2D.OverlapCircle(transform.position, collider.radius, justPlayer))
        {
            mainManager.BeginNextLevel();
        }
    }
}
