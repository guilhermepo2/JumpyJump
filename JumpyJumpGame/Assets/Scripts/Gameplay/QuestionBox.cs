using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionBox : MonoBehaviour, ICollideWithPlayer {
    public Sprite collidedSprite;
    public AudioClip coinSound;

    private SoundManager m_soundManager;
    private SpriteRenderer m_spriteRenderer;
    private bool m_isActivated;

    void Start() {
        m_isActivated = false;
        m_soundManager = FindObjectOfType<SoundManager>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void CollidedWithPlayer() {
        if(m_isActivated) {
            return;
        }

        if(m_soundManager && coinSound) {
            m_soundManager.PlayEffect(coinSound);
        }

        if(collidedSprite) {
            m_spriteRenderer.sprite = collidedSprite;
        }

        m_isActivated = true;
    }
}
