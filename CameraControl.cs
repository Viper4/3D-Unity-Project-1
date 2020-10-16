using System.Collections;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public bool lockCursor;
    public float mouseSensitivity = 10;
    public Transform target;
    public float dstFromTarget = 4;
    public Vector2 pitchMinMax = new Vector2(-30,85);

    public float rotationSmoothTime = 0.1f;
    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;

    public float perspective = 2;

    public float rotateSpeed;

    float distanceFromTarget;
    float yaw;
    float pitch;

    GameObject playerModel;
    GameObject healthBar;
    public Canvas firstPerson;

    private void Awake()
    {
        switch (transform.name)
        {
            case "PlayerCamera":
                GameData data = SaveSystem.LoadPlayer(GameObject.Find("Player").GetComponent<PlayerSystem>());
                perspective = data.perspective;
                mouseSensitivity = data.mouseSensitivity;

                if (data.fov != 0)
                {
                    GetComponent<Camera>().fieldOfView = data.fov;
                }

                playerModel = GameObject.Find("Model");
                healthBar = GameObject.Find("Bar");

                distanceFromTarget = dstFromTarget;
                break;
            case "StartCamera":
                data = SaveSystem.LoadPlayer(GetComponent<PlayerSystem>());
                transform.GetComponent<Camera>().fieldOfView = data.fov;

                if (data.fov != 0)
                {
                    GetComponent<Camera>().fieldOfView = data.fov;
                }
                break;
        }
        
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        switch (transform.name)
        {
            case "PlayerCamera":
                switch (perspective)
                {
                    case 0:
                        distanceFromTarget = 0;
                        target = GameObject.Find("PlayerCameraAnchor").GetComponent<Transform>();

                        playerModel.SetActive(false);
                        healthBar.SetActive(false);
                        firstPerson.gameObject.SetActive(true);

                        break;
                    case 1:
                        distanceFromTarget = 0;
                        target = GameObject.Find("CameraAnchor").GetComponent<Transform>();
                        if (GameObject.Find("CameraAnchor") == null)
                        {
                            perspective = 0;
                        }

                        playerModel.SetActive(true);
                        healthBar.SetActive(true);
                        firstPerson.gameObject.SetActive(false);

                        break;
                    case 2:
                        distanceFromTarget = dstFromTarget;
                        target = GameObject.Find("PlayerCameraAnchor").GetComponent<Transform>();

                        playerModel.SetActive(true);
                        healthBar.SetActive(true);
                        firstPerson.gameObject.SetActive(false);

                        break;
                }

                yaw += Input.GetAxis("Mouse X") * mouseSensitivity / 8;
                pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity / 8;
                pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

                currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
                transform.eulerAngles = currentRotation;

                transform.position = target.position - transform.forward * distanceFromTarget;
                break;
            case "StartCamera":
                transform.Rotate(0, 25 * Time.deltaTime, 0);
                break;
        }
    }
}
