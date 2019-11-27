using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    [SerializeField] private string key;
    [SerializeField] private AudioMixer mixer;

    public void SetVolume(float value)
    {
        mixer.SetFloat(key, Mathf.Log10(value) * 20);
    }
}
