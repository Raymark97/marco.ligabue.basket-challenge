using Core;
using System.Collections;
using Audio;
using UnityEngine;

namespace Gameplay {
	/// <summary>
	/// Controls the behaviour and lifecycle of a thrown basketball.
	/// Handles visual effects, collision sounds, and automatic cleanup.
	/// </summary>
	[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(AudioSource))]
	public class BallController : MonoBehaviour {
		
		[Header("References")]
		[SerializeField] private GameEvents gameEvents;
		[SerializeField] private ParticleSystem fireballParticles;
		[SerializeField] private TrailRenderer trailRenderer;
		
		[Header("Shot Settings")]
		[SerializeField] private float destroyDelay = 5f;
		
		[Header("Debug")]
		public bool bankShot;
		public bool perfectShot;
		public bool fireballActive;
		
		
		private bool _hasBounced;
		private AudioSource _audioSource;
		
		private void Awake() {
			_audioSource = GetComponent<AudioSource>();
		}

		private void Start() {
			StartCoroutine(AutoDestroy());
			ApplyVisuals();
		}

		/// <summary>
		/// Destroys the ball after a delay and triggers a miss event if no score occurred.
		/// </summary>
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

		/// <summary>
		/// Activates the appropriate trail or fireball effects based on shot state.
		/// </summary>
		private void ApplyVisuals() {
			fireballParticles.gameObject.SetActive(fireballActive);
			trailRenderer.gameObject.SetActive(perfectShot && !fireballActive);
		}

		/// <summary>
		/// Handles collisions with the environment, triggering appropriate sound effects.
		/// The volume is scaled based on impact strength.
		/// </summary>
		/// <param name="collision">Collision data from Unity's physics system.</param>
		private void OnCollisionEnter(Collision collision) {
			var impact = collision.relativeVelocity.magnitude;	
			var normalized = Mathf.InverseLerp(0f, 10f, impact);
			var volume = Mathf.Pow(normalized, 0.5f);

			if (!_hasBounced && collision.gameObject.CompareTag("Backboard")) {
				bankShot = true;
				_hasBounced = true;
				AudioManager.Instance.PlaySFX("BackboardHit", _audioSource, volume);
			} 
			else if (collision.gameObject.CompareTag("Rim")) {
				AudioManager.Instance.PlaySFX("RimHit", _audioSource, volume);
			} 
			else if (collision.gameObject.CompareTag("Floor")) {
				AudioManager.Instance.PlaySFX("FloorHit", _audioSource, volume);
			}
		}
	}
}
