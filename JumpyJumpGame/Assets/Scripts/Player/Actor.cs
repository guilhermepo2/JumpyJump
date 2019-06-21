#define DEBUG_CC2D_RAYS
using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class Actor : MonoBehaviour {

    [System.Serializable]
    protected struct ActorRaycastOrigins {
        public Vector3 topLeft;
        public Vector3 topRight;
        public Vector3 bottomRight;
        public Vector3 bottomLeft;
    }

    [System.Serializable]
    public class ActorCollisionState {
        public bool collisionAbove;
        public bool collisionRight;
        public bool collisionBelow;
        public bool collisionLeft;
        public bool becameGroundedThisFrame;
        public bool wasGroundedLastFrame;

        public bool HasCollision() {
            return (collisionAbove || collisionRight || collisionBelow || collisionLeft);
        }

        public void ResetCollision() {
            collisionAbove = collisionRight = collisionBelow = collisionLeft = becameGroundedThisFrame = false;
        }
    }

    public event Action<RaycastHit2D> OnControllerCollidedEvent;
    public event Action<Collider2D> OnTriggerEnterEvent;
    public event Action<Collider2D> OnTriggerStayEvent;
    public event Action<Collider2D> OnTriggerExitEvent;

    protected bool ignoreOneWayPlatformsThisFrame;
    private float m_skinWidth = 0.0625f;
    public float SkinWidth {
        get { return m_skinWidth; }
        set {
            m_skinWidth = value;
            RecalculateDistanceBetweenRays();
        }
    }

    public LayerMask platformMask = 0;
    public LayerMask triggerMask = 0;
    public LayerMask oneWayPlatformMask = 0;

    private const int totalHorizontalRays = 6;
    private const int totalVerticalRays = 4;

    private BoxCollider2D m_boxCollider;
    private Rigidbody2D m_rigidBody2D;
    protected ActorCollisionState collisionState = new ActorCollisionState();
    public ActorCollisionState CollisionState { get; }
    public bool isGrounded { get { return collisionState.collisionBelow; } }
    protected Vector3 m_velocity;
    private const float km_SkinWidthFloatFudgeFactor = 0.001f;

    protected ActorRaycastOrigins m_raycastOrigins;
    private RaycastHit2D m_raycastHit;
    List<RaycastHit2D> m_raycastHitsThisFrame = new List<RaycastHit2D>(2);

    private float m_verticalDistanceBetweenRays;
    private float m_horizontalDistanceBetweenRays;

    public void OnTriggerEnter2D(Collider2D collision) {
        OnTriggerEnterEvent?.Invoke(collision);
    }

    public void OnTriggerStay2D(Collider2D collision) {
        OnTriggerStayEvent?.Invoke(collision);
    }

    public void OnTriggerExit2D(Collider2D collision) {
        OnTriggerExitEvent?.Invoke(collision);
    }

    void Awake() {
        Application.targetFrameRate = 60;

        // add our one-way platforms to our normal platform mask so that we can land on them from above
        platformMask |= oneWayPlatformMask;

        // cache some components
        m_boxCollider = GetComponent<BoxCollider2D>();
        m_rigidBody2D = GetComponent<Rigidbody2D>();

        // here, we trigger our properties that have setters with bodies
        SkinWidth = m_skinWidth;

        // Ignore all layers that are not on Trigger Mask.
        // Everything else will be handled by the Actor.
        for (var i = 0; i < 32; i++) {
            if ((triggerMask.value & 1 << i) == 0) {
                Physics2D.IgnoreLayerCollision(gameObject.layer, i);
            }
        }
    }

    void OnValidate() {
        Rigidbody2D tempRigidbody = GetComponent<Rigidbody2D>();
        // As we are simulating the Actor via code, it has to be Kinematic.
        tempRigidbody.isKinematic = true;

        // We need these ones to ensure effective collision detection.
        tempRigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        tempRigidbody.sleepMode = RigidbodySleepMode2D.NeverSleep;
        tempRigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void RecalculateDistanceBetweenRays() {
        // Horizontal
        var colliderUseableHeight = m_boxCollider.size.y - (2f * m_skinWidth);
        m_verticalDistanceBetweenRays = colliderUseableHeight / (totalHorizontalRays - 1);

        // Vertical
        var colliderUseableWidth = m_boxCollider.size.x - (2f * m_skinWidth);
        m_horizontalDistanceBetweenRays = colliderUseableWidth / (totalVerticalRays - 1);
    }

    private void CalculateRaycastOrigins() {
        // Raycasts needs to be fired from bounds inset inset by the skin width.
        var modifiedBounds = m_boxCollider.bounds;
        modifiedBounds.Expand(-2f * m_skinWidth);

        m_raycastOrigins.topRight = modifiedBounds.max;
        m_raycastOrigins.topLeft = new Vector2(modifiedBounds.min.x, modifiedBounds.max.y);
        m_raycastOrigins.bottomRight = new Vector2(modifiedBounds.max.x, modifiedBounds.min.y);
        m_raycastOrigins.bottomLeft = modifiedBounds.min;
    }

    [System.Diagnostics.Conditional("DEBUG_CC2D_RAYS")]
    void DrawRay(Vector3 start, Vector3 dir, Color color) {
        Debug.DrawRay(start, dir, color);
    }

    public Vector3 Move(Vector3 deltaMovement) {
        collisionState.wasGroundedLastFrame = collisionState.collisionBelow;
        collisionState.ResetCollision();
        m_raycastHitsThisFrame.Clear();
        CalculateRaycastOrigins();

        if (deltaMovement.x != 0f) {
            MoveHorizontal(ref deltaMovement);
        }

        if (deltaMovement.y != 0f) {
            MoveVertical(ref deltaMovement);
        }

        deltaMovement.z = 0;
        transform.Translate(deltaMovement, Space.World);

        if (Time.deltaTime > 0f) {
            m_velocity = deltaMovement / Time.deltaTime;
        }

        if (!collisionState.wasGroundedLastFrame && collisionState.collisionBelow) {
            collisionState.becameGroundedThisFrame = true;
        }

        if (OnControllerCollidedEvent != null) {
            for (int i = 0; i < m_raycastHitsThisFrame.Count; i++) {
                OnControllerCollidedEvent(m_raycastHitsThisFrame[i]);
            }
        }

        ignoreOneWayPlatformsThisFrame = false;

        return m_velocity;
    }

    void MoveHorizontal(ref Vector3 deltaMovement) {
        bool isGoingRight = deltaMovement.x > 0;
        float rayDistance = Mathf.Abs(deltaMovement.x) + m_skinWidth;
        Vector2 rayDirection = isGoingRight ? Vector2.right : Vector2.left;
        Vector2 initialRayOrigin = isGoingRight ? m_raycastOrigins.bottomRight : m_raycastOrigins.bottomLeft;

        for (int i = 0; i < totalHorizontalRays; i++) {
            Vector2 ray = new Vector2(initialRayOrigin.x, initialRayOrigin.y + i * m_verticalDistanceBetweenRays);
            DrawRay(ray, rayDirection * rayDistance, Color.red);

            // if we are grounded we will include oneWayPlatforms only on the first ray (the bottom one)
            if (i == 0 && collisionState.wasGroundedLastFrame) {
                m_raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, platformMask);
            } else {
                m_raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, platformMask & ~oneWayPlatformMask);
            }

            if (m_raycastHit) {
                deltaMovement.x = m_raycastHit.point.x - ray.x;
                rayDistance = Mathf.Abs(deltaMovement.x);

                if (isGoingRight) {
                    deltaMovement.x -= m_skinWidth;
                    collisionState.collisionRight = true;
                } else {
                    deltaMovement.x += m_skinWidth;
                    collisionState.collisionLeft = true;
                }

                // If the MoveHorizontal shall receive a collision funciton, it shall call it here.
                // But this kind of have the same usability
                m_raycastHitsThisFrame.Add(m_raycastHit);

                // Breaking if we have an impact already.
                if (rayDistance < m_skinWidth + km_SkinWidthFloatFudgeFactor) {
                    break;
                }
            }
        }
    }

    void MoveVertical(ref Vector3 deltaMovement) {
        bool isGoingUp = deltaMovement.y > 0;
        float rayDistance = Mathf.Abs(deltaMovement.y) + m_skinWidth;
        Vector2 rayDirection = isGoingUp ? Vector2.up : Vector2.down;
        Vector2 initialRayOrigin = isGoingUp ? m_raycastOrigins.topLeft : m_raycastOrigins.bottomLeft;

        // apply our horizontal deltaMovement here so that we do our raycast from the actual position we would be in if we had moved
        initialRayOrigin.x += deltaMovement.x;

        // if we are moving up, we should ALWAYS ignore the layers in oneWayPlatformMask
        var mask = platformMask;
        if (isGoingUp && !collisionState.wasGroundedLastFrame) {
            mask &= ~oneWayPlatformMask;
        }

        for (var i = 0; i < totalVerticalRays; i++) {
            var ray = new Vector2(initialRayOrigin.x + i * m_horizontalDistanceBetweenRays, initialRayOrigin.y);

            DrawRay(ray, rayDirection * rayDistance, Color.red);
            m_raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, mask);

            if (m_raycastHit) {
                deltaMovement.y = m_raycastHit.point.y - ray.y;
                rayDistance = Mathf.Abs(deltaMovement.y);

                // remember to remove the skinWidth from our deltaMovement
                if (isGoingUp) {
                    deltaMovement.y -= m_skinWidth;
                    collisionState.collisionAbove = true;
                } else {
                    deltaMovement.y += m_skinWidth;
                    collisionState.collisionBelow = true;
                }

                m_raycastHitsThisFrame.Add(m_raycastHit);

                // Direct Impact
                if (rayDistance < m_skinWidth + km_SkinWidthFloatFudgeFactor) break;
            }
        }
    }

    protected bool IsNear(Vector2 initialPosition, Vector2 checkDirection, float distanceToCheck) {
        for (int i = 0; i < totalHorizontalRays; i++) {
            // need to subtract from the initialPosition.y because we are receiving the TOP left or right
            Vector2 ray = new Vector2(initialPosition.x, initialPosition.y - i * m_verticalDistanceBetweenRays);

            DrawRay(ray, checkDirection, Color.red);

            RaycastHit2D raycastHit = Physics2D.Raycast(ray, checkDirection, distanceToCheck, platformMask);

            if (raycastHit) return true;
        }

        return false;
    }
}