using Core;
using System.Collections;
using UnityEngine;

namespace Gameplay {
	[RequireComponent(typeof(Rigidbody))]
	public class BallController : MonoBehaviour {
		[Header("Lifetime")]
		[SerializeField] private float destroyDelay = 5f;

		[Header("Shot Info")]
		public bool bankShot;
		public bool perfectShot;
		public bool fireballActive;

		[Header("References")]
		[SerializeField] private GameEvents gameEvents;
		[SerializeField] private ParticleSystem fireballParticles;
		[SerializeField] private TrailRenderer trailRenderer;

		private Rigidbody _rb;
		private bool _hasBounced;

		private void Awake() {
			_rb = GetComponent<Rigidbody>();
		}

		private void Start() {
			StartCoroutine(AutoDestroy());
			ApplyVisuals();
		}

		private IEnumerator AutoDestroy() {
			yield return new WaitForSeconds(destroyDelay);

			var ball = transform.GetChild(0);
			if (ball.CompareTag("PlayerBall")) {
				gameEvents.OnShotMiss.Invoke(0);
			} else if (ball.CompareTag("NPCBall")) {
				gameEvents.OnShotMiss.Invoke(1);
			}
			Destroy(gameObject);
		}
		
		private void ApplyVisuals() {
			//TODO set fireball material
			fireballParticles.gameObject.SetActive(fireballActive);
			trailRenderer.gameObject.SetActive(perfectShot && !fireballActive);
		}

		private void OnCollisionEnter(Collision collision) {
			if (!_hasBounced && collision.gameObject.CompareTag("Backboard")) {
				bankShot = true;
				_hasBounced = true;
			}
		}
	}
}
