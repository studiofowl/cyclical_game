using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 using UnityEngine.SceneManagement;
using TMPro;

public class MainManager : MonoBehaviour {

	public TextMeshProUGUI mainTextMesh;
	public float timeBetweenCharacters = 0.1f;
	public GameObject[] allLevels;
	public Transform playerTransform, deathCutoffPoint;
	public Player playerScript;
	public bool rotateTextOn = true;

	private int currentLevelNumber = 0;
	private int currentLineInLevel = 0;
	private Coroutine writeLineCo;
	private Transform mainTextTransform;

	void Start () {
		mainTextTransform = mainTextMesh.transform;
		for (int i = 1; i < allLevels.Length; i++) {
			allLevels[i].SetActive(false);
		}
		WriteNextLineForCurrentLevel();
	}

	void Update () {
		if (Input.GetKeyDown("e")) {
			BeginNextLevel();
		}
		if (Input.GetKeyDown("r")) {
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
		if (playerTransform.position.y < deathCutoffPoint.position.y) {
			if (allLevels[currentLevelNumber].transform.childCount < 1) Debug.Log("ERROR - level missing a spawn transform");
			playerScript.ResetPlayerVelocity();
			playerTransform.position = allLevels[currentLevelNumber].transform.GetChild(0).position;
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
				yield return new WaitForSeconds(0.2f);
				mainTextTransform.eulerAngles = new Vector3(0, 0, -0.5f);
				yield return new WaitForSeconds(0.2f);
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
						BeginNewPhrase("collect orb please", false);
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
			default:
				StopAndDeleteWriting();
				break;

		}
	}
}
