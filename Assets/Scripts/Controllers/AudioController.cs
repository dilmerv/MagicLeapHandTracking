using System.Collections;
using System.Collections.Generic;
using MagicLeapHandTracking.Assets.Scripts.Core;
using UnityEngine;

public class AudioController : MonoBehaviourSingleton<AudioController>
{
    [SerializeField]
    private AudioSource pitchEffect;

    public void PlayPitch()
    {
        pitchEffect.Play();
    }
}