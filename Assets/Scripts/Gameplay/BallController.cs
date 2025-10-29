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

		private Rigidbody _rb;
		private bool _hasBounced;

		private void Awake() {
			_rb = GetComponent<Rigidbody>();
		}

		private void Start() {
			StartCoroutine(AutoDestroy());
		}

		private IEnumerator AutoDestroy() {
			yield return new WaitForSeconds(destroyDelay);
			Destroy(gameObject);
		}

		private void OnCollisionEnter(Collision collision) {
			if (!_hasBounced && collision.gameObject.CompareTag("Backboard")) {
				bankShot = true;
				_hasBounced = true;
				Debug.Log("[BallController] Bank shot detected.");
			}
		}

		// Facoltativo: per segnare un tiro perfetto
		public void MarkPerfect() {
			perfectShot = true;
		}
	}
}
