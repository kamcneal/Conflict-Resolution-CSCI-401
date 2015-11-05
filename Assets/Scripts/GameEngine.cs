using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;

public class GameEngine : NetworkBehaviour {

    [HideInInspector]
    public GameObject myPlayer;

    //---------------------------------------------
    //	General game variables
    //---------------------------------------------
    [Header("General Game")]
    public AnimationPanel animationPanel;
    public GameObject checkpointFab;
    public GameObject checkpointLocations;
    private List<Transform> allCheckpoints;
    private List<string> playerNames;
    private List<int> playersChosenToPlay;
    private int currCheckpoint;

    [HideInInspector]
    public List<PlayerClass> allPlayers;
    [SyncVar(hook = "onUpdateID")]
    [HideInInspector]
    public int currAvailableID = 1;
    private PlayerClass currEditedPlayer;   //  The player currently being modified from user input

	//---------------------------------------------
	//	Player input variables
	//---------------------------------------------
    [Header("Player Input Variables")]
	public GameObject[] playerIconFabs;
    public GameObject[] playerIcons;
	public List<GameObject> currentPlayerIcons;
	public Text[] playerNameTextFields;
	public GameObject nameInputPanel;
	public GameObject panelIconSelect;
	public GameObject spotlight;
	public GameObject summaryPanel;
	public GameObject buttonAddPlayer;

	public Text maxMessage;

    //---------------------------------------------
    //	Random player selection variables
    //---------------------------------------------
    [Header("Random Player Selection Variables")]
    [HideInInspector]
    public PlayerClass playerOneClass;
    [HideInInspector]
    public PlayerClass playerTwoClass;
    public GameObject choosePlayersPanel;
    public GameObject playersChosenPanel;
	GameObject placeholderplayer1;
	GameObject placeholderplayer2;
    public Text iconName;
    public Text player1;
    public Text player2;
    public int roundNumber;//not input player variable

    //---------------------------------------------
    //	Wheel spinning variables
    //---------------------------------------------
    [Header("Wheel Spinning Variables")]

	private List<string> iconNames;
    private int currentIcon;
	private static string[] scenariosList;
	private static string[] scenariosTitles;
	private static string currScenario;
	private static string currScenarioTitle;
	private static string[] intentionsList;
	private static string[] currIntentions;
	public GameObject recapPanel;
	private static string P1Recap;
	private static string P2Recap;
	private static string[] playerRoles;

	// Planning and Role-playing
	public Canvas helpMenu;
	//---------------------------------------------
	//	Pro/Con variables
	//---------------------------------------------
	[Header("Pro & Con Variables")]
	public List<string> answers;
	public List<string> answers2;
	private string[] intentions;
	public GameObject proConPanel;
	public List<string> intentList;

	[Header("Score Variables")]
	public Text[] player1Answers;
	public Text[] player2Answers;
	public Text[] answerKey1;
	public Text[] answerKey2;
	public Text roundScore;
	public Text totalScore;
	public int score;
	public int totalscore;
	public GameObject scorePanel;
	public ProsAndConsList pscript;

	void Start () {
		currCheckpoint = 0;

        //  Add all checkpoints
        allCheckpoints = new List<Transform>();
        foreach (Transform child in checkpointLocations.transform) {
            allCheckpoints.Add(child);
        }

		roundNumber = 0;
		currentIcon = 0;
        allPlayers = new List<PlayerClass>();
		playerNames = new List<string> ();
		iconNames = new List<string> ();
        playersChosenToPlay = new List<int>();
		currentPlayerIcons = new List<GameObject> ();
		intentList = new List<string> ();
		answers = new List<string> ();
		answers2 = new List<string> ();
		currIntentions = new string[2];
		playerRoles = new string[2];
		intentionsList = new string[]{"Competing","Compromising","Avoiding","Accomodating","Collaborating"};
		instantiateScenarios ();

		helpMenu = helpMenu.GetComponent<Canvas> ();
		helpMenu.enabled = false;

        //  Spawn the first checkpoint
        Instantiate(checkpointFab, allCheckpoints[currCheckpoint].position, Quaternion.identity);

        //  Spawn the icons
        playerIcons = new GameObject[playerIconFabs.Length];
        for (int i = 0; i < playerIconFabs.Length; ++i) {
            GameObject go = Instantiate(playerIconFabs[i], new Vector3(0, -10, 0), Quaternion.identity) as GameObject;
            playerIcons[i] = go;
        }
	}

    void Update () {
        //  Set myPlayer
        if (myPlayer == null) {
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player")) {
                if (go.GetComponent<FirstPersonController>().isActiveAndEnabled) {
                    myPlayer = go;
                }
            }
        }

        //  Don't let other players activate our checkpoint
        //      This is a terrible way to do it but it's the easiest to implement 
        GameObject check = GameObject.FindGameObjectWithTag("Checkpoint");
        if (check != null) {
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player")) {
                if (go != myPlayer) {
                    Physics.IgnoreCollision(
                        go.GetComponent<Collider>(),
                        check.GetComponent<Collider>()
                    );
                }
            }
        }
    }

    public void activateNameInputPanel () {
        //  Disable player controls
        myPlayer.GetComponent<FirstPersonController>().enabled = false;
        //  Start up the panel animation
        animationPanel.beginAnimation(250, 275, 0.9f);
        //  Wait for animation to finish before displaying UI
        Invoke("actuallyActivateNameInputPanel", animationPanel.animationTime);
    }
    private void actuallyActivateNameInputPanel () {
        //  Display the UI
        nameInputPanel.SetActive(true);
    }

    public void deactivateNameInputPanel () {
        //  Hide the UI
		nameInputPanel.SetActive (false);
    }
	public void activateChoosePlayerPanel() {
		choosePlayersPanel.SetActive (true);
		print ("Activate Choose Players Panel");
		roundNumber++;

        //  Disable player controls
        myPlayer.GetComponent<FirstPersonController>().enabled = false;
	}
	public void deactivateChoosePlayerPanel() {
		choosePlayersPanel.SetActive (false);
	}

	public void activateChosenPanel() {
		playersChosenPanel.SetActive (true);

        //  Check if players have already been chosen
        if (playerOneClass != null && playerTwoClass != null) {
            Debug.Log("Players have already been chosen!");
            this.displayRandomPlayers();
        }
        else {
            //  If client, call ServerChooseRandomPlayers() on the server game engine
            if (this.isServer) {
                ServerChooseRandomPlayers(true);
            }
            else {
                myPlayer.GetComponent<PlayerNetworking>().getRandomPlayersFromServer();
            }
        }

        //  Disable player controls
        myPlayer.GetComponent<FirstPersonController>().enabled = false;
	}
	public void deactivateChosenPanelAndStartRound() {
		playersChosenPanel.SetActive (false);

        //  Update the player's body to be the icon
        GameObject myIcon = this.isServer ? playerOneClass.playerIcon : playerTwoClass.playerIcon;
        Debug.Log("My icon is " + myIcon.name);
        myPlayer.GetComponent<PlayerNetworking>().updateBodyToIcon(myIcon);

        //  Enable player controls
        myPlayer.GetComponent<FirstPersonController>().enabled = true;
	} 

	public void activateProConPanel() {
		if (this.isServer) {
			pscript.populateScrollList (currIntentions [0]);
		}
		
		else {
			
			pscript.populateScrollList (currIntentions [0]);
		}
		proConPanel.SetActive (true);

		myPlayer.GetComponent<FirstPersonController>().enabled = false;

	} 
	public void deactivateProConPanel() {
		
		proConPanel.SetActive (false);
        myPlayer.GetComponent<FirstPersonController>().enabled = true;
		
	}

	public void activateRecapPanel() {
		
		recapPanel.SetActive (true);
		myPlayer.GetComponent<FirstPersonController>().enabled = false;
		
	}

	public void deactivateRecapPanel() {
		
		recapPanel.SetActive (false);
		myPlayer.GetComponent<FirstPersonController>().enabled = true;
		
		
	}

	public void activateScorePanel() {
		
		
		scorePanel.SetActive (true);
		checkAnswers ();
		displayScore (); 
		myPlayer.GetComponent<FirstPersonController>().enabled = false;
		
		
	}
	public void deactivateScorePanel() {
		
		scorePanel.SetActive (false);
		myPlayer.GetComponent<FirstPersonController>().enabled = true;
		
		
	}

    public void nameSave(InputField name) {
        animationPanel.discardPanel();

		if (playerNames.Count < playerNameTextFields.Length) {
            currEditedPlayer = new PlayerClass();
            Debug.Log("Creating player with ID " + currAvailableID);
            currEditedPlayer.playerID = currAvailableID;
            currEditedPlayer.playerName = name.text;
            currAvailableID += 1;
            Debug.Log("ID updated to " + currAvailableID);

            //  Must use command all to update ID from client to server
            if (!this.isServer) {
                myPlayer.GetComponent<PlayerNetworking>().updateCurrAvailableID(currAvailableID);
            }

            Debug.Log("Created a player with ID: " + currEditedPlayer.playerID + " and name: " + currEditedPlayer.playerName);
			
			//TODO: error handling
			
			nameInputPanel.SetActive (false);

			name.placeholder.GetComponent<Text> ().text = "Enter Name";

			playerNames.Add (name.text);

			playerNameTextFields [playerNames.Count - 1].text = name.text;

			name.text = " ";
			panelIconSelect.SetActive (true);
            //  Start up the panel animation
            animationPanel.beginAnimation(500, 600, 0.3f);
            //  Wait for animation to finish before displaying UI
			Invoke("updateIconSelect", animationPanel.animationTime);
		} 
		else {
			summaryPanel.SetActive (true);
			maxMessage.text = "Max Players Reached. Press done to continue";
		}
	}
        
	public void updateIconSelect() {
        //  Move the previous icon away
        playerIcons[currentIcon].transform.position = new Vector3(0, -20, 0);

        //  Get the next icon
        currentIcon = (currentIcon + 1) % playerIcons.Length;

        //  Skip over icon already owned by another player
        while (playerIcons[currentIcon].transform.parent != null) {//playerIcons
			Debug.Log(playerIcons[currentIcon].name + " already owned!");
            currentIcon = (currentIcon + 1) % playerIcons.Length;
        }

        //position the icon in front of the camera
		playerIcons[currentIcon].transform.position =//playericons
            Camera.main.transform.position + Camera.main.transform.forward * .8f + new Vector3(0, -0.18f, 0);
		spotlight.transform.position =
			playerIcons [currentIcon].transform.position +//playerIcons
            new Vector3 (0, 0.8f, 0) +
            (Camera.main.transform.forward * -1/8f);    //  Move it forward a little so more light hits icon
        spotlight.transform.LookAt(playerIcons[currentIcon].transform);

        //  Take out the (Clone) part of the name
        string name = playerIcons[currentIcon].name;
        int i = name.IndexOf("(Clone)");
        if (i != -1) {
            name = name.Substring(0, i) + name.Substring(i + 7);
        }
        iconName.text = name;
	}

	public void saveIcon() {
        /* If the currentIcon , "iconName" that just got saved is not the iconNames list (which contains the selected iconNames) 
         then add it to the list. Add it also to the GameObject List of actual icons (currentPlayer Icons)*/
		if (!iconNames.Contains (iconName.text)) {
			iconNames.Add (iconName.text);

            currEditedPlayer.playerIcon = playerIcons[currentIcon];
            allPlayers.Add(new PlayerClass(currEditedPlayer));
            Debug.Log("Added icon " + currEditedPlayer.playerIcon + " to player #" + currEditedPlayer.playerID);

            //  Update across network
            if (!this.isServer) {
                myPlayer.GetComponent<PlayerNetworking>().sendPlayerToServer(
                    currEditedPlayer.playerName,
                    currentIcon,
                    currEditedPlayer.playerID
                );
            }
            else {
                myPlayer.GetComponent<PlayerNetworking>().receivePlayerFromServer(
                    currEditedPlayer.playerName,
                    currentIcon,
                    currEditedPlayer.playerID
                );
            }

            currentPlayerIcons.Add(playerIcons[currentIcon]);
            //  Update the player text fields
            for (int i = 0; i < allPlayers.Count; ++i) {
                PlayerClass play = allPlayers[i];
                /*playerNameTextFields refers to the summaryPanel in the PlayerInput Variables. It updates each block of text to a player
                and its icon number*/
                playerNameTextFields[i].text =
                    "Player " + play.playerID + " " + play.playerName + " - " + iconNames[i];
            }

			/* Turn the panel off. Discard animation. And set the players to false */
			panelIconSelect.SetActive (false);
			animationPanel.discardPanel();
			for (int i = 0; i < playerIcons.Length; i++) {
                if (playerIcons[i].transform.parent == null) {
                    //playerIcons[i].SetActive(false);
                    //  Move them out of view instead so they can still be found in script
                    playerIcons[i].transform.position = new Vector3(0, -20, 0);
                }
			}

			//turn on summaryPlayerInput Panel
			summaryPanel.SetActive (true);
		} 
		else 
		{
			//TODO handle error
			print ("already chosen");

		}

	}
    //  Update icons across network
    public void updatePlayerAcrossNetwork (string playerName, int iconIndex, int playerID) {
        //  Create and add the player
        PlayerClass newPlayer = new PlayerClass(playerName, playerID);
        newPlayer.playerIcon = playerIcons[iconIndex];

        Debug.Log("Network creating player #" + playerID + " named " + playerName + " with icon " + newPlayer.playerIcon.name);

        iconNames.Add(playerIcons[iconIndex].name);
        currentPlayerIcons.Add(playerIcons[iconIndex]);

        allPlayers.Add(newPlayer);

        //  Update the player text fields
        for (int i = 0; i < allPlayers.Count; ++i) {
            PlayerClass play = allPlayers[i];
            /*playerNameTextFields refers to the summaryPanel in the PlayerInput Variables. It updates each block of text to a player
            and its icon number*/
            playerNameTextFields[i].text =
                "Player " + play.playerID + " " + play.playerName + " - " + iconNames[i];
        }
    }
    
	public void donePlayerInput () {
		summaryPanel.SetActive (false);

		//	Re-enable player controls
        myPlayer.GetComponent<FirstPersonController>().enabled = true;
	}

	public void addPlayerInput () {
		summaryPanel.SetActive (false);
		nameInputPanel.SetActive (true);
	}

    //  Function that can only be run on server
    [Server]
	public void ServerChooseRandomPlayers (bool displayIcons) {
        //  Don't do this if called by client but players have already been chosen
        if (playerOneClass != null && playerTwoClass != null) {
            return;
        }

        //  Check if all players have been chosen to play
        if (playersChosenToPlay.Count >= allPlayers.Count - 1) {
            //  Clear it out so they can be chosen again
            playersChosenToPlay.Clear();
        }

		/*assigns a random number to a index, assign corresponding text, and adds to list of players chosen*/
        int index = Random.Range(0, allPlayers.Count);
        //  Make sure the index hasn't been picked already
        while (playersChosenToPlay.Contains(index)) {
            index = Random.Range(0, allPlayers.Count);
        }

        playersChosenToPlay.Add(index);
	
		//  Set the player one class variable
        playerOneClass = allPlayers[index];

        int index2 = Random.Range(0, allPlayers.Count);
        //  Loop and reroll for as long as you got the same roll or one that's been picked already
        while (playersChosenToPlay.Contains(index2)) {
            index2 = Random.Range(0, allPlayers.Count);
        }

        playersChosenToPlay.Add(index2);

        //  Set the player two class variable
        playerTwoClass = allPlayers[index2];

        //  Must be separate function so this can be done from client side
        if (displayIcons) {
            //  Don't wanna display the client's icons, just server's
            displayRandomPlayers();
        }
	}

    //  Display the random players that have been chosen
    public void displayRandomPlayers () {
        if (playerOneClass == null && playerTwoClass == null) {
            Debug.LogError("Error: One of the players has not been set!");
            return;
        }

        //  Server is always player one
		if (this.isServer) {
            player1.text = playerOneClass.playerName;
            player2.text = "";
			playerOneClass.playerIcon.transform.position =
                Camera.main.transform.position + Camera.main.transform.right * -.6f + Camera.main.transform.forward * .8f + Camera.main.transform.up * -.3f;
		}
        else {
            player2.text = playerTwoClass.playerName;
            player1.text = "";
            playerTwoClass.playerIcon.transform.position =
                Camera.main.transform.position + Camera.main.transform.right * .6f + Camera.main.transform.forward * .8f + Camera.main.transform.up * -.3f; 
		}
	}

	public void updatePlayers() {
		P1Recap = player1.text;
		P2Recap = player2.text;
		Debug.Log (P1Recap);
		Debug.Log (P2Recap);
	}

	static public string getPlayer1() {
		return P1Recap;
	}

	static public string getPlayer2() {
		return P2Recap;
	}

	public void sendAnswers(List<string> ansList) {
		//answers = new List<string> ();

		for (int i = 0; i < ansList.Count; i++) {
			print (ansList[i]);
			if (this.isServer) {
			answers.Add(ansList[i]);
			}
			else {
				answers2.Add(ansList[i]);
			}
		}



	}
	public void checkAnswers() {
		if (intentList == null || intentList.Count == 0) {

			print ("NULL INTENTS");

		} else {
			if (this.isServer) {
				for (int i = 0; i < answers.Count; i++) {
					
					if (intentList.Contains (answers [i])) {
						score++;
						print ("SCORE IS " + score);
				
				
					}
			
				}
			} else {

				for (int i = 0; i < answers2.Count; i++) {
				
					if (intentList.Contains (answers2 [i])) {
						score++;
						print ("SCORE IS " + score);
					}
				}
			}
		}
	}
	public void displayScore() {
		//if (this.isServer) {
			for (int i = 0; i < 6; i++) {
				print (answers [i] + " is first answer ");
				player1Answers [i].text = "-" + answers [i];
				answerKey1 [i].text = "-" + intentList [i];
			
			}
		//} else {



			for (int i = 0; i < 6; i++) {
				print (answers [i] + " is first answer ");
				player2Answers [i].text = "-" + answers [i];
				answerKey2 [i].text = "-" + intentList [i];
				
			}

		//}
		roundScore.text = "Round Score : " + score;
		
		totalScore.text = "Total Score : " + score;
		
	}
	public void sendIntention(string[] intent){
		print ("IM SENDING INTENTION");
		//intentions = new string[6];
		//intentList = new List<string> ();

		for (int i = 0; i < intent.Length; i++) {
			print (intent[i]);
			//intentions [i] = intent [i];
			intentList.Add(intent[i]);
		}
		print (intentList.Count + " is how big the list is");
	}

	public static void setIntention(int playerNumber, int intentionNumber) {
		currIntentions [0] = intentionsList [intentionNumber];
		Debug.Log ("Set Player " + playerNumber + " to Intention " + intentionsList [intentionNumber]);
	
	
	}

	public static void setScenario(int scenarioNumber) {
		currScenario = scenariosList [scenarioNumber];
		currScenarioTitle = scenariosTitles [scenarioNumber];
		Debug.Log ("Set scenario to " + currScenarioTitle);
		setRoles (scenarioNumber);
	}

	static private void setRoles(int scenarioNumber) {
		if(scenarioNumber == 0) {
			int selected = Random.Range(0,1);
			playerRoles[selected] = "Captain";
			if (selected == 0)
				playerRoles[1] = "Cadet";
			else
				playerRoles[0] = "Cadet";
		} else if(scenarioNumber == 1) {
			int selected = Random.Range(0,1);
			playerRoles[selected] = "Lieutenant of Communications";
			if (selected == 0)
				playerRoles[1] = "Lieutenant of Navigation";
			else
				playerRoles[0] = "Lieutenant of Navigation";
		} else if(scenarioNumber == 2) {
			int selected = Random.Range(0,1);
			playerRoles[selected] = "Lieutenant Commander of Weapons";
			if (selected == 0)
				playerRoles[1] = "Ensign";
			else
				playerRoles[0] = "Ensign";
		} else if(scenarioNumber == 3) {
			int selected = Random.Range(0,1);
			playerRoles[selected] = "Captain";
			if (selected == 0)
				playerRoles[1] = "Chief Officer";
			else
				playerRoles[0] = "Chief Officer";
		} else if(scenarioNumber == 4) {
			int selected = Random.Range(0,1);
			playerRoles[selected] = "Staff Officer of Communication";
			if (selected == 0)
				playerRoles[1] = "Staff Officer of Technology";
			else
				playerRoles[0] = "Staff Officer of Technology";
		} else if(scenarioNumber == 5) {
			int selected = Random.Range(0,1);
			playerRoles[selected] = "Physician's Assistant";
			if (selected == 0)
				playerRoles[1] = "Lead Nurse";
			else
				playerRoles[0] = "Lead Nurse";
		} else if(scenarioNumber == 6) {
			int selected = Random.Range(0,1);
			playerRoles[selected] = "Chief Officer";
			if (selected == 0)
				playerRoles[1] = "Captain";
			else
				playerRoles[0] = "Captain";
		} else {
			int selected = Random.Range(0,1);
			playerRoles[selected] = "Fleet Commander";
			if (selected == 0)
				playerRoles[1] = "Captain";
			else
				playerRoles[0] = "Captain";
		}
	}

	private void instantiateScenarios() {
		scenariosList = new string[] {
			"Two weeks ago, the Captain asked the Cadet to prepare a report on the number of enemy invasions. The Manager plans to share the report at the quarterly intergalactic meeting. The Cadet procrastinates doing the work and plans to complete the report the morning of the meeting. When the Cadet reports to work, there is a system failure with the computer and the system will be down all day for repairs. When the Captain asks for the report for the meeting that will occur in an hour, the Cadet does not have the report to give to the Captain. What’s the Captain to do? What is the Cadet’s response when the Captain comes to ask for the report?", 
			"The new Lieutenant of Communications instructs the Lieutenant of Navigation to inform the staff of the new meeting location change from conference room to the flight deck. The Lieutenant of Navigation did not share the information with everyone. Some people show up at the conference room for the meeting and some show up on the flight deck. After the initial confusion, the meeting occurs on the flight deck. This isn’t the first time the Lieutenant of Navigation has made this mistake. After the meeting, the Lieutenant of Communications schedules a meeting with the Lieutenant of Navigation to resolve the miscommunication issue. What will the Lieutenant of Communications say? How will the Lieutenant of Navigation respond?", 
			"The Lieutenant Commander of Weapons in charge of the new satellite design and deployment project is working with the Ensign from Weapons who has the expertise on satellite design but no experience in the field of satellite deployment. The Ensign constantly challenges the Lieutenant Commander’s design plans. The Lieutenant Commander has been patient up to a point, but at the last meeting, erupts out of anger and frustration and shuts down the Ensign in front of the whole team. The Ensign sits in silence and quickly leaves at the end of the meeting. The Lieutenant Commander soon realizes the Ensign’s expertise is needed for the project. How will the Lieutenant Commander resolve the situation? What is the best way for the Ensign to respond to the Lieutenant Commander in light of the Lieutenant Commander actions at the meeting", 
			"The Captain encourages the Chief Officer to apply for the promotion to First Officer of the ship. The Chief Officer meets the requirements and submits the application according to protocol. Over dinner, the Captain reassures the Chief Officer the promotion belongs to the Chief Officer. A week later, the Chief Officer receives a letter from the Fleet Commander denying the application. The Chief Officer confronts the Captain in the transport room.  What will the Chief Officer say? How will the Captain seek to make things right?", 
			"The team of Staff Officers schedules a meeting to finish the final presentation on new communications software. At an earlier meeting, the roles and responsibilities were assigned and deadlines were established. Everyone agree and committed to the plan. On the day of the meeting, the Staff Officer of Communications is a no show. There is no notice and no materials are submitted for the meeting. The project lead, the Staff Officer of Technology is rightfully upset since the presentation is fewer than six hours away. The Staff Officer of Technology tracks down the Staff Officer of Communications in the officers’ quarters. How will the Staff Officer of Communications respond to not showing up to the meeting? How will the Staff Officer of Technology guide and direct the conversation?", 
			"During the last three team meetings to determine the hiring priorities for medical staff, the Physician’s Assistant talks too much and tends to dominate the discussion which impedes the efficiency of the discussion. The team delegates the lead nurse to speak with the Physician’s Assistant about the productivity of the next meeting. How with the Lead Nurse handle the situation? What will the outcome be for the Physician’s Assistant?", 
			"While working on the bridge, the Chief Officer questions the thoughts and ideas of commands from the Captain. The Chief Officer’s tone is sharp, condescending, and bordering insubordination. The Captain detects the Chief Officer’s defensive tone and believes it may be due to the fact that the Chief Officer recently was passed over promotion. Even though the Chief Officer has addressed the issue with the Captain earlier, it still seems the issue remains unresolved. The Captain asks the Chief Officer to meet with the Captain in the Captain’s office. What will the Captain say to bring resolution to the situation? What is bothering the Chief Officer so much that the issue is not yet resolved? What will it take to finally bring closure to the situation?", 
			"The Fleet Commander calls a meeting with the Captain to discuss the work ethic of the Lieutenant Commander of Communications. There is concern that the Lieutenant Commander is not keeping classified information confidential. The Fleet Commander learns from a Staff member that the Lieutenant Commander of Communications shares classified information with people not connected to the mission. The leak of classified information jeopardizes the Special Forces mission to rescue hostages from the enemy. The Fleet Commander directs the Captain to discuss the breach of confidentiality issue with the Lieutenant Commander. How will the Captain address the allegations with the Lieutenant Commander? What is the Lieutenant Commander’s response to the Fleet Commander’s concern?"
		};
		scenariosTitles = new string[] {
			"The Incomplete Assignment",
			"Meeting Miscommunication",
			"Design, Deployment, and Delegation",
			"Passed Over for Promotion",
			"No Show No Call",
			"Discussion Domination",
			"Unresolved Issues",
			"Confidentiality Breach"
		};
	}

	public static string getScenarioTitle() {
		return currScenarioTitle;
	}

	public static string getScenario() {
		return currScenario;
	}

	public static string[] getRoles() {
		return playerRoles;
	}

	public static string[] getIntentions() {
		return currIntentions;
	}

    public void checkpointHit() {
        //  Call appropriate function
		if (currCheckpoint == 0) {
			this.activateNameInputPanel ();
		}

		if (currCheckpoint == 1) {
			this.activateChoosePlayerPanel ();
		}
		// Checkpoint == 2 is wheels

		if (currCheckpoint == 3) {
			updatePlayers();
			this.activateRecapPanel();
		}

		if (currCheckpoint == 4) {
			//Planning stuff
			helpMenu.enabled = true;
		}

		if (currCheckpoint == 5) {
			this.activateProConPanel();
		}
		if (currCheckpoint == 6) {
			this.activateScorePanel();
		}
        //  Spawn next checkpoint
		currCheckpoint++;
        if (currCheckpoint < allCheckpoints.Count) {
            GameObject check = Instantiate(
                checkpointFab,
                allCheckpoints[currCheckpoint].position,
                Quaternion.identity
            ) as GameObject;
        }
	}

    private void onUpdateID (int update) {
        string which = this.isServer ? "server" : "client";
        Debug.Log("On " + which + ". Updating ID from " + currAvailableID + " to " + update);
        currAvailableID = update;
    }

    //  Utility function for recursively changing a GameObject's layer
    public void fullChangeLayer (Transform obj, string layer) {
        foreach (Transform child in obj) {
            child.gameObject.layer = LayerMask.NameToLayer(layer);
            fullChangeLayer(child, layer);
        }
    }
}
