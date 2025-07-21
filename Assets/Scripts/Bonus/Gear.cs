using UnityEngine;

public class Gear : MonoBehaviour
{
    [SerializeField] private float floatAmplitude = 0.25f;
    [SerializeField] private float floatFrequency = 1f;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        FloatMotion();
    }

    private void FloatMotion()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

}
