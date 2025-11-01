using Gameplay;
using System.Collections;
using UnityEngine;

namespace Core {
	public class GameManager : MonoBehaviour {
		public static GameManager Instance;

		[Header("References")]
		[SerializeField] private GameEvents gameEvents;

		[Header("Gameplay")]
		[SerializeField] public Transform hoop;
		[SerializeField] public Transform backboard;
		[SerializeField] public float maxHeight = 4.5f;


		private void Awake() {
			if (Instance == null) Instance = this;
			else { Destroy(gameObject); }
		}
		
		
	}
}
