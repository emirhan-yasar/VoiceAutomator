using UnityEngine;
using UnityEngine.UI;

namespace Voice
{
    public class VoiceMixer : MonoBehaviour
    {
        [SerializeField] private AudioSource vocalSource;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private int clipSamples = 60;
        [SerializeField] private float ratioThreshold = 0.5f;
        [SerializeField] private float voiceLevelThreshold = 0.1f;

        [Header("Canvas")] 
        [SerializeField] private Slider vocalLevelSlider;
        [SerializeField] private Slider musicLevelSlider;
        [SerializeField] private VoiceVisualizer vocalVisualizer;
        [SerializeField] private VoiceVisualizer musicVisualizer;
        [SerializeField] private Slider voiceAdjusterSlider;
        
        private AudioClip _vocalClip;
        private AudioClip _musicClip;

        private bool _isPlaying;
        private void Awake()
        {
            //StartPlaying();
            _vocalClip = vocalSource.clip;
            _musicClip = musicSource.clip;
            musicLevelSlider.value = musicSource.volume;
            vocalLevelSlider.value = vocalSource.volume;
        }

        private void Update()
        {
            if(!_isPlaying) return;
            VoiceLevelAdjuster();
            musicVisualizer.UpdateVisualizer(_musicClip, musicSource.timeSamples);
            vocalVisualizer.UpdateVisualizer(_vocalClip, vocalSource.timeSamples);
        }

        private void VoiceLevelAdjuster()
        {
            var vocalTime = vocalSource.timeSamples;
            var musicTime = musicSource.timeSamples;

            if (vocalTime >= _vocalClip.samples * _vocalClip.channels ||
                musicTime >= _musicClip.samples * _musicClip.channels)
            {
                StopPlaying();
                return;
            }
            
            var vocalClipData = new float[clipSamples];
            _vocalClip.GetData(vocalClipData, vocalTime);
            var vocalVoiceLevel = 0f;
            var vocalCount = 0;
            foreach (var data in vocalClipData)
            {
                if(data < voiceLevelThreshold) continue;
                vocalCount++;
                vocalVoiceLevel += data;
            }

            vocalVoiceLevel = vocalCount == 0 ? 0f : vocalVoiceLevel / vocalCount;

            var musicClipData = new float[clipSamples];
            _musicClip.GetData(musicClipData, musicTime);
            var musicVoiceLevel = 0f;
            var musicCount = 0;
            foreach (var data in musicClipData)
            {
                if(data < voiceLevelThreshold) continue;
                musicCount++;
                musicVoiceLevel += data;
            }
            musicVoiceLevel = musicCount == 0 ? 0f : musicVoiceLevel / musicCount;
            
            
            if(vocalVoiceLevel < voiceLevelThreshold || musicVoiceLevel < voiceLevelThreshold) return;
            
            var musicGreaterVocal = musicVoiceLevel > vocalVoiceLevel;
            var ratio = musicGreaterVocal
                ? vocalVoiceLevel / musicVoiceLevel
                : musicVoiceLevel / vocalVoiceLevel;
        
            if(ratio < ratioThreshold) return;
            vocalSource.volume = 1f;
            musicSource.volume = 1f;
            var musicVolume = musicSource.volume;
            var vocalVolume = vocalSource.volume;
            
            if (musicGreaterVocal)
                musicVolume *= ratio;
            else
                vocalVolume *= ratio;
            
            musicVolume *= Mathf.Clamp01(1 + voiceAdjusterSlider.value);
            vocalVolume *= Mathf.Clamp01(1 - voiceAdjusterSlider.value);
            
            musicSource.volume = musicVolume;
            musicLevelSlider.value = musicVolume;
            
            vocalSource.volume = vocalVolume;
            vocalLevelSlider.value = vocalVolume;
        }

        public void StartPlaying()
        {
            vocalSource.Play();
            musicSource.Play();
            _isPlaying = true;
        }
        
        public void PausePlaying()
        {
            vocalSource.Pause();
            musicSource.Pause();
            _isPlaying = false;
        }
        
        public void StopPlaying()
        {
            vocalSource.Stop();
            musicSource.Stop();
            _isPlaying = false;
        }

        public void Exit()
        {
            Application.Quit();
        }
    }
}
