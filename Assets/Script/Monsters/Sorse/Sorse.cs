using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SorseController))]
[RequireComponent(typeof(GenericEnemy))]
public class Sorse : MonoBehaviour {
    public float health = 5;
    public float moveSpeed = 3;
    public float turnTime = 2f;

    private Vector2 velocity;
    private CollisionsController genericController;
    private SorseController myController;
    private GenericEnemy enemyScript;

    private Transform flipTransform;
    private SpriteRenderer sr;
    private Color defaultColor;

    private Coroutine flipCo;
    void Start () {
        genericController = GetComponent<CollisionsController>();
        myController = GetComponent<SorseController>();
        enemyScript = GetComponent<GenericEnemy>();
        flipTransform = transform.GetChild(0);
        sr = flipTransform.GetComponent<SpriteRenderer>();
        defaultColor = sr.color;
        velocity = Vector2.zero;
        flipCo = StartCoroutine(FlipCo());
    }
	
	void Update () {
        if (enemyScript.isHit) {
            HitByPlayer();
        }
        if (health <= 0) {
            Destroy(gameObject);
        }

        velocity.x = moveSpeed * genericController.collisions.faceDir;

        myController.Move(velocity * Time.deltaTime, false);

    }

    void HitByPlayer() {
        if (sr.color == defaultColor) {
            Debug.Log("Sorse hit");
            health -= enemyScript.lastHitDamageAmount;
            StartCoroutine(HurtCo());
        }
        enemyScript.lastHitDamageAmount = 0;
        enemyScript.isHit = false;
    }
    public void SwapDirections(bool fromWallHit = false) {
        if (fromWallHit) {
            StopCoroutine(flipCo);
        }
        flipCo = StartCoroutine(FlipCo());
        FlipSprite();
        genericController.collisions.faceDir *= -1;
    }
    void FlipSprite() {
        Vector2 newScale = flipTransform.localScale;
        newScale.x *= -1;
        flipTransform.localScale = newScale;
    }
    IEnumerator HurtCo() {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.05f);
        sr.color = defaultColor;
    }
    IEnumerator FlipCo() {
        yield return new WaitForSeconds(turnTime);
        SwapDirections();
    }
}
