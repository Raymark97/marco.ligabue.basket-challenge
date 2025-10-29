using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI {
	public class MenuManager : MonoBehaviour {
		public void StartGame() {
			SceneManager.LoadScene("Scenes/MainScene");
		}

		public void OpenSettings() { }

		public void ExitGame() {
			Application.Quit();
		}
	}
}
