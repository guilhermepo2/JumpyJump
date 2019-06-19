using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

    void Start() {
        PlayerController t_playerReference = FindObjectOfType<PlayerController>();
        t_playerReference.PlayerDeath += RestartScene;    
    }

    public void RestartScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
