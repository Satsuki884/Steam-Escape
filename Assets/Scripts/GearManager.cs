using UnityEngine;

public class GearManager : MonoBehaviour {
    public static int gearsCollected = 0;

    public void AddGear() {
        gearsCollected++;
    }
}