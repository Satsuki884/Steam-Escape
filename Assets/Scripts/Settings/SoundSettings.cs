using UnityEngine;
using UnityEngine.UI;

public class SoundSettings : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    private const string VolumePrefKey = "Volume";

    private void Awake()
    {
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
        volumeSlider.value = savedVolume;
        SetVolume(savedVolume);
        volumeSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {
        SetVolume(value);
        PlayerPrefs.SetFloat(VolumePrefKey, value);
        PlayerPrefs.Save();
    }

    private void SetVolume(float value)
    {
        AudioListener.volume = value;
    }

    private void OnDestroy()
    {
        volumeSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }
}