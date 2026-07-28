using UnityEngine;

namespace Frontier.Audio
{
    /// <summary>
    /// Dynamic audio manager with music layers, spatial audio, weather transitions,
    /// ambient dialogue, UI feedback, and occlusion/reverb zones.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        public AudioSource musicSource;
        public AudioSource sfxSource;
        public AudioSource ambientSource;
        public AudioSource dialogueSource;
        public AudioSource uiSource;

        [Header("Music Layers")]
        public AudioClip explorationMusic;
        public AudioClip combatMusic;
        public AudioClip baseBuildingMusic;
        public AudioClip anomalyMusic;
        
        [Range(0, 1)] public float explorationVolume = 0.8f;
        [Range(0, 1)] public float combatVolume = 0.9f;
        [Range(0, 1)] public float baseBuildingVolume = 0.6f;
        [Range(0, 1)] public float anomalyVolume = 1f;

        [Header("Weather Audio")]
        public AudioClip clearAmbience;
        public AudioClip rainAmbience;
        public AudioClip stormAmbience;
        public AudioClip windAmbience;
        public AudioClip snowAmbience;

        [Header("Settings")]
        public float musicCrossfadeDuration = 2f;
        public float sfxVolume = 0.8f;
        public float ambientVolume = 0.5f;
        public float dialogueVolume = 0.7f;
        public float uiVolume = 0.6f;

        [Header("Reverb Zones")]
        public AudioReverbZone interiorReverb;
        public AudioReverbZone exteriorReverb;
        public AudioReverbZone caveReverb;

        private MusicState _currentMusicState = MusicState.Exploration;
        private WeatherState _currentWeatherState = WeatherState.Clear;
        private float _musicTransitionProgress;

        public enum MusicState
        {
            Exploration, Combat, BaseBuilding, Anomaly, Silent
        }

        public enum WeatherState
        {
            Clear, Rain, Storm, Wind, Snow
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeSources();
        }

        private void InitializeSources()
        {
            if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
            if (ambientSource == null) ambientSource = gameObject.AddComponent<AudioSource>();
            if (dialogueSource == null) dialogueSource = gameObject.AddComponent<AudioSource>();
            if (uiSource == null) uiSource = gameObject.AddComponent<AudioSource>();

            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = explorationVolume;

            ambientSource.loop = true;
            ambientSource.playOnAwake = false;
            ambientSource.volume = ambientVolume;
        }

        private void Update()
        {
            // Handle music transitions
            if (_musicTransitionProgress < 1f)
            {
                _musicTransitionProgress += Time.deltaTime / musicCrossfadeDuration;
                // Crossfade logic would go here
            }
        }

        #region Music Control

        public void SetMusicState(MusicState state)
        {
            if (state == _currentMusicState) return;

            _currentMusicState = state;
            _musicTransitionProgress = 0f;

            switch (state)
            {
                case MusicState.Exploration:
                    CrossfadeToMusic(explorationMusic, explorationVolume);
                    break;
                case MusicState.Combat:
                    CrossfadeToMusic(combatMusic, combatVolume);
                    break;
                case MusicState.BaseBuilding:
                    CrossfadeToMusic(baseBuildingMusic, baseBuildingVolume);
                    break;
                case MusicState.Anomaly:
                    CrossfadeToMusic(anomalyMusic, anomalyVolume);
                    break;
                case MusicState.Silent:
                    FadeOutMusic();
                    break;
            }
        }

        private void CrossfadeToMusic(AudioClip clip, float targetVolume)
        {
            if (musicSource.clip != clip && clip != null)
            {
                musicSource.clip = clip;
                musicSource.Play();
            }
            musicSource.volume = targetVolume;
        }

        private void FadeOutMusic()
        {
            musicSource.volume = 0f;
        }

        #endregion

        #region SFX Control

        public void PlaySFX(AudioClip clip, Vector3 position, float volumeScale = 1f)
        {
            AudioSource source = GetSpatialSource(position);
            source.PlayOneShot(clip, sfxVolume * volumeScale);
        }

        public void PlaySFX2D(AudioClip clip, float volumeScale = 1f)
        {
            sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
        }

        private AudioSource GetSpatialSource(Vector3 position)
        {
            GameObject go = new GameObject("TempSFX");
            go.transform.position = position;
            go.transform.parent = transform;
            AudioSource source = go.AddComponent<AudioSource>();
            source.spatialBlend = 1f;
            source.minDistance = 5f;
            source.maxDistance = 50f;
            Destroy(go, 3f);
            return source;
        }

        #endregion

        #region Ambient/Weather Audio

        public void SetWeatherState(WeatherState state)
        {
            if (state == _currentWeatherState) return;
            _currentWeatherState = state;

            switch (state)
            {
                case WeatherState.Clear:
                    CrossfadeAmbience(clearAmbience);
                    break;
                case WeatherState.Rain:
                    CrossfadeAmbience(rainAmbience);
                    break;
                case WeatherState.Storm:
                    CrossfadeAmbience(stormAmbience);
                    break;
                case WeatherState.Wind:
                    CrossfadeAmbience(windAmbience);
                    break;
                case WeatherState.Snow:
                    CrossfadeAmbience(snowAmbience);
                    break;
            }
        }

        private void CrossfadeAmbience(AudioClip clip)
        {
            if (clip != null)
            {
                ambientSource.clip = clip;
                ambientSource.Play();
            }
        }

        #endregion

        #region Dialogue & UI

        public void PlayDialogue(AudioClip clip)
        {
            if (dialogueSource.isPlaying)
            {
                dialogueSource.Stop();
            }
            dialogueSource.clip = clip;
            dialogueSource.volume = dialogueVolume;
            dialogueSource.Play();
        }

        public void StopDialogue()
        {
            dialogueSource.Stop();
        }

        public void PlayUISound(AudioClip clip)
        {
            uiSource.PlayOneShot(clip, uiVolume);
        }

        #endregion

        #region Reverb Zones

        public void SetReverbZone(ReverbZoneType zoneType)
        {
            switch (zoneType)
            {
                case ReverbZoneType.Interior:
                    EnableReverb(interiorReverb);
                    break;
                case ReverbZoneType.Exterior:
                    EnableReverb(exteriorReverb);
                    break;
                case ReverbZoneType.Cave:
                    EnableReverb(caveReverb);
                    break;
            }
        }

        private void EnableReverb(AudioReverbZone zone)
        {
            if (zone != null)
            {
                zone.enabled = true;
            }
        }

        public enum ReverbZoneType
        {
            Interior, Exterior, Cave
        }

        #endregion

        #region Volume Control

        public void SetMasterVolume(float volume)
        {
            AudioListener.volume = Mathf.Clamp01(volume);
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }

        public void SetAmbientVolume(float volume)
        {
            ambientVolume = Mathf.Clamp01(volume);
            ambientSource.volume = ambientVolume;
        }

        public void SetDialogueVolume(float volume)
        {
            dialogueVolume = Mathf.Clamp01(volume);
            dialogueSource.volume = dialogueVolume;
        }

        #endregion
    }
}
