using UnityEngine;

public class PigeonAnimation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Camera.main.transform != null)
        {
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}
