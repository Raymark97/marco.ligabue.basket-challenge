using UnityEngine;
using UnityEngine.Audio;

namespace Audio {
	[RequireComponent(typeof(AudioSource))]
	public class AudioManager : MonoBehaviour {
		public static AudioManager Instance;
		public AudioMixer mixer;
		public AudioLibrary library;
		public AudioSource musicAudioSource;

		protected void Awake() {
			if (Instance) {
				Debug.LogWarning("Multiple AudioManagers found in the scene. Deleting the new one.");
				Destroy(gameObject);
				return;
			}
			DontDestroyOnLoad(gameObject);
			Instance = this;
			musicAudioSource = GetComponent<AudioSource>();
		}

		public void Start() {
			musicAudioSource.loop = true;
			musicAudioSource.clip = library.GetClip("GameMusic");
		}

		public void SetMasterVolume(float volume) {
			mixer.SetFloat("MasterVolume", toDB(volume));
		}

		public void SetMusicVolume(float volume) {
			mixer.SetFloat("MusicVolume", toDB(volume));
		}

		public void SetSFXVolume(float volume) {
			mixer.SetFloat("SFXVolume", toDB(volume));
		}
	
		private static float toDB(float sliderValue) => Mathf.Lerp(-80f, 0f, Mathf.Pow(sliderValue, 0.25f));

		public void PlaySFX(string clipName, AudioSource source, float volume = 1f) {
			var clip = library.GetClip(clipName);
			if (clip == null) {
				Debug.LogWarning("Clip not found: " + clipName);
				return;
			}
			source.PlayOneShot(clip, volume);
		}
	}
}
