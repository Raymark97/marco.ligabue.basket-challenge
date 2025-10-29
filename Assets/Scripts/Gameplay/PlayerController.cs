using Core;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay {
	public class PlayerController : MonoBehaviour {
		[Header("UI")]
		[SerializeField] private Slider slider;

		[Header("Input")]
		[SerializeField] private float sensitivity = 0.1f;
		[SerializeField] private float maxTime = 1f;

		[Header("Gameplay")]
		[SerializeField] private GameObject ballPrefab;
		[SerializeField, Range(0f, 0.3f)]
		private float perfectThreshold = 0.1f;
		[SerializeField]
		private float maxShotPowerMultiplier = 1.2f;
		[SerializeField]
		private float minPowerFraction = 0.3f;

		private bool _isDragging;
		private float _startY;
		private float _endY;
		private float _timer;
		private bool _cooldown;

		private GameManager _gm;

		private void Start() {
			_gm = GameManager.Instance;
		}

		private void Update() {
			if (_cooldown) return;
			HandleInput();

			if (_isDragging)
				slider.value = Mathf.Clamp01((_endY - _startY) * sensitivity);
		}

		private void HandleInput() {
			if (Input.GetMouseButtonDown(0))
				StartDrag(Input.mousePosition.y);
			else if (Input.GetMouseButton(0))
				UpdateDrag(Input.mousePosition.y);
			else if (Input.GetMouseButtonUp(0))
				EndDrag();

			if (_isDragging && _timer >= maxTime)
				EndDrag();
		}

		private void StartDrag(float y) {
			_isDragging = true;
			_startY = _endY = y;
			_timer = 0;
		}

		private void UpdateDrag(float y) {
			if (!_isDragging) return;
			_endY = Mathf.Max(y, _endY);
			_timer += Time.deltaTime;
		}

		private void EndDrag() {
			if (!_isDragging) return;
			_isDragging = false;
			_cooldown = true;
			Shoot(slider.value);
			StartCoroutine(ResetSlider());
		}

		private void Shoot(float charge) {
			var direct = _gm.DirectShotVelocity;
			var bank = _gm.BankShotVelocity;
			
			var directMag = direct.magnitude;
			var bankMag = bank.magnitude;


			var chargePower = Mathf.Lerp(minPowerFraction, maxShotPowerMultiplier, charge);
			
			var currentPower = chargePower * directMag;
			
			var diffDirect = Mathf.Abs(currentPower - directMag) / directMag;
			var diffBank = Mathf.Abs(currentPower - bankMag) / bankMag;

			var perfectDirect = diffDirect <= perfectThreshold;
			var perfectBank = diffBank <= perfectThreshold;

			Vector3 chosenVelocity;
			var isBank = false;

			if (perfectDirect || (!perfectBank && diffDirect < diffBank)) {
				chosenVelocity = perfectDirect ? direct : direct.normalized * currentPower;
			} else {
				chosenVelocity = perfectBank ? bank : bank.normalized * currentPower;
				isBank = true;
			}
			
			var ball = Instantiate(ballPrefab, transform.position, Quaternion.identity);
			var rb = ball.GetComponent<Rigidbody>();
			ball.transform.GetChild(0).tag = "PlayerBall";
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			rb.AddForce(chosenVelocity * rb.mass, ForceMode.Impulse);

			var type = perfectDirect ? "PERFETTO Diretto" :
			perfectBank ? "PERFETTO Tabellone" :
			isBank ? "Bank approssimato" : "Diretto approssimato";
			Debug.Log($"Tiro: {type} (slider={charge:F2}, power={chargePower:F2})");
		}

		private IEnumerator ResetSlider() {
			yield return new WaitForSeconds(0.5f);
			slider.value = 0;
			_cooldown = false;
		}
		
		public float MaxShotPowerMultiplier => maxShotPowerMultiplier;
		public float PerfectThreshold => perfectThreshold;

	}
}
