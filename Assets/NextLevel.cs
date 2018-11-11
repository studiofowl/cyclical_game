using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class NextLevel : MonoBehaviour {

    public LayerMask justPlayer;
    public GameObject[] levels;

    int levelCount = 10;
    CircleCollider2D collider;


	// Use this for initialization
	void Start () {
        collider = GetComponent<CircleCollider2D>();
	}
	
	// Update is called once per frame
	void Update () {
		if (Physics2D.OverlapCircle(transform.position, collider.radius, justPlayer)) {
            int levelNo = GetLevel(collider.gameObject.transform.parent.name);
            ChangeToLevel(levelNo + 1);
        }
	}

    //  Returns the # of the level from the name of the level
    //  GetLevel("Level5") == 5
    private int GetLevel(string levelName)
    {
        return int.Parse(levelName.Substring(5, levelName.Length - 5));
    }

    //  Disables a level by deactivating it
    private void DisableLevel(int level)
    {
        string levelName = "Level" + level.ToString();
        GameObject levelObject = GameObject.Find(levelName);
        levelObject.SetActive(false);
    }
    
    //  Activates the level to change to, and then iteratively deactivates all the other levels
    private void ChangeToLevel(int level)
    {   
        string levelName = "Level" + level.ToString();
        Debug.Log(levelName);
        Debug.Log(GameObject.Find(levelName));
        GameObject.Find(levelName).SetActive(true);
        

        for (int i = 0; i < levelCount; i++) {
            if (i == level)
            {
                continue;
            } else
            {
                DisableLevel(i);
            }
        }
    }
}
