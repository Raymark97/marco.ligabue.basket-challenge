using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class Detector : MonoBehaviour {
	public UnityEvent<string> onBallEnter = new();
	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag("PlayerBall")) {
			onBallEnter.Invoke("player");
		} else if (other.gameObject.CompareTag("NPCBall")) {
			onBallEnter.Invoke("NPC");
		}
	}
}
