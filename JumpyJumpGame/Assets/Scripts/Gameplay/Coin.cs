using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour, IInteraction {
    public AudioClip coinCollectedClip;

    private SoundManager m_soundManager;

    void Start() {
        m_soundManager = FindObjectOfType<SoundManager>();
    }

    public void Interact() {
        if (m_soundManager && coinCollectedClip) {
            m_soundManager.PlayEffect(coinCollectedClip);
        }

        Destroy(gameObject);
    }
}
