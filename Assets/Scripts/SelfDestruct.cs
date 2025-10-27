using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    [SerializeField] private float delay = 5f;
    void Start() {
        StartCoroutine(DelayedDestroy());
    }
    private IEnumerator DelayedDestroy() {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
