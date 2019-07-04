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
        m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void CollidedWithPlayer() {
        if(m_isActivated) {
            return;
        }

        JuiceBox();

        m_isActivated = true;
    }

    private void JuiceBox() {
        if (m_soundManager && coinSound) {
            m_soundManager.PlayEffect(coinSound);
        }

        if (collidedSprite) {
            m_spriteRenderer.sprite = collidedSprite;
        }

        StartCoroutine(GoUpAndDownRoutine());
    }

    private IEnumerator GoUpAndDownRoutine() {
        Transform spriteTransform = m_spriteRenderer.transform;
        float timeToGoUp = 0.1f;
        float amountToGoUp = 0.5f;
        Vector3 originalPosition = spriteTransform.position;
        Vector3 destination = originalPosition;
        destination.y += amountToGoUp;

        // go up
        for (float i = 0; i < timeToGoUp; i += Time.deltaTime) {
            float t = (i / timeToGoUp);
            spriteTransform.position = Vector3.Lerp(originalPosition, destination, t);
            yield return null;
        }

        // go down
        for(float i = 0; i < timeToGoUp; i += Time.deltaTime) {
            float t = (i / timeToGoUp);
            spriteTransform.position = Vector3.Lerp(destination, originalPosition, t);
            yield return null;
        }

        spriteTransform.position = originalPosition;
    }
}
