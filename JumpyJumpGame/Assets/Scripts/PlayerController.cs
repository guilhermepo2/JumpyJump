using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float jumpPeakHeight = 2;
    public float horizontalDistanceToPeak = 2;
    public float footSpeed = 5;
    public float groundDamping = 1f;
    public float airDamping = 1f;

    // Handling Jump and Velocity
    private float m_jumpInitialVelocity;
    private float m_gravity;
    private float m_goingUpGravity;
    private float m_goingDownGravity;
    private Vector3 m_playerVelocity;
    private float inputHorizontalSpeed;

    // Input Buffering
    private const float m_pressedToJumpRememberTime = 0.125f;
    private const float m_playerGroundedRememberTime = 0.125f;
    private float m_pressedJumpTime;
    private float m_wasGroundedTime;

    // Animation Stuff
    private Actor m_actorReference;
    private Transform m_playerSpriteTransform;
    private Animator m_playerSpriteAnimator;
    private const string IDLE_ANIMATION = "Idle";
    private const string RUNNING_ANIMATION = "Running";
    private const string JUMPING_ANIMATION = "Jumping";

    // Player State
    public enum EPlayerState {
        Grounded,
        Jumping,
    }
    private EPlayerState m_currentPlayerState;

    void Awake() {
        m_actorReference = GetComponent<Actor>();
        m_playerSpriteTransform = GetComponentInChildren<SpriteRenderer>().transform;
        m_playerSpriteAnimator = GetComponentInChildren<Animator>();

        // v0 = (2 * jump_height) / (jump_time)
        // jump_time = distance to peak / foot speed
        m_jumpInitialVelocity = (2 * jumpPeakHeight * footSpeed) / (horizontalDistanceToPeak);

        // g = (-2 * jump_height) / (jump_time^2)
        m_goingUpGravity = -((2 * jumpPeakHeight * footSpeed * footSpeed) / (horizontalDistanceToPeak * horizontalDistanceToPeak));
        m_goingDownGravity = m_goingUpGravity * 3f;
        m_gravity = m_goingDownGravity;
        inputHorizontalSpeed = 0;
        m_currentPlayerState = EPlayerState.Grounded;
    }

    // Update is called once per frame
    void Update() {
        m_pressedJumpTime -= Time.deltaTime;
        m_wasGroundedTime -= Time.deltaTime;
        inputHorizontalSpeed = Input.GetAxisRaw("Horizontal");

        if(Input.GetButtonDown("Jump")) {
            m_pressedJumpTime = m_pressedToJumpRememberTime;
        }

        if(m_actorReference.isGrounded) {
            m_wasGroundedTime = m_playerGroundedRememberTime;
            m_playerVelocity.y = 0;
        }

        switch(m_currentPlayerState) {
            case EPlayerState.Grounded:
                ProcessGroundedState();
                break;
            case EPlayerState.Jumping:
                ProcessJumpingState();
                break;
        }

        ProcessSpriteScale();
        ProcessAnimation();

        if(m_playerVelocity.y < 0) {
            m_gravity = m_goingDownGravity;
            m_currentPlayerState = EPlayerState.Jumping;
        }

        float smoothedMovementFactor = m_actorReference.isGrounded ? groundDamping : airDamping;
        float xVelocityLerp = Mathf.Clamp01(Time.deltaTime * smoothedMovementFactor);
        m_playerVelocity.x = Mathf.Lerp(m_playerVelocity.x, inputHorizontalSpeed * footSpeed, xVelocityLerp);

        Vector3 eulerDeltaMovement = m_playerVelocity * Time.deltaTime;
        Vector3 velocityVerletDeltaMovement = new Vector3(eulerDeltaMovement.x, eulerDeltaMovement.y + (0.5f * m_gravity * Time.deltaTime * Time.deltaTime), 0f);
        m_playerVelocity = m_actorReference.Move(velocityVerletDeltaMovement);
    }

    private void ProcessGroundedState() {
        if(m_pressedJumpTime > 0 && m_wasGroundedTime > 0) {
            m_pressedJumpTime = 0;
            m_wasGroundedTime = 0;
            m_gravity = m_goingUpGravity;
            m_playerVelocity.y = m_jumpInitialVelocity;
            m_currentPlayerState = EPlayerState.Jumping;
        }
    }

    private void ProcessJumpingState() {
        if (m_actorReference.isGrounded) {
            m_currentPlayerState = EPlayerState.Grounded;
        }
    }

    private void ProcessSpriteScale() {
        if(inputHorizontalSpeed == 0) {
            return;
        }

        transform.localScale = new Vector3(Mathf.Sign(inputHorizontalSpeed) * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    private void ProcessAnimation() {
        if(m_playerSpriteAnimator == null) {
            return;
        }

        if(!m_actorReference.isGrounded) {
            m_playerSpriteAnimator.Play(JUMPING_ANIMATION);
        } else if (Mathf.Abs(m_playerVelocity.x) > 0.5f) {
            m_playerSpriteAnimator.Play(RUNNING_ANIMATION);
        } else {
            m_playerSpriteAnimator.Play(IDLE_ANIMATION);
        }
    }
}
