using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class Detector : MonoBehaviour {
	public UnityEvent<int> onBallEnter = new();
	private void OnTriggerEnter(Collider other) {
		Debug.Log($"Entered {other.gameObject.name}");
		if (other.gameObject.CompareTag("PlayerBall")) {
			onBallEnter.Invoke(0);
			other.gameObject.tag = "Untagged";
		} else if (other.gameObject.CompareTag("NPCBall")) {
			onBallEnter.Invoke(1);
			other.gameObject.tag = "Untagged";
		}
	}
}
