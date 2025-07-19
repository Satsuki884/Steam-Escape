using UnityEngine;

public class Gear : MonoBehaviour {
    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            GearManager.gearsCollected++;
            Destroy(gameObject);
        }
    }
}