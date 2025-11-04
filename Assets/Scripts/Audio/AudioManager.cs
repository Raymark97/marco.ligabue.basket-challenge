using UnityEngine;
using UnityEngine.Audio;

namespace Audio {
	[RequireComponent(typeof(AudioSource))]
	public class AudioManager : MonoBehaviour {
		public static AudioManager Instance;
		[SerializeField] private AudioMixer mixer;
		[SerializeField] private AudioLibrary library;
		private AudioSource _musicAudioSource;

		private const string MasterVolume = "MasterVolume";
		private const string MusicVolume = "MusicVolume";
		private const string SFXVolume = "SFXVolume";

		protected void Awake() {
			if (Instance) {
				Debug.LogWarning("Multiple AudioManagers found in the scene. Deleting the new one.");
				Destroy(gameObject);
				return;
			}
			DontDestroyOnLoad(gameObject);
			Instance = this;
			_musicAudioSource = GetComponent<AudioSource>();
		}

		public void Start() {
			_musicAudioSource.loop = true;
			_musicAudioSource.clip = library.GetClip("GameMusic");
			_musicAudioSource.Play();

			LoadVolumes();
		}

		public void SetMasterVolume(float volume) {
			mixer.SetFloat(MasterVolume, toDB(volume));
			PlayerPrefs.SetFloat(MasterVolume, volume);
		}

		public void SetMusicVolume(float volume) {
			mixer.SetFloat(MusicVolume, toDB(volume));
			PlayerPrefs.SetFloat(MusicVolume, volume);
		}

		public void SetSFXVolume(float volume) {
			mixer.SetFloat(SFXVolume, toDB(volume));
			PlayerPrefs.SetFloat(SFXVolume, volume);
		}

		public float GetMasterVolume() => PlayerPrefs.GetFloat("MasterVolume", 1f);
		public float GetMusicVolume() => PlayerPrefs.GetFloat("MusicVolume", 1f);
		public float GetSFXVolume() => PlayerPrefs.GetFloat("SFXVolume", 1f);

		private void LoadVolumes() {
			SetMasterVolume(GetMasterVolume());
			SetMusicVolume(GetMusicVolume());
			SetSFXVolume(GetSFXVolume());
			Debug.Log("[AudioManager] Loaded volumes: " +
			          $"Master={GetMasterVolume()}, Music={GetMusicVolume()}, SFX={GetSFXVolume()}");
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
		
		public void PlaySFXAtPoint(string clipName, Transform transform, float volume = 1f) {
			var clip = library.GetClip(clipName);
			if (clip == null) {
				Debug.LogWarning("Clip not found: " + clipName);
				return;
			}
			AudioSource.PlayClipAtPoint(clip, transform.position, volume);
		}
	}
}
