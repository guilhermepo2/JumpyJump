using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleStateMachine : MonoBehaviour {

    // using enum to name our states
    public enum EPlayerState {
        Grounded,
        Jumping,
        OnWall,
        JumpingFromWall,
    }

    private EPlayerState m_currentPlayerState;

    // random variables to keep the code readable lol
    private bool playerJump;
    private bool isGrounded;
    private bool collidingWithWalls;

    void Start() {
        m_currentPlayerState = EPlayerState.Grounded;
    }

    void Update() {

        // common code for all states goes here...

        // handling the current state
        switch(m_currentPlayerState) {
            case EPlayerState.Grounded:
                GroundedState();
                break;
            case EPlayerState.Jumping:
                JumpingState();
                break;
            case EPlayerState.OnWall:
                OnWallState();
                break;
            case EPlayerState.JumpingFromWall:
                JumpingFromWallState();
                break;
        }

        // mess with x velocity
        // do stuff with y velocity
        // player.Move();
    }

    // Functions for each state
    // each state handles its own specific things and transitioning to other states
    private void GroundedState() {
        // grounded specifics...

        if(playerJump) {
            m_currentPlayerState = EPlayerState.Jumping;
        }
    }

    private void JumpingState() {
        // jumping specifics...

        if(isGrounded) {
            m_currentPlayerState = EPlayerState.Grounded;
        } else if(collidingWithWalls && playerJump) {
            m_currentPlayerState = EPlayerState.JumpingFromWall;
        } else if(collidingWithWalls) {
            m_currentPlayerState = EPlayerState.OnWall;
        }
    }

    private void OnWallState() {
        // on wall specifics...

        if(playerJump) {
            m_currentPlayerState = EPlayerState.JumpingFromWall;
        } else if(isGrounded) {
            m_currentPlayerState = EPlayerState.Grounded;
        }
    }

    private void JumpingFromWallState() {
        // jumping from wall specifics... 

        if(isGrounded) {
            m_currentPlayerState = EPlayerState.Grounded;
        }
    }
}
