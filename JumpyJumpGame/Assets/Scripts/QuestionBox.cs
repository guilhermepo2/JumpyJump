using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionBox : MonoBehaviour, ICollideWithPlayer {
    public Sprite collidedSprite;
    public AudioClip coinSound;

    private SoundManager m_soundManager;
    private SpriteRenderer m_spriteRenderer;

    void Start() {
        m_soundManager = FindObjectOfType<SoundManager>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void CollidedWithPlayer() {
        if(m_soundManager && coinSound) {
            m_soundManager.PlayEffect(coinSound);
        }

        if(collidedSprite) {
            m_spriteRenderer.sprite = collidedSprite;
        }
    }
}
