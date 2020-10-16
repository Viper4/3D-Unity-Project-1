using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuskAI : MonoBehaviour
{
    public float walkSpeed = 4f;
    public float speedSmoothTime = 0.5f;
    public float gravity = -12f;
    public float turnSpeed = 7f;
    [Range(0, 1)]
    public float airControlPercent;
    public float viewRadius = 10f;
    [Range(0,360)]
    public float viewAngle = 90f;
    public float hearRadius = 1.5f;

    float speedSmoothVelocity;
    float currentSpeed;
    float velocityY;
    float walking = 1;

    float dstToClosestTarget;
    float angleToClosestTarget;
    Target target;

    [HideInInspector]
    public bool isWandering = false;

    Animator animator;
    CharacterController controller;

    private CollisionSystem collisionSystem;

    // Start is called before the first frame update
    void Awake()
    {
        collisionSystem = GetComponent<CollisionSystem>();
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        if (transform.parent)
        {
            int difficulty = transform.parent.GetComponentInParent<PrefabGenerator>().difficulty;

            switch (difficulty)
            {
                case 0: // Easy
                    walkSpeed /= 1.4f;
                    viewRadius /= 1.4f;
                    collisionSystem.health /= 1.4f;
                    collisionSystem.maxHealth /= 1.4f;
                    collisionSystem.attackAmount = Mathf.RoundToInt(collisionSystem.attackAmount / 1.4f);
                    collisionSystem.healAmount = Mathf.RoundToInt(collisionSystem.healAmount / 1.4f);
                    collisionSystem.attackDelay /= 1.4f;
                    collisionSystem.healDelay /= 1.4f;
                    break;
                case 1: // Normal
                    // Keep values
                    break;
                case 2: // Hard
                    walkSpeed *= 1.4f;
                    viewRadius *= 1.4f;
                    hearRadius *= 1.1f;
                    collisionSystem.health *= 1.4f;
                    collisionSystem.maxHealth *= 1.4f;
                    collisionSystem.attackAmount = Mathf.RoundToInt(collisionSystem.attackAmount * 1.4f);
                    collisionSystem.healAmount = Mathf.RoundToInt(collisionSystem.healAmount * 1.4f);
                    collisionSystem.attackDelay *= 1.4f;
                    collisionSystem.healDelay *= 1.4f;
                    break;
                case 3: // Extreme
                    walkSpeed *= 2f;
                    viewRadius *= 2f;
                    hearRadius *= 1.5f;
                    collisionSystem.health *= 2f;
                    collisionSystem.maxHealth *= 2f;
                    collisionSystem.attackAmount *= 2;
                    collisionSystem.healAmount *= 2;
                    collisionSystem.attackDelay *= 2f;
                    collisionSystem.healDelay *= 2f;
                    break;
            }
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Time.timeScale != 0)
        {
            // Optimization not needed since Husk doesn't have too many features
            AI();

            Move();

            // Changes animation speed based off running or walking
            float animationSpeedPercent = currentSpeed / walkSpeed;
            animator.SetFloat("speedPercent", animationSpeedPercent, speedSmoothTime, Time.deltaTime);
        } 
    }

    void AI()
    {
        dstToClosestTarget = Mathf.Infinity;
        angleToClosestTarget = viewAngle;
        target = null;
        Target[] allTargets = FindObjectsOfType<Target>();
        foreach (Target currentTarget in allTargets)
        {
            float dstToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (dstToTarget < dstToClosestTarget & currentTarget.name == "Campfire")
            {
                dstToClosestTarget = dstToTarget;
                target = currentTarget;

            }
        }
        switch (transform.tag)
        {
            case "Hostile":
                SetPriority(allTargets, "Friendly");
                break;
            case "Friendly":
                SetPriority(allTargets, "Hostile");
                break;
        }
        if (target != null)
        {
            switch (target.name)
            {
                case "Campfire":
                    // If target is active Campfire and within viewRadius * 4.5, farther than viewRadius - 3, then MoveTowards target
                    if (dstToClosestTarget > viewRadius - 2 & target.gameObject.activeSelf)
                    {
                        MoveTowards(target);
                    }
                    else
                    {
                        StartWandering();
                    }
                    break;
                case "Player":
                    // If target is Player and is within viewRadius and within viewAngle * 0.5, MoveTowards target. Or if player is within hearRadius then movetowards target
                    if (dstToClosestTarget < viewRadius & angleToClosestTarget < viewAngle * 0.5f || dstToClosestTarget < hearRadius)
                    {
                        MoveTowards(target);
                    }
                    else
                    {
                        StartWandering();
                    }
                    break;
            }
        }
        else
        {
            StartWandering();
        }
    }

    void SetPriority(Target[] allTargets, string tag)
    {
        foreach (Target currentPriority in allTargets)
        {
            if (currentPriority.CompareTag(tag))
            {
                float dstToPriority = Vector3.Distance(transform.position, currentPriority.transform.position);
                Vector3 direction = currentPriority.transform.position - transform.position;
                float angleToPriority = Vector3.Angle(direction, transform.forward);
                if (dstToPriority < viewRadius & angleToPriority < viewAngle * 0.5f || dstToPriority < hearRadius)
                {
                    dstToClosestTarget = dstToPriority;
                    angleToClosestTarget = angleToPriority;
                    target = currentPriority;
                }
            }            
        }
    }

    void MoveTowards(Target target)
    {
        Vector3 direction = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z) - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, turnSpeed * Time.deltaTime);

        walking = 1;
    }

    void StartWandering()
    {
        if (!isWandering)
        {
            StartCoroutine(Wander());
        }
    }

    IEnumerator Wander()
    {
        int wanderDelay = Random.Range(12, 24);
        int walkTime = Random.Range(1, 4);
        int rotateTime = Random.Range(1, 20);

        isWandering = true;

        Vector3 direction = new Vector3(Random.rotation.x, 0, Random.rotation.z);
        Quaternion rotation = Quaternion.LookRotation(direction);

        while (rotateTime > 1)
        {
            rotateTime -= 1;
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, turnSpeed * Time.deltaTime);
            yield return null;

        }

        walking = 1;
        yield return new WaitForSeconds(walkTime);
        walking = 0;
        yield return new WaitForSeconds(wanderDelay);
        isWandering = false;
    }

    void Move()
    {
        currentSpeed = Mathf.SmoothDamp(currentSpeed, walkSpeed * walking, ref speedSmoothVelocity, GetModifiedSmoothTime(speedSmoothTime));

        velocityY += Time.deltaTime * gravity;

        Vector3 velocity = transform.forward * currentSpeed + Vector3.up * velocityY;

        controller.Move(velocity * Time.deltaTime);
        currentSpeed = new Vector2(controller.velocity.x, controller.velocity.z).magnitude;

        if (controller.isGrounded)
        {
            velocityY = 0;
        }
    }

    float GetModifiedSmoothTime(float smoothTime)
    {
        if (controller.isGrounded)
        {
            return smoothTime;
        }

        if (airControlPercent == 0)
        {
            return float.MaxValue;
        }
        return smoothTime / airControlPercent;
    }
}
