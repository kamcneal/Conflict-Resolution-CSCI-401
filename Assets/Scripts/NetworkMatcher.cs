﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;

public class NetworkMatcher : MonoBehaviour {

	public MenuScript menuScript;
    private string gameCode;
    private NetworkMatch networkMatch;

	// Use this for initialization
	void Start () {
        NetworkManager.singleton.StartMatchMaker();
        networkMatch = NetworkManager.singleton.matchMaker;
        networkMatch.SetProgramAppID((AppID)379051);
        gameCode = "";
	}

    //--------------------------------------------------
    //	Server Code
    //--------------------------------------------------

    public void startServer () {
        if (gameCode.Equals("")) {
            Debug.LogError("Error: gameCode has not been set");
            return;
        }
        Debug.Log("Creating match under name: " + gameCode);

        //	Create the matchmaker request
        CreateMatchRequest create = new CreateMatchRequest();
        create.name = gameCode;
        create.size = 2;
        create.advertise = true;
        create.password = "";

        networkMatch.CreateMatch(create, NetworkManager.singleton.OnMatchCreate);   
    }

    //--------------------------------------------------
    //	Client Code
    //--------------------------------------------------

    //	Request the list of matches matching gameTypeName
    public void connectToServer (string gCode) {
		gCode = gCode.ToUpper();
		gameCode = gCode;
		Debug.Log("Attempting to join " + gCode);
		NetworkManager.singleton.matchMaker.ListMatches(0, 20, gCode, OnMatchList);
    }

    //	Check for exactly 1 match
    public void OnMatchList (ListMatchResponse matchListResponse) {
        List<MatchDesc> matches = matchListResponse.matches;

		//	The match name must be exact same as gameCode
		for (int i = 0; i < matches.Count; ++i) {
			MatchDesc match = matches[i];
			if (!match.name.Equals(gameCode)) {
				matches.RemoveAt(i);
				i--;
			}
		}

        if (matches.Count > 1) {
            menuScript.MultipleMatchJoinFailed();
            Debug.LogError("THERE ARE MULTIPLE MATCHES IDK WHAT TO DOOOOO");
            for (int i = 0; i < matches.Count; ++i) {
                Debug.LogError("ID: " + matches[i].networkId + "\nName: " + matches[i].name);
            }
        }
        else if (matches.Count == 1) {
            Debug.Log("Joining " + matches[0].name);
            networkMatch.JoinMatch(matches[0].networkId, "", NetworkManager.singleton.OnMatchJoined);
        }
        else {
			menuScript.JoinFailed();
            Debug.LogError("MATCH NOT FOUND!!");
        }
    }

    //--------------------------------------------------
    //	Utility functions
    //--------------------------------------------------

    public string generateMatchKey () {
        gameCode = "";
        int codeLength = 6;
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        //  Generate random string
        for (int i = 0; i < codeLength; ++i) {
            char randomChar = chars[Random.Range(0, chars.Length)];
            gameCode += randomChar;
        }

        return gameCode;
    }
}
