using UnityEngine;
using UnityEngine.UI;

namespace Voice
{
    public class VoiceVisualizer : MonoBehaviour
    {
        [SerializeField] private Slider[] sliders;
        [SerializeField] private float volumeMultiplier = 3;
        [SerializeField] private int sampleRate = 64;

        private int _middleSliderIndex;

        private void Awake()
        {
            _middleSliderIndex = sliders.Length / 2;
        }
        
        public void UpdateVisualizer(AudioClip clip, int timeSample)
        {
            var sliderArrays = new float[sliders.Length];
            for (int i = -_middleSliderIndex; i < sliders.Length - _middleSliderIndex; i++)
            {
                var position = timeSample + i * sampleRate;
                if(position < 0 || position > clip.samples * clip.channels) continue;
                var volumeData = new float[sampleRate];
                clip.GetData(volumeData, position);
                var volume = 0f;
                foreach (var data in volumeData)
                    volume += Mathf.Abs(data);
                volume /= sampleRate;
                sliderArrays[i + _middleSliderIndex] = volume;
                sliders[i + _middleSliderIndex].value = volume * volumeMultiplier;
            }

        }
    }
}
