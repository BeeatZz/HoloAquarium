using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform camTransform;

    void Start()
    {
        camTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (LevelManager.Instance != null && LevelManager.Instance.is3DPerspectiveMode)
        {
            transform.LookAt(transform.position + camTransform.forward);
        }
        else
        {
            transform.rotation = Quaternion.identity;
        }
    }
}