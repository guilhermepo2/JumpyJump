using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource soundEffectsSource;

    public void PlayEffect(AudioClip clip) {
        if(soundEffectsSource != null && clip != null) {
            soundEffectsSource.PlayOneShot(clip);
        }
    }
}
