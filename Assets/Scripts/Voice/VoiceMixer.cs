using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
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
        
        private AudioClip _vocalClip;
        private AudioClip _musicClip;

        private void Awake()
        {
            _vocalClip = vocalSource.clip;
            _musicClip = musicSource.clip;
            musicLevelSlider.value = musicSource.volume;
            vocalLevelSlider.value = vocalSource.volume;
        }

        private void Update()
        {
            var vocalTime = vocalSource.timeSamples;
            var musicTime = musicSource.timeSamples;

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
            if (musicGreaterVocal)
            {
                var musicVolume = musicSource.volume;
                musicVolume *= ratio;
                musicSource.volume = musicVolume;
                musicLevelSlider.value = musicVolume;
                return;
            }
            
            var vocalVolume = vocalSource.volume;
            vocalVolume *= ratio;
            vocalSource.volume = vocalVolume;
            vocalLevelSlider.value = vocalVolume;

        }
    }
}
