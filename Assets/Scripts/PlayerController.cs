using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {
	[SerializeField]
	private Slider slider;
	[SerializeField]
	private float sensitivity = 0.1f;
	[SerializeField]
	private float maxTime = 1f;
	[SerializeField]
	private GameObject ballPrefab;
	
	[SerializeField]
	private bool debugMode = false;
	
	private GameManager _gameManager;
	private bool _isDragging;
	private float _startY;
	private float _endY;
	private float _timer;
	private bool _cooldown;


	void Update() {
		if (_cooldown) return;

		if (!debugMode) {
			if (Input.GetMouseButtonDown(0)) {
				_isDragging = true;
				_startY = Input.mousePosition.y;
			}

			// Quando rilasci il click
			if (Input.GetMouseButtonUp(0) || _timer >= maxTime) {
				_isDragging = false;
				_cooldown = true;
				StartCoroutine(ResetSlider());
				Shoot();
			}

			// Durante il drag
			if (_isDragging) {
				_endY = Mathf.Max(Input.mousePosition.y, _endY);
				slider.value = (_endY - _startY) * sensitivity;
			}
		} else {
			if (Input.GetMouseButtonDown(0)) {
				Shoot();
			} else if (Input.GetMouseButtonDown(1)) {
				ShootBank();
			}
		}
	}

	private void Start() {
		_gameManager = GameManager.Instance;
	}

	public void Shoot() {
		if (!ballPrefab) {
			Debug.LogWarning("BallPrefab not found");
			return;
		}

		var ballClone = Instantiate(ballPrefab, transform.position, Quaternion.identity);
		var ballrb = ballClone.GetComponent<Rigidbody>();
		
		ballrb.velocity = Vector3.zero;
		ballrb.angularVelocity = Vector3.zero;
		ballrb.AddForce(_gameManager.directShotVelocity * ballrb.mass, ForceMode.Impulse);
	}
	
	public void ShootBank() {
		if (!ballPrefab) {
			Debug.LogWarning("BallPrefab not found");
			return;
		}

		var ballClone = Instantiate(ballPrefab, transform.position, Quaternion.identity);
		var ballrb = ballClone.GetComponent<Rigidbody>();
		
		ballrb.velocity = Vector3.zero;
		ballrb.angularVelocity = Vector3.zero;
		ballrb.AddForce(_gameManager.indirectShotVelocity * ballrb.mass, ForceMode.Impulse);
	}
	


	public IEnumerator ResetSlider() {
		yield return new WaitForSeconds(0.5f);
		slider.value = 0;
		_timer = 0;
		_endY = 0;
		_cooldown = false;
	}
}
