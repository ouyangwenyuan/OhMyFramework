using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace DragonU3DSDK.Audio
{
    /// <summary>
    /// 音效管理器，提供统一的音乐音效播放基础封装。暂自我管理单例。
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance = null;

        private static float vol = 1f;
        private static float musicVol = 1f;
        private static float soundsVol = 1f;

        private static Dictionary<int, Audio> musicAudio;
        private static Dictionary<int, Audio> soundsAudio;

        private static int nativeAudioInitID = -100;

        private static bool musicClose = false;
        private static bool soundClose = false;

        private static bool initialized = false;
        
        private static List<int> _toBeRemoved = new List<int>();

        private static AudioManager instance
        {
            get
            {
//                if (_instance == null)
//                {
//                    _instance = (AudioManager)FindObjectOfType(typeof(AudioManager));
//                    if (_instance == null)
//                    {
//                        // Create gameObject and add component
//                        _instance = (new GameObject("AudioManager")).AddComponent<AudioManager>();
//#if UNITY_EDITOR
//                        var listener = FindObjectOfType(typeof(AudioListener));
//                        if (!listener)
//                            _instance.gameObject.AddComponent<AudioListener>();
//#endif
//                    }
//                }
                return _instance;
            }
        }

        private void Awake()
        {
            _instance = this;
            instance.Init();
        }

        private static int GetNativeAudioId()
        {
            return nativeAudioInitID--;
        }

        public static bool MusicClose
        {
            get
            {
                return musicClose;
            }
            set
            {
                musicClose = value;
                if (value)
                {
                    PauseAllMusic();
                }
                else
                {
                    ResumeAllMusic();
                }
            }
        }

        public static bool SoundClose
        {
            get
            {
                return soundClose;
            }
            set
            {
                soundClose = value;
                if (value)
                {
                    StopAllSounds();
                }
            }
        }

        /// <summary>
        /// The gameobject that the sound manager is attached to
        /// </summary>
        public static GameObject gameobject { get { return instance.gameObject; } }

        /// <summary>
        /// When set to true, new Audios that have the same audio clip as any other Audio, will be ignored
        /// </summary>
        public static bool ignoreDuplicateMusic { get; set; }

        /// <summary>
        /// When set to true, new Audios that have the same audio clip as any other Audio, will be ignored
        /// </summary>
        public static bool ignoreDuplicateSounds { get; set; }


        /// <summary>
        /// Global volume
        /// </summary>
        public static float globalVolume
        {
            get
            {
                return vol;
            }
            set
            {
                vol = value;
            }
        }

        /// <summary>
        /// Global music volume
        /// </summary>
        public static float globalMusicVolume
        {
            get
            {
                return musicVol;
            }
            set
            {
                musicVol = value;
            }
        }

        /// <summary>
        /// Global sounds volume
        /// </summary>
        public static float globalSoundsVolume
        {
            get
            {
                return soundsVol;
            }
            set
            {
                soundsVol = value;
            }
        }

//        static AudioManager()
//        {
//            instance.Init();
//        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            List<int> keys;
            // Stop and remove all non-persistent music audio
            keys = new List<int>(musicAudio.Keys);
            foreach (var key in keys)
            {
                if (musicAudio.ContainsKey(key))
                {
                    var music = musicAudio[key];
                    if (!music.persist && music.activated)
                    {
                        Destroy(music.audioSource);
                        musicAudio.Remove(key);
                    }
                }
            }
            // Stop sound fx
            keys = new List<int>(soundsAudio.Keys);
            foreach (var key in keys)
            {
                if (soundsAudio.ContainsKey(key))
                {
                    var sound = soundsAudio[key];
                    Destroy(sound.audioSource);
                    soundsAudio.Remove(key);
                }
            }
        }

        void Update()
        {
            var e = musicAudio.GetEnumerator();
            while (e.MoveNext())
            {
                var music = e.Current.Value;
                music.Update();
                if (!music.playing && !music.paused)
                {
                    Destroy(music.audioSource);
                    _toBeRemoved.Add(e.Current.Key);
                }
            }

            if (_toBeRemoved.Count > 0)
            {
                for (int i = 0; i < _toBeRemoved.Count; i++)
                {
                    musicAudio.Remove(_toBeRemoved[i]);
                }
                _toBeRemoved.Clear();
            }
            
            // Update sound fx            
            e = soundsAudio.GetEnumerator();
            while (e.MoveNext())
            {
                var sound = e.Current.Value;
                sound.Update();
                if (!sound.playing && !sound.paused)
                {
                    Destroy(sound.audioSource);
                    _toBeRemoved.Add(e.Current.Key);
                }
            }

            if (_toBeRemoved.Count > 0)
            {
                for (int i = 0; i < _toBeRemoved.Count; i++)
                {
                    soundsAudio.Remove(_toBeRemoved[i]);
                }
                _toBeRemoved.Clear();
            }
        }

        void Init()
        {
            if (!initialized)
            {
                musicAudio = new Dictionary<int, Audio>();
                soundsAudio = new Dictionary<int, Audio>();

                ignoreDuplicateMusic = false;
                ignoreDuplicateSounds = false;

                initialized = true;
                DontDestroyOnLoad(this);
            }
        }


        #region GetAudio Functions

        /// <summary>
        /// Returns the Audio that has as its id the audioID if one is found, returns null if no such Audio is found
        /// </summary>
        /// <param name="audioID">The id of the Audio to be retrieved</param>
        /// <returns>Audio that has as its id the audioID, null if no such Audio is found</returns>
        public static Audio GetAudio(int audioID)
        {
            Audio audio;

            audio = GetMusicAudio(audioID);
            if (audio != null)
            {
                return audio;
            }

            audio = GetSoundAudio(audioID);
            if (audio != null)
            {
                return audio;
            }

            return null;
        }

        /// <summary>
        /// Returns the first occurrence of Audio that plays the given audioClip. Returns null if no such Audio is found
        /// </summary>
        /// <param name="audioClip">The audio clip of the Audio to be retrieved</param>
        /// <returns>First occurrence of Audio that has as plays the audioClip, null if no such Audio is found</returns>
        public static Audio GetAudio(AudioClip audioClip)
        {
            Audio audio = GetMusicAudio(audioClip);
            if (audio != null)
            {
                return audio;
            }

            audio = GetSoundAudio(audioClip);
            if (audio != null)
            {
                return audio;
            }

            return null;
        }

        /// <summary>
        /// Returns the music Audio that has as its id the audioID if one is found, returns null if no such Audio is found
        /// </summary>
        /// <param name="audioID">The id of the music Audio to be returned</param>
        /// <returns>Music Audio that has as its id the audioID if one is found, null if no such Audio is found</returns>
        public static Audio GetMusicAudio(int audioID)
        {
            List<int> keys = new List<int>(musicAudio.Keys);
            foreach (int key in keys)
            {
                if (audioID == key)
                {
                    return musicAudio[key];
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the first occurrence of music Audio that plays the given audioClip. Returns null if no such Audio is found
        /// </summary>
        /// <param name="audioClip">The audio clip of the music Audio to be retrieved</param>
        /// <returns>First occurrence of music Audio that has as plays the audioClip, null if no such Audio is found</returns>
        public static Audio GetMusicAudio(AudioClip audioClip)
        {
            List<int> keys;
            keys = new List<int>(musicAudio.Keys);
            foreach (int key in keys)
            {
                Audio audio = musicAudio[key];
                if (audio.clip == audioClip)
                {
                    return audio;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the sound fx Audio that has as its id the audioID if one is found, returns null if no such Audio is found
        /// </summary>
        /// <param name="audioID">The id of the sound fx Audio to be returned</param>
        /// <returns>Sound fx Audio that has as its id the audioID if one is found, null if no such Audio is found</returns>
        public static Audio GetSoundAudio(int audioID)
        {
            List<int> keys = new List<int>(soundsAudio.Keys);
            foreach (int key in keys)
            {
                if (audioID == key)
                {
                    return soundsAudio[key];
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the first occurrence of sound Audio that plays the given audioClip. Returns null if no such Audio is found
        /// </summary>
        /// <param name="audioClip">The audio clip of the sound Audio to be retrieved</param>
        /// <returns>First occurrence of sound Audio that has as plays the audioClip, null if no such Audio is found</returns>
        public static Audio GetSoundAudio(AudioClip audioClip)
        {
            List<int> keys;
            keys = new List<int>(soundsAudio.Keys);
            foreach (int key in keys)
            {
                Audio audio = soundsAudio[key];
                if (audio.clip == audioClip)
                {
                    return audio;
                }
            }

            return null;
        }


        #endregion

        #region Play Functions


        /// <summary>
        /// Play background music
        /// </summary>
        /// <param name="clip">The audio clip to play</param>
        /// <returns>The ID of the created Audio object</returns>
        public static int PlayMusic(AudioClip clip)
        {
            return PlayMusic(clip, 1f, false, false, 1f, 1f, -1f, null);
        }

        /// <summary>
        /// Play background music
        /// </summary>
        /// <param name="clip">The audio clip to play</param>
        /// <param name="volume"> The volume the music will have</param>
        /// <returns>The ID of the created Audio object</returns>
        public static int PlayMusic(AudioClip clip, float volume)
        {
            return PlayMusic(clip, volume, false, false, 1f, 1f, -1f, null);
        }

        /// <summary>
        /// Play background music
        /// </summary>
        /// <param name="clip">The audio clip to play</param>
        /// <param name="volume"> The volume the music will have</param>
        /// <param name="loop">Wether the music is looped</param>
        /// <param name = "persist" > Whether the audio persists in between scene changes</param>
        /// <returns>The ID of the created Audio object</returns>
        public static int PlayMusic(AudioClip clip, float volume, bool loop, bool persist)
        {
            return PlayMusic(clip, volume, loop, persist, 1f, 1f, -1f, null);
        }

        /// <summary>
        /// Play background music
        /// </summary>
        /// <param name="clip">The audio clip to play</param>
        /// <param name="volume"> The volume the music will have</param>
        /// <param name="loop">Wether the music is looped</param>
        /// <param name="persist"> Whether the audio persists in between scene changes</param>
        /// <param name="fadeInValue">How many seconds it needs for the audio to fade in/ reach target volume (if higher than current)</param>
        /// <param name="fadeOutValue"> How many seconds it needs for the audio to fade out/ reach target volume (if lower than current)</param>
        /// <returns>The ID of the created Audio object</returns>
        public static int PlayMusic(AudioClip clip, float volume, bool loop, bool persist, float fadeInSeconds, float fadeOutSeconds)
        {
            return PlayMusic(clip, volume, loop, persist, fadeInSeconds, fadeOutSeconds, -1f, null);
        }

        /// <summary>
        /// Play background music
        /// </summary>
        /// <param name="clip">The audio clip to play</param>
        /// <param name="volume"> The volume the music will have</param>
        /// <param name="loop">Wether the music is looped</param>
        /// <param name="persist"> Whether the audio persists in between scene changes</param>
        /// <param name="fadeInValue">How many seconds it needs for the audio to fade in/ reach target volume (if higher than current)</param>
        /// <param name="fadeOutValue"> How many seconds it needs for the audio to fade out/ reach target volume (if lower than current)</param>
        /// <param name="currentMusicfadeOutSeconds"> How many seconds it needs for current music audio to fade out. It will override its own fade out seconds. If -1 is passed, current music will keep its own fade out seconds</param>
        /// <param name="sourceTransform">The transform that is the source of the music (will become 3D audio). If 3D audio is not wanted, use null</param>
        /// <returns>The ID of the created Audio object</returns>
        public static int PlayMusic(AudioClip clip, float volume, bool loop, bool persist, float fadeInSeconds, float fadeOutSeconds, float currentMusicfadeOutSeconds, Transform sourceTransform)
        {
            if (clip == null)
            {
                Debug.Log("Sound Manager: Audio clip is null, cannot play music", clip);
            }

            if (ignoreDuplicateMusic)
            {
                List<int> keys = new List<int>(musicAudio.Keys);
                foreach (int key in keys)
                {
                    if (musicAudio[key].audioSource.clip == clip)
                    {
                        return musicAudio[key].audioID;
                    }
                }
            }

            // Stop all current music playing
            StopAllMusic(currentMusicfadeOutSeconds);

            // Create the audioSource
            Audio audio = new Audio(Audio.AudioType.Music, clip, loop, persist, volume, fadeInSeconds, fadeOutSeconds, sourceTransform);

            // Add it to music list
            musicAudio.Add(audio.audioID, audio);

            if (MusicClose)
            {
                audio.Pause();
            }

            return audio.audioID;
        }


        public static int PlaySound(string soundName)
        {
            //TODO use Resources.Load not a good idea
            AudioClip sound = Resources.Load<AudioClip>(soundName);
            return PlaySound(sound);
        }

        /// <summary>
        /// Play a sound fx
        /// </summary>
        /// <param name="clip">The audio clip to play</param>
        /// <returns>The ID of the created Audio object</returns>
        public static int PlaySound(AudioClip clip)
        {
            return PlaySound(clip, 1f, false, null);
        }

        /// <summary>
        /// Play a sound fx
        /// </summary>
        /// <param name="clip">The audio clip to play</param>
        /// <param name="volume"> The volume the music will have</param>
        /// <returns>The ID of the created Audio object</returns>
        public static int PlaySound(AudioClip clip, float volume)
        {
            return PlaySound(clip, volume, false, null);
        }

        /// <summary>
        /// Play a sound fx
        /// </summary>
        /// <param name="clip">The audio clip to play</param>
        /// <param name="loop">Wether the sound is looped</param>
        /// <returns>The ID of the created Audio object</returns>
        public static int PlaySound(AudioClip clip, bool loop)
        {
            return PlaySound(clip, 1f, loop, null);
        }

        public static int PlaySound(AudioClip clip, bool loop, float pitch)
        {
            return PlaySound(clip, 1.0f, loop, null, pitch);
        }

        /// <summary>
        /// Play a sound fx
        /// </summary>
        /// <param name="clip">The audio clip to play</param>
        /// <param name="volume"> The volume the music will have</param>
        /// <param name="loop">Wether the sound is looped</param>
        /// <param name="sourceTransform">The transform that is the source of the sound (will become 3D audio). If 3D audio is not wanted, use null</param>
        /// <returns>The ID of the created Audio object</returns>
        public static int PlaySound(AudioClip clip, float volume, bool loop, Transform sourceTransform, float pitch = 1.0f)
        {
            if (clip == null)
            {
                Debug.LogWarning("Sound Manager: Audio clip is null, cannot play music", clip);
            }

            if (SoundClose)
            {
                return -1;
            }

            if (ignoreDuplicateSounds)
            {
                List<int> keys = new List<int>(soundsAudio.Keys);
                foreach (var key in keys)
                {
                    if (soundsAudio.ContainsKey(key) && soundsAudio[key].audioSource.clip == clip)
                        return soundsAudio[key].audioID;
                }
            }

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID
            // Create the audioSource
            Audio audio = new Audio(Audio.AudioType.Sound, clip, loop, false, volume, 0f, 0f, sourceTransform);
            audio.SetPitch(pitch);
            // Add it to music list
            soundsAudio.Add(audio.audioID, audio);
            return audio.audioID;
#else
            return -1;
#endif
        }


#endregion

#region Stop Functions

        /// <summary>
        /// Stop all audio playing
        /// </summary>
        public static void StopAll()
        {
            StopAll(-1f);
        }

        /// <summary>
        /// Stop all audio playing
        /// </summary>
        /// <param name="fadeOutSeconds"> How many seconds it needs for all music audio to fade out. It will override  their own fade out seconds. If -1 is passed, all music will keep their own fade out seconds</param>
        public static void StopAll(float fadeOutSeconds)
        {
            StopAllMusic(fadeOutSeconds);
            StopAllSounds();
        }

        /// <summary>
        /// Stop all music playing
        /// </summary>
        public static void StopAllMusic()
        {
            StopAllMusic(-1f);
        }

        /// <summary>
        /// Stop all music playing
        /// </summary>
        /// <param name="fadeOutSeconds"> How many seconds it needs for all music audio to fade out. It will override  their own fade out seconds. If -1 is passed, all music will keep their own fade out seconds</param>
        public static void StopAllMusic(float fadeOutSeconds)
        {
            List<int> keys = new List<int>(musicAudio.Keys);
            foreach (int key in keys)
            {
                Audio audio = musicAudio[key];
                if (fadeOutSeconds > 0)
                {
                    audio.fadeOutSeconds = fadeOutSeconds;
                }
                audio.Stop();
            }
        }

        /// <summary>
        /// Stop all sound fx playing
        /// </summary>
        public static void StopAllSounds()
        {
            List<int> keys = new List<int>(soundsAudio.Keys);
            foreach (int key in keys)
            {
                Audio audio = soundsAudio[key];
                audio.Stop();
            }
        }

        public static void StopSoundById(int soundId)
        {
            if (soundId >= 0)
            {
                List<int> keys = new List<int>(soundsAudio.Keys);
                foreach (int key in keys)
                {
                    Audio audio = soundsAudio[key];
                    if (audio.audioID == soundId)
                    {
                        audio.Stop();
                    }
                }
            }
        }

#endregion

#region Pause Functions

        /// <summary>
        /// Pause all audio playing
        /// </summary>
        public static void PauseAll()
        {
            PauseAllMusic();
            PauseAllSounds();
        }

        /// <summary>
        /// Pause all music playing
        /// </summary>
        public static void PauseAllMusic()
        {
            List<int> keys = new List<int>(musicAudio.Keys);
            foreach (int key in keys)
            {
                Audio audio = musicAudio[key];
                audio.Pause();
            }
        }

        /// <summary>
        /// Pause all sound fx playing
        /// </summary>
        public static void PauseAllSounds()
        {
            List<int> keys = new List<int>(soundsAudio.Keys);
            foreach (int key in keys)
            {
                Audio audio = soundsAudio[key];
                audio.Pause();
            }
        }

#endregion

#region Resume Functions

        /// <summary>
        /// Resume all audio playing
        /// </summary>
        public static void ResumeAll()
        {
            ResumeAllMusic();
            ResumeAllSounds();
        }

        /// <summary>
        /// Resume all music playing
        /// </summary>
        public static void ResumeAllMusic()
        {
        
            if (MusicClose)
                return;

            List<int> keys = new List<int>(musicAudio.Keys);
            foreach (int key in keys)
            {
                Audio audio = musicAudio[key];
                audio.Resume();
            }
        }

        /// <summary>
        /// Resume all sound fx playing
        /// </summary>
        public static void ResumeAllSounds()
        {
            List<int> keys = new List<int>(soundsAudio.Keys);
            foreach (int key in keys)
            {
                Audio audio = soundsAudio[key];
                audio.Resume();
            }
        }


#endregion
    }

    public class Audio
    {
        private static int audioCounter = 0;
        private float volume;
        private float targetVolume;
        private float initTargetVolume;
        private float tempFadeSeconds;
        private float fadeInterpolater;
        private float onFadeStartVolume;
        private AudioType audioType;
        private AudioClip initClip;
        private Transform sourceTransform;

        /// <summary>
        /// The ID of the Audio
        /// </summary>
        public int audioID { get; private set; }

        /// <summary>
        /// The audio source that is responsible for this audio
        /// </summary>
        public AudioSource audioSource { get; private set; }

        /// <summary>
        /// Audio clip to play/is playing
        /// </summary>
        public AudioClip clip
        {
            get
            {
                return audioSource == null ? initClip : audioSource.clip;
            }
        }

        /// <summary>
        /// Whether the audio will be lopped
        /// </summary>
        public bool loop { get; set; }

        /// <summary>
        /// Whether the audio persists in between scene changes
        /// </summary>
        public bool persist { get; set; }

        /// <summary>
        /// How many seconds it needs for the audio to fade in/ reach target volume (if higher than current)
        /// </summary>
        public float fadeInSeconds { get; set; }

        /// <summary>
        /// How many seconds it needs for the audio to fade out/ reach target volume (if lower than current)
        /// </summary>
        public float fadeOutSeconds { get; set; }

        /// <summary>
        /// Whether the audio is currently playing
        /// </summary>
        public bool playing { get; set; }

        /// <summary>
        /// Whether the audio is paused
        /// </summary>
        public bool paused { get; private set; }

        /// <summary>
        /// Whether the audio is stopping
        /// </summary>
        public bool stopping { get; private set; }

        /// <summary>
        /// Whether the audio is created and updated at least once. 
        /// </summary>
        public bool activated { get; private set; }

        public enum AudioType
        {
            Music,
            Sound,
        }

        public Audio(AudioType audioType, AudioClip clip, bool loop, bool persist, float volume, float fadeInValue, float fadeOutValue, Transform sourceTransform)
        {
            if (sourceTransform == null)
            {
                this.sourceTransform = AudioManager.gameobject.transform;
            }
            else
            {
                this.sourceTransform = sourceTransform;
            }

            this.audioID = audioCounter;
            audioCounter++;

            this.audioType = audioType;
            this.initClip = clip;
            this.loop = loop;
            this.persist = persist;
            this.targetVolume = volume;
            this.initTargetVolume = volume;
            this.tempFadeSeconds = -1;
            this.volume = 0f;
            this.fadeInSeconds = fadeInValue;
            this.fadeOutSeconds = fadeOutValue;

            this.playing = false;
            this.paused = false;
            this.activated = false;

            CreateAudiosource(clip, loop);
            Play();
        }

        void CreateAudiosource(AudioClip _clip, bool _loop)
        {
            audioSource = sourceTransform.gameObject.AddComponent<AudioSource>() as AudioSource;

            audioSource.clip = _clip;
            audioSource.loop = _loop;
            audioSource.volume = 0f;
            if (sourceTransform != AudioManager.gameobject.transform)
            {
                audioSource.spatialBlend = 1;
            }
        }

        // 音效播放加速
        public void SetPitch(float _pitch)
        {
            if (audioSource != null)
            {
                audioSource.pitch = _pitch;
            }
        }

        /// <summary>
        /// Start playing audio clip from the beggining
        /// </summary>
        public void Play()
        {
            Play(initTargetVolume);
        }

        /// <summary>
        /// Start playing audio clip from the beggining
        /// </summary>
        /// <param name="volume">The target volume</param>
        public void Play(float volume)
        {
            if (audioSource == null)
            {
                CreateAudiosource(initClip, loop);
            }

            audioSource.Play();
            playing = true;

            fadeInterpolater = 0f;
            onFadeStartVolume = this.volume;
            targetVolume = volume;
        }

        /// <summary>
        /// Stop playing audio clip
        /// </summary>
        public void Stop()
        {
            fadeInterpolater = 0f;
            onFadeStartVolume = volume;
            targetVolume = 0f;

            stopping = true;
        }

        /// <summary>
        /// Pause playing audio clip
        /// </summary>
        public void Pause()
        {
            audioSource.Pause();
            paused = true;
        }

        /// <summary>
        /// Resume playing audio clip
        /// </summary>
        public void UnPause()
        {
            audioSource.UnPause();
            paused = false;
        }

        /// <summary>
        /// Resume playing audio clip
        /// </summary>
        public void Resume()
        {
            audioSource.UnPause();
            paused = false;
        }

        /// <summary>
        /// Sets the audio volume
        /// </summary>
        /// <param name="volume">The target volume</param>
        public void SetVolume(float volume)
        {
            if (volume > targetVolume)
            {
                SetVolume(volume, fadeOutSeconds);
            }
            else
            {
                SetVolume(volume, fadeInSeconds);
            }
        }

        /// <summary>
        /// Sets the audio volume
        /// </summary>
        /// <param name="volume">The target volume</param>
        /// <param name="fadeSeconds">How many seconds it needs for the audio to fade in/out to reach target volume. If passed, it will override the Audio's fade in/out seconds, but only for this transition</param>
        public void SetVolume(float volume, float fadeSeconds)
        {
            SetVolume(volume, fadeSeconds, this.volume);
        }

        /// <summary>
        /// Sets the audio volume
        /// </summary>
        /// <param name="volume">The target volume</param>
        /// <param name="fadeSeconds">How many seconds it needs for the audio to fade in/out to reach target volume. If passed, it will override the Audio's fade in/out seconds, but only for this transition</param>
        /// <param name="startVolume">Immediately set the volume to this value before beginning the fade. If not passed, the Audio will start fading from the current volume towards the target volume</param>
        public void SetVolume(float volume, float fadeSeconds, float startVolume)
        {
            targetVolume = Mathf.Clamp01(volume);
            fadeInterpolater = 0;
            onFadeStartVolume = startVolume;
            tempFadeSeconds = fadeSeconds;
        }

        /// <summary>
        /// Sets the Audio 3D max distance
        /// </summary>
        /// <param name="max">the max distance</param>
        public void Set3DMaxDistance(float max)
        {
            audioSource.maxDistance = max;
        }

        /// <summary>
        /// Sets the Audio 3D min distance
        /// </summary>
        /// <param name="max">the min distance</param>
        public void Set3DMinDistance(float min)
        {
            audioSource.minDistance = min;
        }

        /// <summary>
        /// Sets the Audio 3D distances
        /// </summary>
        /// <param name="min">the min distance</param>
        /// <param name="max">the max distance</param>
        public void Set3DDistances(float min, float max)
        {
            Set3DMinDistance(min);
            Set3DMaxDistance(max);
        }

        public void Update()
        {
            if (audioSource == null)
            {
                return;
            }

            activated = true;

            if (volume != targetVolume)
            {
                float fadeValue;
                fadeInterpolater += Time.unscaledDeltaTime;
                if (volume > targetVolume)
                {
                    fadeValue = tempFadeSeconds != -1 ? tempFadeSeconds : fadeOutSeconds;
                }
                else
                {
                    fadeValue = tempFadeSeconds != -1 ? tempFadeSeconds : fadeInSeconds;
                }

                volume = Mathf.Lerp(onFadeStartVolume, targetVolume, fadeInterpolater / fadeValue);
            }
            else if (tempFadeSeconds != -1)
            {
                tempFadeSeconds = -1;
            }

            switch (audioType)
            {
                case AudioType.Music:
                    {
                        audioSource.volume = volume * AudioManager.globalMusicVolume * AudioManager.globalVolume;
                        break;
                    }
                case AudioType.Sound:
                    {
                        audioSource.volume = volume * AudioManager.globalSoundsVolume * AudioManager.globalVolume;
                        break;
                    }
            }

            if (volume == 0f && stopping)
            {
                audioSource.Stop();
                stopping = false;
                playing = false;
                paused = false;
            }

            // Update playing status
            if (audioSource.isPlaying != playing && Application.isFocused)
            {
                playing = audioSource.isPlaying;
            }
        }
    }
}
