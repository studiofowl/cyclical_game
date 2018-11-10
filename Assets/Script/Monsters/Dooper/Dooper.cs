using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DooperController))]
[RequireComponent(typeof(GenericEnemy))]
public class Dooper : MonoBehaviour {
    public float health = 5;
    public float damage = 1;
    public float moveSpeed = 3;
    public float gravity = 12.5f;
    public float startingJumpVelocity = 6;
    public float spaceBetweenPlayer = 1f;
    public Color kickColor = Color.red;
    public Color runColor = Color.green;
    public float dropVelocityAfterJump = -10f;
    public bool canMove = true;

    private Vector2 velocity;
    private CollisionsController genericController;
    private DooperController myController;
    private GenericEnemy enemyScript;
    private Transform skapTransform;

    private bool runFaster = false;

    private Transform flipTransform;
    internal Animator legsAnim;
    private SpriteRenderer upperSr, lowerSr;
    private Color defaultColor;
    // Use this for initialization
    void Start () {
        flipTransform = transform.GetChild(0);
        legsAnim = flipTransform.GetChild(0).GetChild(0).GetComponent<Animator>();
        upperSr = flipTransform.GetChild(0).GetComponent<SpriteRenderer>();
        lowerSr = flipTransform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        defaultColor = upperSr.color;

        if (FindObjectOfType<Player>()) {
            skapTransform = FindObjectOfType<Player>().transform;
        }
        else {
            Debug.Log("Dooper needs Player");
        }

        genericController = GetComponent<CollisionsController>();
        myController = GetComponent<DooperController>();
        enemyScript = GetComponent<GenericEnemy>();
        velocity = new Vector2(moveSpeed, 0) * genericController.collisions.faceDir;
    }
	
	// Update is called once per frame
	void Update () {
        if (enemyScript.isHit) {
            HitByPlayer();
        }
        if (health <= 0) {
            Destroy(gameObject);
        }

        CalculateVelocity();

        if (canMove) {
            myController.Move(velocity * Time.deltaTime, false);
        }

        if (genericController.collisions.below) {
            velocity.x = moveSpeed * genericController.collisions.faceDir;
            velocity.y = 0;
        }

        if (!myController.myCollisions.isKicking && myController.myCollisions.ableToAttack && genericController.collisions.grounded) {
            if (myController.CheckAreaInFrontOfDooper(spaceBetweenPlayer)) {
                KickAttack();
            }
        }
        if (!myController.myCollisions.isKicking && !runFaster) {
            if (genericController.collisions.faceDir > 0 && skapTransform.position.x < transform.position.x
            || genericController.collisions.faceDir < 0 && skapTransform.position.x > transform.position.x) {
                runFaster = true;
                if (lowerSr.color != Color.red) lowerSr.color = runColor;
            }
        }

        /*if (myController.myCollisions.isKicking && skapTransform.position.y > (transform.position.y + offsetForSkapJumping)) {
            if (!skapGenericController.collisions.grounded) StopKicking();
        }*/
    }

    void CalculateVelocity() {
        if (!myController.myCollisions.isKicking && !runFaster) {
            velocity.x = moveSpeed * genericController.collisions.faceDir;
        }
        else if (myController.myCollisions.isKicking && !runFaster) {
            velocity.x = moveSpeed * genericController.collisions.faceDir * 2f;
        }
        else {
            velocity.x = moveSpeed * genericController.collisions.faceDir * 2.5f;
        }
        velocity.y += gravity * Time.deltaTime;
    }

    void HitByPlayer() {
        if (upperSr.color == defaultColor) {
            Debug.Log("Dooper hit");
            health -= enemyScript.lastHitDamageAmount;
            if (myController.myCollisions.isKicking) runFaster = true;
            StartCoroutine(HurtCo(myController.myCollisions.isKicking));
        }
        enemyScript.lastHitDamageAmount = 0;
        enemyScript.isHit = false;
    }

    public void SwapDirections(bool fromWallHit = false) {
        if (fromWallHit && runFaster) {
            lowerSr.color = defaultColor;
            runFaster = false;
        }
        FlipSprite();
        genericController.collisions.faceDir *= -1;
    }

    void FlipSprite() {
        Vector2 newScale = flipTransform.localScale;
        newScale.x *= -1;
        flipTransform.localScale = newScale;
    }
    IEnumerator HurtCo(bool wasKicking) {
        upperSr.color = Color.red;
        lowerSr.color = Color.red;
        yield return new WaitForSeconds(0.05f);
        upperSr.color = defaultColor;
        if (wasKicking) lowerSr.color = defaultColor;
        else lowerSr.color = runColor;
    }

    void KickAttack() {
        myController.myCollisions.ableToAttack = false;
        myController.myCollisions.isKicking = true;
        lowerSr.color = kickColor;
        legsAnim.SetBool("isKicking", true);
        velocity.y = startingJumpVelocity;
    }

    public void StopKicking() {

        myController.myCollisions.isKicking = false;
        legsAnim.SetBool("isKicking", false);
        velocity.y = 0;
        if (genericController.collisions.faceDir > 0 && skapTransform.position.x > transform.position.x
            || genericController.collisions.faceDir < 0 && skapTransform.position.x < transform.position.x) SwapDirections();
        if (!genericController.collisions.grounded) {
            if (lowerSr.color != Color.red) lowerSr.color = defaultColor;
            velocity.x = 0;
            velocity.y = dropVelocityAfterJump;
            return;
        }
        /*runFaster = true;
        if (lowerSr.color != Color.red) lowerSr.color = runColor;*/
    }
}
