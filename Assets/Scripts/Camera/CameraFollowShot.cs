using Core;
using System.Collections;
using UnityEngine;

namespace Camera {
	public class CameraFollowShot : MonoBehaviour {
		[Header("References")]
		[SerializeField] private GameEvents gameEvents;
		[SerializeField] private Transform hoop;

		[Header("Camera Settings")]
		[SerializeField] private float zoomDistance = 3f;
		[SerializeField] private float heightOffset = 2f;
		[SerializeField] private float rotationSmooth = 3f;
		[SerializeField] private float zoomSmooth = 2f;
		[SerializeField] private float maxFollowDistance = 10f;
		[SerializeField] private float followDistance = 2f;
		[SerializeField] private float followSmooth = 2f;
		[SerializeField] private float zoomFOV = 40f;
		[SerializeField] private float baseHeightOffset = 1.5f;
		[SerializeField] private float returnDelay = 0.8f;
		[SerializeField] private float startDelay = 0.5f;

		private UnityEngine.Camera _cam;
		private float _baseFOV;
		private Coroutine _routine;
		private Transform _ball;

		private void Awake() {
			_cam = GetComponentInChildren<UnityEngine.Camera>();
			_baseFOV = _cam.fieldOfView;
		}

		private void Start() {
			ReturnToPlayer();
		}

		private void OnEnable() {
			gameEvents.OnBallThrown.AddListener(OnBallThrown);
			gameEvents.OnScoreAdded.AddListener(OnShotEnded);
			gameEvents.OnShotMiss.AddListener(OnShotEnded);
		}

		private void OnDisable() {
			gameEvents.OnBallThrown.RemoveListener(OnBallThrown);
			gameEvents.OnScoreAdded.RemoveListener(OnShotEnded);
			gameEvents.OnShotMiss.RemoveListener(OnShotEnded);
		}

		private void OnBallThrown(int playerId, GameObject ball) {
			if (playerId != 0) return;
			if (_routine != null)
				StopCoroutine(_routine);
			_routine = StartCoroutine(FollowRoutine(ball.transform));
		}

		private void OnShotEnded(int playerId) {
			if (playerId != 0) return;
			ReturnToPlayer();
		}
		private void OnShotEnded(int playerId, bool __, bool ___) {
			if (playerId != 0) return;
			ReturnToPlayer();
		}

		private IEnumerator FollowRoutine(Transform ball) {
			_ball = ball;
			var rb = _ball.GetComponent<Rigidbody>();
			var startFOV = _cam.fieldOfView;

			// memorizziamo l'altezza del ferro
			float hoopHeight = hoop.position.y;

			while (_ball) {
				// distanza dalla palla al canestro
				float distToHoop = Vector3.Distance(_ball.position, hoop.position);

				// Zoom dinamico: più la palla si avvicina al ferro, più riduciamo il FOV
				float targetFOV = Mathf.Lerp(zoomFOV, startFOV, distToHoop / maxFollowDistance);
				_cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, targetFOV, Time.deltaTime * zoomSmooth);

				// Posizione: dietro la palla ma con altezza vincolata al ferro
				Vector3 followOffset = -rb.velocity.normalized * followDistance;
				Vector3 targetPos = _ball.position + followOffset;
				targetPos.y = Mathf.Lerp(targetPos.y, hoopHeight + heightOffset, 0.7f); // si stabilizza vicino al ferro

				_cam.transform.position = Vector3.Lerp(
				_cam.transform.position,
				targetPos,
				Time.deltaTime * followSmooth
				);

				// Rotazione: guarda verso un punto tra la palla e il canestro, ma limitando la componente verticale
				Vector3 lookTarget = Vector3.Lerp(_ball.position, hoop.position, 0.6f);
				lookTarget.y = hoopHeight; // mantiene lo sguardo quasi orizzontale
				Vector3 lookDir = lookTarget - _cam.transform.position;

				_cam.transform.rotation = Quaternion.Slerp(
				_cam.transform.rotation,
				Quaternion.LookRotation(lookDir),
				Time.deltaTime * rotationSmooth
				);

				// Interrompe il follow appena la palla inizia a scendere
				if (rb.velocity.y < 0)
					break;

				yield return null;
			}

			// Pausa breve per mostrare il risultato
			yield return new WaitForSeconds(returnDelay);
			ReturnToPlayer();
		}

		private void ReturnToPlayer() {
			if (_routine != null)
				StopCoroutine(_routine);
			_cam.transform.position = transform.position;
			_cam.transform.rotation = transform.rotation;
			_cam.fieldOfView = _baseFOV;
		}
	}
}
