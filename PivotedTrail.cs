using System.Collections;
using UnityEngine;

public class PivotedTrail : MonoBehaviour
{
    public Transform target;
    public float dstFromTarget = 4;

    public float rotationSmoothTime = 0.1f;
    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;

    float distanceFromTarget;

    public float movementDelay;
    float yaw;
    float pitch;
    private float pitchSpeed;
    private float yawSpeed;

    public TrailRenderer trail;

    private enum StartColorState { Red, Green, Blue }
    private enum EndColorState { Red, Green, Blue }

    StartColorState startColorState;
    EndColorState endColorState;

    void Awake()
    {
        trail = GetComponent<TrailRenderer>();
        int randomStart = Random.Range(0, 2);
        switch (randomStart)
        {
            case 0:
                trail.startColor = new Color(1, 0, 0, trail.startColor.a);
                break;
            case 1:
                trail.startColor = new Color(0, 1, 0, trail.startColor.a);
                break;
            case 2:
                trail.startColor = new Color(0, 0, 1, trail.startColor.a);
                break;
        }
        int randomEnd = Random.Range(0, 2);
        switch (randomEnd)
        {
            case 0:
                trail.endColor = new Color(1, 0, 0, trail.startColor.a);
                break;
            case 1:
                trail.endColor = new Color(0, 1, 0, trail.startColor.a);
                break;
            case 2:
                trail.endColor = new Color(0, 0, 1, trail.startColor.a);
                break;
        }

        distanceFromTarget = dstFromTarget;

        StartCoroutine(SetRandomRotation());
        StartCoroutine(ChangeTrailColor(0.1f));
    }

    // Update is called once per frame
    void Update()
    {
        pitch += pitchSpeed;
        yaw -= yawSpeed;

        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
        transform.eulerAngles = currentRotation;

        transform.position = target.position - transform.forward * distanceFromTarget;
    }

    IEnumerator SetRandomRotation()
    {
        pitchSpeed = Random.Range(-1.5f, 1.5f);
        yawSpeed = Random.Range(-1.5f, 1.5f);

        if (pitchSpeed == 0 && yawSpeed == 0)
        {
            StartCoroutine(SetRandomRotation());

            yield break;
        }

        yield return new WaitForSeconds(movementDelay);
        StartCoroutine(SetRandomRotation());
    }

    IEnumerator ChangeTrailColor(float delay)
    {
        if (trail.startColor == new Color(1, 0, 0, trail.startColor.a))
        {
            startColorState = StartColorState.Red;

        }
        if (trail.startColor == new Color(0, 1, 0, trail.startColor.a))
        {
            startColorState = StartColorState.Green;
        }
        if (trail.startColor == new Color(0, 0, 1, trail.startColor.a))
        {
            startColorState = StartColorState.Blue;
        }
        if (trail.endColor == new Color(1, 0, 0, trail.endColor.a))
        {
            endColorState = EndColorState.Red;
        }
        if (trail.endColor == new Color(0, 1, 0, trail.endColor.a))
        {
            endColorState = EndColorState.Green;
        }
        if (trail.endColor == new Color(0, 0, 1, trail.endColor.a))
        {
            endColorState = EndColorState.Blue;
        }

        switch (startColorState)
        {
            case StartColorState.Red:
                trail.startColor = new Color(trail.startColor.r - 0.01f, trail.startColor.g + 0.01f, trail.startColor.b, trail.startColor.a);

                break;
            case StartColorState.Green:
                trail.startColor = new Color(trail.startColor.r, trail.startColor.g - 0.01f, trail.startColor.b + 0.01f, trail.startColor.a);

                break;
            case StartColorState.Blue:
                trail.startColor = new Color(trail.startColor.r + 0.01f, trail.startColor.g, trail.startColor.b - 0.01f, trail.startColor.a);

                break;
        }
        switch (endColorState)
        {
            case EndColorState.Red:
                trail.endColor = new Color(trail.endColor.r - 0.01f, trail.endColor.g + 0.01f, trail.endColor.b, trail.endColor.a);

                break;
            case EndColorState.Green:
                trail.endColor = new Color(trail.endColor.r, trail.endColor.g - 0.01f, trail.endColor.b + 0.01f, trail.endColor.a);

                break;
            case EndColorState.Blue:
                trail.endColor = new Color(trail.endColor.r + 0.01f, trail.endColor.g, trail.endColor.b - 0.01f, trail.endColor.a);

                break;
        }
        yield return new WaitForSeconds(delay);
        StartCoroutine(ChangeTrailColor(delay));
    }
}
