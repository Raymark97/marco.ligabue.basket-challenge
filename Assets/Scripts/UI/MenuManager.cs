using Audio;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI {
	public class MenuManager : MonoBehaviour {
		
		[SerializeField] private GameObject mainMenu;
		[SerializeField] private GameObject settingsMenu;
		[SerializeField] private Slider masterVolumeSlider;
		[SerializeField] private Slider musicVolumeSlider;
		[SerializeField] private Slider sfxVolumeSlider;
		public void StartGame() {
			SceneManager.LoadScene("Scenes/MainScene");
		}

		public void OpenSettings() {
			mainMenu.SetActive(false);
			var audioManager = AudioManager.Instance;
			masterVolumeSlider.value = audioManager.GetMasterVolume();
			musicVolumeSlider.value = audioManager.GetMusicVolume();
			sfxVolumeSlider.value = audioManager.GetSFXVolume();
			settingsMenu.SetActive(true);
		}
		
		public void CloseSettings() {
			settingsMenu.SetActive(false);
			mainMenu.SetActive(true);
		}

		public void ExitGame() {
			Application.Quit();
		}
	}
}
