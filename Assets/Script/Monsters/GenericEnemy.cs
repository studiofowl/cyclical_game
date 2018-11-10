using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericEnemy : MonoBehaviour {
    // attributes that all enemies share which can be changed to affect any type of enemy
    // that makes use of them

    // boolean set to determine if the enemt was hit
    public bool isHit = false;
    // amount of damage the enemy recieves
    public float lastHitDamageAmount = 0;
}
