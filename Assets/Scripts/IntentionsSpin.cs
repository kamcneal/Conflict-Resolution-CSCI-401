﻿using UnityEngine;
using System.Collections;

public class IntentionsSpin : MonoBehaviour {

	private bool spinning = false;
	private float currentSpeed = 0;
	public int currentRoll = 0;

	// Use this for initialization
	void Start () {
		GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
	}

	void DecreaseSpeed() {
		if (currentSpeed > 10 && currentSpeed < 20) {
			currentSpeed = 0f;
		} else {
			currentSpeed -= 10f;
		}
		if (currentSpeed == 0) {
			CancelInvoke();
			spinning = false;
		}
	}

	int calculateAngle(int number) {
		return (25 + (number * 72));
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp ("return")) {
			if (!spinning) {
				int roll = Random.Range (0, 4);
				int equivAngle;
				if (currentRoll > roll) {
					equivAngle = (calculateAngle (roll) + 360) - calculateAngle (currentRoll);
				} else {
					equivAngle = calculateAngle (roll) - calculateAngle (currentRoll);
				}
				int rotations = Random.Range (2, 5);
				Debug.Log ("IntInit: " + currentRoll);
				Debug.Log ("IntCurr: " + roll);
				Debug.Log ("IntAngle: " + equivAngle);
				Debug.Log ("IntRotations: " + rotations);
				currentSpeed = (-10 + Mathf.Sqrt (100f + 160f*(360f*(float)(rotations)+(float)(equivAngle))))/2f;
				currentRoll = roll;
				spinning = true;
				InvokeRepeating ("DecreaseSpeed", 0.5f, 0.5f);
			}
		}
		transform.Rotate (Vector3.up, currentSpeed * Time.deltaTime);
	}
}
