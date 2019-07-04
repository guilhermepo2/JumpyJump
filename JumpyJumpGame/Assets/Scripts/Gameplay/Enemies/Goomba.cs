using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goomba : MonoBehaviour, IEnemy {
    public Vector2 startingDirection;
    public float footSpeed = 2.5f;

    private const float km_gravity = -10f;
    private Actor m_actorReference;
    private Vector2 m_currentDirection;
    private Vector3 m_goombaVelocity;

    void OnGoombaCollider(RaycastHit2D hit) {
        if(hit.normal.x == 1) {
            m_currentDirection = Vector2.right;
        } if(hit.normal.x == -1) {
            m_currentDirection = Vector2.left;
        }
    }

    void Awake() {
        m_actorReference = GetComponent<Actor>();
        m_actorReference.OnControllerCollidedEvent += OnGoombaCollider;
        m_currentDirection = startingDirection;
    }

    void Update() {
        if (m_actorReference.isGrounded) {
            m_goombaVelocity.y = 0;
        }

        m_goombaVelocity.x = m_currentDirection.x * footSpeed;
        m_goombaVelocity.y += km_gravity * Time.deltaTime;

        m_goombaVelocity = m_actorReference.Move(m_goombaVelocity * Time.deltaTime);
    }

    public void Kill() {
        Destroy(gameObject);
    }
}
