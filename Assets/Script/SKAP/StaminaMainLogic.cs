using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StaminaMainLogic : MonoBehaviour {
	public int numberOfBars = 5;
	public float regenTime = 3;
	public bool normalRegenOn = true;
    public Color changeColor = Color.white;
	public Sprite img1;
	public Sprite img2;
	public Sprite img3;

    internal bool staminaAvailable = true;
    internal bool triggerOn = false;
    internal bool overrideOn = false;

    private int imgNum = 1;
	private Component[] barImages;
	private Color defaultColor;
	private int numberOfActiveBars = 0;
	private bool regenReady = false;
	private bool coroutineOn = false;
	private IEnumerator regenCo;
    private bool canRegen = true;
	// Use this for initialization
	void Start () {
		regenCo = RegenCoroutine();
		for (int i = 0; i <= (numberOfBars - 1); i++) {
			transform.GetChild(i).gameObject.SetActive(true);
		}
		defaultColor = transform.GetChild (0).gameObject.GetComponent<Image> ().color;
		barImages = GetComponentsInChildren<Image> ();
		StartCoroutine (ImgChangeCoroutine ());
	}
	
	// Update is called once per frame
	void Update () {
		BarAnimation ();
		numberOfActiveBars = 0;
		for (int i = 0; i <= (numberOfBars - 1); i++) {
			if (transform.GetChild (i).gameObject.GetComponent<Image>().color != changeColor) {
				numberOfActiveBars++;
			}
		}
		if (numberOfActiveBars > 0 && !staminaAvailable) {
			staminaAvailable = true;
		}
		else if (numberOfActiveBars <= 0 && staminaAvailable) {
			staminaAvailable = false;
		}
		if (numberOfActiveBars < numberOfBars && canRegen) {
			if (!coroutineOn && !regenReady || overrideOn && !coroutineOn) {
				if (!normalRegenOn && numberOfActiveBars == 0 || normalRegenOn) {
					regenCo = RegenCoroutine();
					StartCoroutine (regenCo);
				}
			}
			if (regenReady) {
				transform.GetChild (numberOfActiveBars).gameObject.GetComponent<Image> ().color = defaultColor;
				regenReady = false;
				if (overrideOn) {
					overrideOn = false;
				}
			}
		}
		if (triggerOn) {
			if (!overrideOn) {
				if (coroutineOn) {
				StopCoroutine(regenCo);
				coroutineOn = false;
				}
				if (numberOfActiveBars == numberOfBars || numberOfActiveBars > 0 && transform.GetChild (numberOfActiveBars).gameObject.GetComponent<Image> ().color != defaultColor) {
					transform.GetChild (numberOfActiveBars - 1).gameObject.GetComponent<Image> ().color = changeColor;
				}
			}
			triggerOn = false;
			if (overrideOn) {
					overrideOn = false;
			}
			
		}
	
	}

    public void AllowOrStopRegen(bool stop) {
        if (stop) {
            canRegen = false;
        }
        else {
            canRegen = true;
        }
    }
	void BarAnimation() {
		foreach (Image barImg in barImages) {
			if (imgNum == 1 && barImg.sprite != img1) {
				barImg.sprite = img1;
			}
			else if (imgNum == 2 && barImg.sprite != img2) {
				barImg.sprite = img2;
			}
			else if (imgNum == 3 && barImg.sprite != img3) {
				barImg.sprite = img3;
			}
		}
	}
	IEnumerator ImgChangeCoroutine()
	{
		while (gameObject)
		{
			imgNum = 1;
			yield return new WaitForSeconds(.5f);
			imgNum = 2;
			yield return new WaitForSeconds(.5f);
			imgNum = 3;
			yield return new WaitForSeconds(.5f);
		}
	}
	IEnumerator RegenCoroutine() {
		coroutineOn = true;
		yield return new WaitForSeconds(regenTime);
		regenReady = true;
		coroutineOn = false;
	}
}