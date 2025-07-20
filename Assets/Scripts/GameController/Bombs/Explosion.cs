using UnityEngine;

public class Explosion : MonoBehaviour {
    void Start() {
        Destroy(gameObject, 0.5f);
    }

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Enemy")) Destroy(other.gameObject);
    }
}
