using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public float rotateSpeed = 4f;
    
    int rotateDelay = 20;
    float inputTimer = 0;

    Quaternion defaultRotation;

    void Start()
    {
        defaultRotation = transform.rotation;
    }

    public IEnumerator Open(Quaternion rotation)
    {
        if (transform.rotation != defaultRotation)
        {
            rotation = defaultRotation;
        }
        if (inputTimer <= 0)
        {
            inputTimer = rotateDelay;
        }
        while (inputTimer > 0)
        {
            inputTimer -= 1;
            if (inputTimer <= 1)
            {
                transform.rotation = rotation;
            }
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotateSpeed * Time.deltaTime);
            yield return null;
        }
        yield return new WaitForSeconds(rotateDelay / rotateSpeed);
        if (inputTimer > 0)
        {
            StartCoroutine(Open(rotation));
        }
    }
}
