using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxEffect : MonoBehaviour {
    [Range(-1, 1)]
    public float parallaxSmoothness = 1f;

    private Transform[] m_parallaxedElements;
    private float[] m_parallaxScales;
    private Transform m_cameraReference;
    private Vector3 m_cameraPreviousPosition;
    private Vector3 m_tempBackgroundTargetPosition;

    void Awake() {
        m_cameraReference = Camera.main.transform;
        GameObject[] tempBackgrounds = GameObject.FindGameObjectsWithTag("Parallax");
        m_parallaxedElements = new Transform[tempBackgrounds.Length];

        for(int i = 0; i < tempBackgrounds.Length; i++) {
            m_parallaxedElements[i] = tempBackgrounds[i].transform;
        }
    }

    void Start() {
        m_cameraPreviousPosition = m_cameraReference.position;
        m_parallaxScales = new float[m_parallaxedElements.Length];

        for(int i = 0; i < m_parallaxedElements.Length; i++) {
            m_parallaxScales[i] = m_parallaxedElements[i].transform.position.z * -1;
        }
    }

    void Update() {
        for(int i = 0; i < m_parallaxedElements.Length; i++) {
            float parallax = (m_cameraPreviousPosition.x - m_cameraReference.position.x) * m_parallaxScales[i];

            m_tempBackgroundTargetPosition.x = m_parallaxedElements[i].position.x + (parallax * parallaxSmoothness);
            m_tempBackgroundTargetPosition.y = m_parallaxedElements[i].position.y;
            m_tempBackgroundTargetPosition.z = m_parallaxedElements[i].position.z;

            m_parallaxedElements[i].position = m_tempBackgroundTargetPosition;
        }

        m_cameraPreviousPosition = m_cameraReference.position;
    }
}
