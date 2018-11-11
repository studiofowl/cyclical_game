using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainManager : MonoBehaviour {

	public TextMeshProUGUI mainTextMesh;
	public string startingLine = "collect orb please";
	public float timeBetweenCharacters = 0.1f;
	public GameObject[] allLevels;
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
		currentLevelNumber++;
		currentLineInLevel = 0;
		if (currentLevelNumber > allLevels.Length - 1) {
			Debug.Log("ERROR - level number exceeded number of levels");
			return;
		}
		allLevels[currentLevelNumber - 1].SetActive(false);
		allLevels[currentLevelNumber].SetActive(true);
		WriteNextLineForCurrentLevel();
	}

	private void WriteNextLineForCurrentLevel() {
		if (currentLevelNumber == 0) {
			if (currentLineInLevel == 0) {
				BeginNewPhrase("collect orb please", false);
			}
			else if (currentLineInLevel == 1) {
				WriteNextLine("use WASD to move", true, 1f);
			}
		}
		else if (currentLevelNumber == 1) {
			if (currentLineInLevel == 0) {
				BeginNewPhrase("now do it again", true);
			}
		}
		else {
			StopAndDeleteWriting();
		}
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
}
