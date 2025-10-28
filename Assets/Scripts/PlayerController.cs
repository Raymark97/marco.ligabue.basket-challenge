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


	private void Start() {
		_gameManager = GameManager.Instance;
	}


	private void Update() {
		if (_cooldown) return;
		if(!debugMode)
			HandleInput();
		else {
			DebugInput();
		}
		if (_isDragging) {
			_timer += Time.deltaTime;
			slider.value = (_endY - _startY) * sensitivity;
		}
	}
	private void DebugInput() {
		if (Input.GetMouseButtonDown(0)) {
			Shoot();
		} else if (Input.GetMouseButtonDown(1)) {
			ShootBank();
		}
	}

	#region Input

	private void HandleInput() {
		if (Input.GetMouseButtonDown(0)) {
			StartDrag(Input.mousePosition.y);
		} else if (Input.GetMouseButton(0)) {
			UpdateDrag(Input.mousePosition.y);
		} else if (Input.GetMouseButtonUp(0)) {
			EndDrag();
		}

		if (Input.touchCount > 0) {
			var touch = Input.GetTouch(0);
			switch (touch.phase) {
				case TouchPhase.Began:
					StartDrag(touch.position.y);
					break;

				case TouchPhase.Moved:
				case TouchPhase.Stationary:
					UpdateDrag(touch.position.y);
					break;

				case TouchPhase.Ended:
				case TouchPhase.Canceled:
					EndDrag();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		if (_isDragging && _timer >= maxTime)
			EndDrag();
	}

	private void StartDrag(float startY) {
		_isDragging = true;
		_timer = 0f;
		_startY = startY;
		_endY = startY;
	}

	private void UpdateDrag(float currentY) {
		if (!_isDragging) return;
		_endY = Mathf.Max(currentY, _endY);
	}

	private void EndDrag() {
		if (!_isDragging) return;

		_isDragging = false;
		_cooldown = true;
		StartCoroutine(ResetSlider());
		Shoot();
	}

	#endregion

	#region Shoot

	public void Shoot() {
		if (!ballPrefab) {
			Debug.LogWarning("BallPrefab not found");
			return;
		}

		var ballClone = Instantiate(ballPrefab, transform.position, Quaternion.identity);
		ballClone.tag = "PlayerBall";
		ballClone.transform.GetChild(0).tag = "PlayerBall";
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
		ballClone.tag = "PlayerBall";
		ballClone.transform.GetChild(0).tag = "PlayerBall";
		var ballrb = ballClone.GetComponent<Rigidbody>();

		ballrb.velocity = Vector3.zero;
		ballrb.angularVelocity = Vector3.zero;
		ballrb.AddForce(_gameManager.bankShotVelocity * ballrb.mass, ForceMode.Impulse);
	}

	#endregion
	
	public IEnumerator ResetSlider() {
		yield return new WaitForSeconds(0.5f);
		slider.value = 0;
		_timer = 0;
		_endY = 0;
		_cooldown = false;
	}
}
