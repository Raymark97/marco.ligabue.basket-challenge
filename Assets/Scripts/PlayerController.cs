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
	private GameObject hoop;


	private bool _isDragging;
	private float _startY;
	private float _endY;
	private float _timer;
	private bool _cooldown;


	void Update() {
		if (_cooldown) return;

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
	}
	
	public void Shoot() {
		if (!ballPrefab || !hoop) {
			Debug.LogWarning("Ball o Hoop non assegnati!");
			return;
		}

		var ballClone = Instantiate(ballPrefab, transform.position, Quaternion.identity);

		var toTarget = hoop.transform.position - ballClone.transform.position;
		var toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);
		var distance = toTargetXZ.magnitude;
		var heightDifference = toTarget.y;

		var ballrb = ballClone.GetComponent<Rigidbody>();

		
		var speed = 10f;
		var gravity = Mathf.Abs(Physics.gravity.y);

		
		var insideSqrt = speed * speed * speed * speed - gravity * (gravity * distance * distance + 2 * heightDifference * speed * speed);

		if (insideSqrt < 0) {
			Debug.LogWarning("Troppo lontano o troppo lento: impossibile colpire il bersaglio con questa velocità.");
			return;
		}

		var angle = Mathf.Atan((speed * speed - Mathf.Sqrt(insideSqrt)) / (gravity * distance));
		var dirXZ = toTargetXZ.normalized;

		
		var launchVelocity = dirXZ * (speed * Mathf.Cos(angle)) + Vector3.up * (speed * Mathf.Sin(angle));

		ballrb.velocity = Vector3.zero;
		ballrb.angularVelocity = Vector3.zero;
		ballrb.AddForce(launchVelocity * ballrb.mass, ForceMode.Impulse);
	}


	public IEnumerator ResetSlider() {
		yield return new WaitForSeconds(0.5f);
		slider.value = 0;
		_timer = 0;
		_endY = 0;
		_cooldown = false;
	}
}
