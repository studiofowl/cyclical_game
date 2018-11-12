using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 using UnityEngine.SceneManagement;
using TMPro;

public class MainManager : MonoBehaviour {

	public TextMeshProUGUI mainTextMesh;
	public float timeBetweenCharacters = 0.1f;
	public GameObject[] allLevels;
	public Transform playerTransform, deathCutoffPoint, safetyNetTriggerPoint;
	public Player playerScript;
	public bool rotateTextOn = true;

	public int currentLevelNumber = 0;
	private int currentLineInLevel = 0;
	private Coroutine writeLineCo;
	private Transform mainTextTransform;
	private bool safetyNetTriggered = false;

	void Start () {
		mainTextTransform = mainTextMesh.transform;
		for (int i = 0; i < allLevels.Length; i++) {
			allLevels[i].SetActive(false);
		}
		playerTransform.position = allLevels[currentLevelNumber].transform.GetChild(0).position;
		allLevels[currentLevelNumber].SetActive(true);
		WriteNextLineForCurrentLevel();
	}

	void Update () {
		if (Input.GetKeyDown("e")) {
			BeginNextLevel();
			playerTransform.position = allLevels[currentLevelNumber].transform.GetChild(0).position;
		}
		if (Input.GetKeyDown("r")) {
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
		if (playerTransform.position.y < deathCutoffPoint.position.y) {
			if (allLevels[currentLevelNumber].transform.childCount < 1) Debug.Log("ERROR - level missing a spawn transform");
			playerScript.ResetPlayerVelocity();
			if (!safetyNetTriggered) playerTransform.position = allLevels[currentLevelNumber].transform.GetChild(0).position;
			else playerTransform.position = allLevels[currentLevelNumber].transform.GetChild(1).position;
		}
		if (!safetyNetTriggered && currentLevelNumber == 14) {
			if (playerTransform.position.x >= safetyNetTriggerPoint.position.x && playerTransform.position.y <= safetyNetTriggerPoint.position.y) {
				BeginNewPhrase("that was another joke", false);
				safetyNetTriggered = true;
			}
		}
	}

	private void BeginNewPhrase(string currentPhrase, bool isFinalLineInPhrase, float preWriteDelayAmount = 0) {
		StopAndDeleteWriting();
		writeLineCo = StartCoroutine(WriteLine(currentPhrase, isFinalLineInPhrase, preWriteDelayAmount));
	}

	private void WriteNextLine(string currentLine, bool isFinalLineInPhrase, float preWriteDelayAmount = 0) {
		if (writeLineCo != null) StopCoroutine(writeLineCo);
		mainTextTransform.eulerAngles = new Vector3(0, 0, 0);
		mainTextMesh.text += "\n";
		writeLineCo = StartCoroutine(WriteLine(currentLine, isFinalLineInPhrase, preWriteDelayAmount));
	}

	private void StopAndDeleteWriting() {
		if (writeLineCo != null) StopCoroutine(writeLineCo);
		mainTextTransform.eulerAngles = new Vector3(0, 0, 0);
		mainTextMesh.text = "";
	}

	public void BeginNextLevel() {
		if ((currentLevelNumber + 1) > allLevels.Length - 1) {
			Debug.Log("ERROR - level number exceeded number of levels");
			return;
		}
		safetyNetTriggered = false;
		currentLevelNumber++;
		currentLineInLevel = 0;
		allLevels[currentLevelNumber - 1].SetActive(false);
		allLevels[currentLevelNumber].SetActive(true);
		WriteNextLineForCurrentLevel();
	}
	
	IEnumerator WriteLine(string currentPhrase, bool isFinalLineInPhrase, float preWriteDelayAmount) {
		yield return new WaitForSeconds(preWriteDelayAmount);
		foreach (char currentChar in currentPhrase) {
			yield return new WaitForSeconds(timeBetweenCharacters);
			mainTextMesh.text += currentChar;
		}
		if (isFinalLineInPhrase) {
			while (rotateTextOn) {
				yield return new WaitForSeconds(0.2833f);
				mainTextTransform.eulerAngles = new Vector3(0, 0, -0.5f);
				yield return new WaitForSeconds(0.2833f);
				mainTextTransform.eulerAngles = new Vector3(0, 0, 0.5f);
			}
			mainTextTransform.eulerAngles = new Vector3(0, 0, 0);
		}
		else {
			currentLineInLevel++;
			WriteNextLineForCurrentLevel();
		}
	}

	private void WriteNextLineForCurrentLevel() {
		switch (currentLevelNumber) {
			case 0:
				switch (currentLineInLevel) {
					case 0:
						BeginNewPhrase("please collect orb", false);
						break;
					case 1:
						WriteNextLine("use WASD to move", true, 0.5f);
						break;
				}
				break;
			case 1:
				BeginNewPhrase("now do it again", true);
				break;
			case 2:
				BeginNewPhrase("again please", true);
				break;
			case 3:
				BeginNewPhrase("again", true);
				break;
			case 4:
				switch (currentLineInLevel) {
					case 0:
						BeginNewPhrase("try jump", false);
						break;
					case 1:
						WriteNextLine("use SPACEBAR to jump", true, 0.5f);
						break;
				}
				break;
			case 5:
				switch (currentLineInLevel) {
					case 0:
						BeginNewPhrase("nice", false);
						break;
					case 1:
						WriteNextLine("do it again", true, 0.5f);
						break;
				}
				break;
			case 6:
				switch (currentLineInLevel) {
					case 0:
						BeginNewPhrase("here's a big gap", false);
						break;
					case 1:
						WriteNextLine("try hold jump", true, 0.5f);
						break;
				}
				break;
			case 7:
				switch (currentLineInLevel) {
					case 0:
						BeginNewPhrase("try grab", false);
						break;
					case 1:
						WriteNextLine("just jump to grab", false, 0.5f);
						break;
					case 2:
						WriteNextLine("then jump from grab", true, 0.5f);
						break;
				}
				break;
			case 8:
				switch (currentLineInLevel) {
					case 0:
						BeginNewPhrase("try roll", false);
						break;
					case 1:
						WriteNextLine("hold RIGHTMOUSECLICK", false, 0.5f);
						break;
					case 2:
						WriteNextLine("and use WASD to roll", true);
						break;
				}
				break;
			case 9:
				switch (currentLineInLevel) {
					case 0:
						BeginNewPhrase("you're doing great", false);
						break;
					case 1:
						WriteNextLine("keep at it", true, 0.5f);
						break;
				}
				break;
			case 10:
				BeginNewPhrase("i'm proud of you", true);
				break;
			case 11:
				switch (currentLineInLevel) {
					case 0:
						BeginNewPhrase("this orb is a doozy", false);
						break;
					case 1:
						WriteNextLine("but i believe in you", true, 0.5f);
						break;
				}
				break;
			case 12:
				switch (currentLineInLevel) {
					case 0:
						BeginNewPhrase("outstanding", false);
						break;
					case 1:
						WriteNextLine("next orb is easy", false, 0.5f);
						break;
					case 2:
						WriteNextLine("you deserve some rest", true, 0.5f);
						break;
				}
				break;
			case 13:
				switch (currentLineInLevel) {
					case 0:
						BeginNewPhrase("oh my, i'm sorry", false);
						break;
					case 1:
						WriteNextLine("this one is impossible", false, 0.5f);
						break;
					case 2:
						WriteNextLine("(try jump)", true, 5f);
						break;
				}
				break;
			case 14:
				switch (currentLineInLevel) {
					case 0:
						BeginNewPhrase("that was a joke", false);
						break;
					case 1:
						WriteNextLine("but this one's impossible", false, 0.5f);
						break;
					case 2:
						WriteNextLine("i'm sorry", true, 0.5f);
						break;
					case 3:
						WriteNextLine("we need you to get the orb", false, 0.5f);
						break;
					case 4:
						WriteNextLine("walk to the right", true, 0.5f);
						break;
				}
				break;
			default:
				StopAndDeleteWriting();
				break;

		}
	}
}
