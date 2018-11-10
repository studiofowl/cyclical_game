using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StaminaWheelLogic : MonoBehaviour {
	public float wheelSpeed = 200;
    public float chaseSpeed = 10;
	public float displacement = 2f;
	public Color changeColor;
	public bool isActivated = true;
    public float circleHBRadius = 0.275f;

    private LayerMask justPowerup;
    private GameObject icon;
	private Color defaultcolor;
	private bool isOverlaping;
	private StaminaMainLogic staminascript;
	private Transform SKAP;
	private int swingCount = 0;
	private bool coroutineOn = false;
	private bool isRotating = true;
    private SpriteRenderer sr, iconSr;
    private Animator anim, iconAnim;
    private CircleCollider2D cc;
    private int[] positionsList;
    private int positionsListIndex = 0;
	// Use this for initialization
	void Start () {
        positionsList = new int[] { 8, 4, 1, 7, 5, 2, 8, 4, 1, 6, 3 };
        justPowerup = LayerMask.GetMask("Powerup");
        if (FindObjectOfType<Player>()) {
            SKAP = FindObjectOfType<Player>().transform;
        }
        else {
            Debug.Log("stamina wheel needs Player");
            gameObject.SetActive(false);
        }
        if (FindObjectOfType<StaminaMainLogic>()) {
            staminascript = FindObjectOfType<StaminaMainLogic>().GetComponent<StaminaMainLogic>();
        }
        else {
            Debug.Log("stamina wheel needs stamina");
            gameObject.SetActive(false);
        }
        cc = GetComponent<CircleCollider2D>();
        icon = transform.parent.GetChild(1).gameObject;
        sr = GetComponent<SpriteRenderer>();
        iconSr = icon.GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        iconAnim = icon.GetComponent<Animator>();
        defaultcolor = sr.color;
		if (isActivated && !sr.enabled) {
			ActivateWheel(true);
		}
		else if (!isActivated && sr.enabled) {
			ActivateWheel(false);
		}
		isRotating = isActivated;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyDown ("q") && isActivated == true) {
			isActivated = false;
			ActivateWheel(false);

		}
		 else if (Input.GetKeyDown ("q") && isActivated == false) {
			transform.parent.position = new Vector2 (SKAP.position.x, SKAP.position.y + (displacement*2));
			isActivated = true;
			ActivateWheel(true);
		}

		if (isActivated) {
			Vector2 targetPos = new Vector2 (SKAP.position.x, SKAP.position.y + displacement);
			transform.parent.position = Vector2.Lerp (transform.parent.position, targetPos, Time.deltaTime * chaseSpeed);
			if (isRotating && staminascript.staminaAvailable) {
                transform.Rotate(-Vector3.forward * Time.deltaTime * wheelSpeed);
                CheckForIcon();
            }
		}
	}
    public bool CheckStaminaWheel() {
        if (isOverlaping) {
            int positionNumber = positionsList[positionsListIndex] + 1;
            if (positionsListIndex == positionsList.Length - 1) {
                positionsListIndex = 0;
            }
            else {
                positionsListIndex++;
            }
            icon.transform.position = transform.parent.GetChild(positionNumber).position;
            return true;
        }
        else {
            swingCount += 1;
            if (swingCount >= 2) {
                StartCoroutine(TemporaryDisableCoroutine());
            }
            else if (swingCount == 1 && !coroutineOn) {
                StartCoroutine(SwingCountCoroutine());
            }
            return false;
        }
    }
    void ActivateWheel (bool activate) {
		sr.enabled = activate;
		anim.enabled = activate;
        iconSr.enabled = activate;
		iconAnim.enabled = activate;
	}
    public void CheckForIcon(bool fromInput = false) {
        Vector2 centerLoc = cc.bounds.center;
        Collider2D hit = Physics2D.OverlapCircle(centerLoc, circleHBRadius, justPowerup);

        Vector2 position1 = new Vector2(centerLoc.x - circleHBRadius, centerLoc.y);
        Vector2 position2 = new Vector2(centerLoc.x, centerLoc.y - circleHBRadius);
        Debug.DrawRay(position1, Vector2.right * circleHBRadius * 2, Color.red);
        Debug.DrawRay(position2, Vector2.up * circleHBRadius * 2, Color.red);
        if (hit) {
            sr.color = changeColor;
            isOverlaping = true;
        }
        else {
            sr.color = defaultcolor;
            isOverlaping = false;
        }
    }
	IEnumerator SwingCountCoroutine() {
		coroutineOn = true;
		yield return new WaitForSeconds(0.5f);
        swingCount = 0;
		coroutineOn = false;
	}
	IEnumerator TemporaryDisableCoroutine() {
		isRotating = false;
        swingCount = 0;
		yield return new WaitForSeconds(1f);
		isRotating = true;
	}
}
