using UnityEngine;
using UnityEngine.Audio;

namespace Audio {
    /// <summary>
    /// Centralized manager for all audio playback and volume control within the game.
    /// Handles background music, sound effects, and persistent user volume preferences.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour {
        
        public static AudioManager Instance;
		
        [Header("References")]
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private AudioLibrary library;
		
        private AudioSource _musicAudioSource;

        // Volume parameter names in the Audio Mixer
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

        /// <summary>
        /// Starts looping background music and restores previously saved volume settings.
        /// </summary>
        public void Start() {
            _musicAudioSource.loop = true;
            _musicAudioSource.clip = library.GetClip("GameMusic");
            _musicAudioSource.Play();

            LoadVolumes();
        }

        /// <summary>
        /// Sets the master volume and saves it to <see cref="PlayerPrefs"/>.
        /// </summary>
        public void SetMasterVolume(float volume) {
            mixer.SetFloat(MasterVolume, toDB(volume));
            PlayerPrefs.SetFloat(MasterVolume, volume);
        }

        /// <summary>
        /// Sets the music volume and saves it to <see cref="PlayerPrefs"/>.
        /// </summary>
        public void SetMusicVolume(float volume) {
            mixer.SetFloat(MusicVolume, toDB(volume));
            PlayerPrefs.SetFloat(MusicVolume, volume);
        }

        /// <summary>
        /// Sets the SFX volume and saves it to <see cref="PlayerPrefs"/>.
        /// </summary>
        public void SetSFXVolume(float volume) {
            mixer.SetFloat(SFXVolume, toDB(volume));
            PlayerPrefs.SetFloat(SFXVolume, volume);
        }

        /// <summary>
        /// Retrieves the last saved master volume from <see cref="PlayerPrefs"/>.
        /// </summary>
        public float GetMasterVolume() => PlayerPrefs.GetFloat("MasterVolume", 1f);

        /// <summary>
        /// Retrieves the last saved music volume from <see cref="PlayerPrefs"/>.
        /// </summary>
        public float GetMusicVolume() => PlayerPrefs.GetFloat("MusicVolume", 1f);

        /// <summary>
        /// Retrieves the last saved SFX volume from <see cref="PlayerPrefs"/>.
        /// </summary>
        public float GetSFXVolume() => PlayerPrefs.GetFloat("SFXVolume", 1f);

        /// <summary>
        /// Loads and applies saved volume settings for all mixer channels.
        /// </summary>
        private void LoadVolumes() {
            SetMasterVolume(GetMasterVolume());
            SetMusicVolume(GetMusicVolume());
            SetSFXVolume(GetSFXVolume());
            Debug.Log("[AudioManager] Loaded volumes: " +
			          $"Master={GetMasterVolume()}, Music={GetMusicVolume()}, SFX={GetSFXVolume()}");
        }

        /// <summary>
        /// Converts a normalized slider value (0–1) to decibel (dB) for the Audio Mixer.
        /// </summary>
        private static float toDB(float sliderValue) => Mathf.Lerp(-80f, 0f, Mathf.Pow(sliderValue, 0.25f));

        /// <summary>
        /// Plays a one-shot sound effect through the specified <see cref="AudioSource"/>.
        /// </summary>
        /// <param name="clipName">Key of the audio clip to play, as defined in the <see cref="AudioLibrary"/>.</param>
        /// <param name="source">Audio source used to play the clip.</param>
        /// <param name="volume">Playback volume multiplier (default = 1f).</param>
        public void PlaySFX(string clipName, AudioSource source, float volume = 1f) {
            var clip = library.GetClip(clipName);
            if (clip == null) {
                Debug.LogWarning("Clip not found: " + clipName);
                return;
            }
            source.PlayOneShot(clip, volume);
        }
		
        /// <summary>
        /// Plays a 3D positional sound effect at a given transform's position.
        /// </summary>
        /// <param name="clipName">Key of the audio clip to play, as defined in the <see cref="AudioLibrary"/>.</param>
        /// <param name="t">World-space position of the sound.</param>
        /// <param name="volume">Playback volume multiplier (default = 1f).</param>
        public void PlaySFXAtPoint(string clipName, Transform t, float volume = 1f) {
            var clip = library.GetClip(clipName);
            if (!clip) {
                Debug.LogWarning("Clip not found: " + clipName);
                return;
            }
            AudioSource.PlayClipAtPoint(clip, t.position, volume);
        }
    }
}
