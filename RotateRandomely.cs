using UnityEngine;

public class RotateRandomely : MonoBehaviour
{
    public int minRotateSpeed = 1;
    public int maxRotateSpeed = 60;

    private int rotX;
    private int rotY;
    private int rotZ;

    void Awake()
    {
        rotX = Random.Range(minRotateSpeed, maxRotateSpeed);
        rotY = Random.Range(minRotateSpeed, maxRotateSpeed);
        rotZ = Random.Range(minRotateSpeed, maxRotateSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotX * Time.deltaTime, rotY * Time.deltaTime, rotZ * Time.deltaTime);
    }
}
