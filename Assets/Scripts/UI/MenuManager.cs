using Audio;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI {
	/// <summary>
	/// Handles main menu and settings UI navigation,
	/// including audio settings synchronization and scene transitions.
	/// </summary>
	public class MenuManager : MonoBehaviour {
        
		[Header("References")]
		[SerializeField] private GameObject mainMenu;
		[SerializeField] private GameObject settingsMenu;
		[SerializeField] private Slider masterVolumeSlider;
		[SerializeField] private Slider musicVolumeSlider;
		[SerializeField] private Slider sfxVolumeSlider;

		/// <summary>
		/// Loads the main gameplay scene and starts the match.
		/// </summary>
		public void StartGame() {
			SceneManager.LoadScene("Scenes/MainScene");
		}

		/// <summary>
		/// Opens the settings menu, populating sliders with current audio values.
		/// </summary>
		public void OpenSettings() {
			mainMenu.SetActive(false);
			var audioManager = AudioManager.Instance;
			masterVolumeSlider.value = audioManager.GetMasterVolume();
			musicVolumeSlider.value = audioManager.GetMusicVolume();
			sfxVolumeSlider.value = audioManager.GetSFXVolume();
			settingsMenu.SetActive(true);
		}
        
		/// <summary>
		/// Closes the settings menu and returns to the main menu.
		/// </summary>
		public void CloseSettings() {
			settingsMenu.SetActive(false);
			mainMenu.SetActive(true);
		}

		/// <summary>
		/// Quits the application.
		/// </summary>
		public void ExitGame() {
			Application.Quit();
		}
	}
}
