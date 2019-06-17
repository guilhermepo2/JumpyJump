using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfter : MonoBehaviour {
    public float timeToDestroy = 0.5f;

    void Start() {
        StartCoroutine(WaitUntilDestroy(timeToDestroy));
    }

    private IEnumerator WaitUntilDestroy(float time) {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
