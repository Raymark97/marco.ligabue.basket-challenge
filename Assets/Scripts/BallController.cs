using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    [SerializeField] private float delay = 5f;

    public bool bankShot;
    public bool perfectShot;
    void Start() {
        StartCoroutine(DelayedDestroy());
    }
    private IEnumerator DelayedDestroy() {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
