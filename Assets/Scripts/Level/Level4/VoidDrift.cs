using UnityEngine;

public class VoidDrift : MonoBehaviour
{
    public float speed = 0.5f;
    public float range = 1f;
    private Vector3 startPos;
    private float offset;

    void Start()
    {
        startPos = transform.position;
        offset = UnityEngine.Random.Range(0f, 100f);
    }

    void Update()
    {
        transform.position = startPos + new Vector3(0, Mathf.Sin(Time.time * speed + offset) * range, 0);
        transform.Rotate(new Vector3(10, 15, 5) * Time.deltaTime);
    }
}