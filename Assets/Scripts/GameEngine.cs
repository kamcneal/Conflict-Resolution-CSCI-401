﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;

public class GameEngine : MonoBehaviour {

    [HideInInspector]
    public GameObject player;

	//---------------------------------------------
	//	Player input variables
	//---------------------------------------------
	public GameObject[] playerIcons;
	public Text[] playerNameTextFields;
	public GameObject nameInputPanel;
	public GameObject panelIconSelect;
	public int currentIcon;
	public Text iconName;
	public GameObject spotlight;
	public GameObject summaryPanel;

	[HideInInspector]
	public List<string> playerNames;

	[HideInInspector]
	public List<string> iconNames;

	// Use this for initialization
	void Start () {
		currentIcon = 0;
        player = GameObject.FindGameObjectWithTag("Player");
		playerNames = new List<string> ();
		iconNames = new List<string> ();
	}

    void Update () {
        if (player == null) {
            player = GameObject.FindGameObjectWithTag("Player");
        }
    }

    public void activateNameInputPanel () {
        //  Display the UI
		nameInputPanel.SetActive (true);

        //  Disable player controls
        player.GetComponent<FirstPersonController>().enabled = false;
    }

    public void deactivateNameInputPanel () {
        //  Hide the UI
		nameInputPanel.SetActive (false);

        //  Enable player controls
        player.GetComponent<FirstPersonController>().enabled = true;
    }

	public void nameSave(InputField name) {
		nameInputPanel.SetActive (false);

		name.placeholder.GetComponent<Text> ().text = "Enter Name";
		playerNames.Add (name.text);
		if (playerNames.Count > playerNameTextFields.Length) {

			//TODO: error handling

		} 

		else {
			playerNameTextFields[playerNames.Count - 1].text = name.text;
		}

		name.text = " ";
		panelIconSelect.SetActive (true);
		playerIcons [0].transform.parent.gameObject.SetActive (true);
		updateIconSelect ();	
	}

	public void updateIconSelect() {
		//position the icon in front of the camera
		playerIcons[currentIcon].transform.position = Camera.main.transform.position + Camera.main.transform.forward * .8f;
		spotlight.transform.position = playerIcons [currentIcon].transform.position + new Vector3 (0, 2, 0);
		spotlight.transform.rotation = Quaternion.Euler (90, 0, 0); 
		//disable the previous icon
		int toFalse = currentIcon - 1;
		if (toFalse == -1) {

			toFalse = playerIcons.Length - 1;

		}
		playerIcons[toFalse].SetActive (false);
	
		//enable the current Icon
		playerIcons[currentIcon].SetActive (true);
		iconName.text = playerIcons[currentIcon].name;
		currentIcon++;
		if (currentIcon == playerIcons.Length) {

			currentIcon = 0;

		}
	}

	public void saveIcon() {
		iconNames.Add (iconName.text);

		for (int i = 0; i < iconNames.Count; i++) {

			playerNameTextFields [i].text = "Player " + (i + 1) + " " + playerNames[i] + " - " + iconNames [i];

		}
		panelIconSelect.SetActive (false);
		for (int i = 0; i < playerIcons.Length; i++) {
			playerIcons [i].SetActive (false);
		}

		summaryPanel.SetActive (true);
	}

	public void donePlayerInput () {
		summaryPanel.SetActive (false);
		//	Re-enable player controls
		//	Remove all UI panels
		//	Spawn next checkpoint at role selection button
			//	Get CheckpointLocations[x]
			//	Instantiate(checkpoint, location)
		player.GetComponent<FirstPersonController>().enabled = true;
	}

	public void addPlayerInput () {
		summaryPanel.SetActive (false);
		nameInputPanel.SetActive (true);
		//	Re-enable player controls
		//	Remove all UI panels
		//	Spawn next checkpoint at role selection button
		//	Get CheckpointLocations[x]
		//	Instantiate(checkpoint, location)

	}

}