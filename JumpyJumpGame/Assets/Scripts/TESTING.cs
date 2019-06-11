using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TESTING : MonoBehaviour {
    Vector3 m_originalPosition;

    void Start() {
        m_originalPosition = transform.position;
    }

    void Update() {
        transform.position = m_originalPosition + new Vector3(Mathf.Sin(Time.time), Mathf.Cos(Time.time), 0f);
    }
}
