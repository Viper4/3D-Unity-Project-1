using System.Collections;
using UnityEngine;

public class HoodedFigureAI : MonoBehaviour
{
    public float walkSpeed = 1.4f;
    public float runSpeed = 4f;
    public float speedSmoothTime = 0.1f;
    public float gravity = -12f;
    public float turnSpeed = 6f;
    [Range(0, 1)]
    public float airControlPercent;
    public float viewRadius = 10;

    float speedSmoothVelocity;
    float currentSpeed;
    float velocityY;
    float walking = 1;
    float healthPercent;

    float dstToClosestTarget;
    Target target;

    bool running;

    private CollisionSystem collisionSystem;

    public enum CurrentState
    {
        Idle,
        Wandering,
        Seeking,
        Attacking,
        Moving
    }

    CurrentState currentState = CurrentState.Idle;

    Animator animator;
    CharacterController controller;

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
                    runSpeed /= 1.4f;
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
                    runSpeed *= 1.4f;
                    viewRadius *= 1.4f;
                    collisionSystem.health *= 1.4f;
                    collisionSystem.maxHealth *= 1.4f;
                    collisionSystem.attackAmount = Mathf.RoundToInt(collisionSystem.attackAmount * 1.4f);
                    collisionSystem.healAmount = Mathf.RoundToInt(collisionSystem.healAmount * 1.4f);
                    collisionSystem.attackDelay *= 1.4f;
                    collisionSystem.healDelay *= 1.4f;
                    break;
                case 3: // Extreme
                    walkSpeed *= 2f;
                    runSpeed *= 2f;
                    viewRadius *= 2f;
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
            healthPercent = collisionSystem.GetHealthPercent();

            AI();

            Move();

            // Changes animation speed based off running or walking
            float animationSpeedPercent = ((running) ? currentSpeed / runSpeed : currentSpeed / walkSpeed * .5f);
            animator.SetFloat("speedPercent", animationSpeedPercent, speedSmoothTime, Time.deltaTime);
        }
    }

    void AI()
    {
        dstToClosestTarget = viewRadius;
        target = null;
        Target[] allTargets = FindObjectsOfType<Target>();
        foreach (Target currentTarget in allTargets)
        {
            float dstToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (currentTarget.CompareTag("Healer"))
            {
                if (dstToTarget < dstToClosestTarget & healthPercent < 1)
                {
                    dstToClosestTarget = dstToTarget;
                    target = currentTarget;
                }
            }
            else
            {
                if (currentTarget.transform != transform & dstToTarget < dstToClosestTarget)
                {
                    dstToClosestTarget = dstToTarget;
                    target = currentTarget;
                }
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
            switch (target.tag)
            {
                case "Healer":
                    if (healthPercent < 1)
                    {
                        currentState = CurrentState.Moving;
                    }
                    else
                    {
                        if (currentState == CurrentState.Moving)
                        {
                            currentState = CurrentState.Idle;
                        }
                    }
                    break;
                case "Hostile":
                    switch (transform.tag)
                    {
                        case "Hostile":
                            FollowTarget(dstToClosestTarget);
                            break;
                        case "Friendly":
                            currentState = CurrentState.Attacking;
                            break;
                    }
                    break;
                case "Friendly":
                    switch (transform.tag)
                    {
                        case "Hostile":
                            currentState = CurrentState.Attacking;
                            break;
                        case "Friendly":
                            FollowTarget(dstToClosestTarget);
                            break;
                    }
                    break;
            }
            switch (target.name)
            {
                case "Campfire":
                    FollowTarget(dstToClosestTarget);

                    break;
            }
        }
        
        switch (currentState)
        {
            case CurrentState.Idle:
                // When currentState is Idle, stop running and start wandering
                running = false;

                StartCoroutine(Wander());
                break;
            case CurrentState.Attacking:
                // If currentState is Attacking and target isn't in viewRadius, Start seeking and running with random seekTime min = 2 and max = 6
                if (target == null)
                {
                    StartCoroutine(Seek(2, 6, true));
                }
                // Else if target is in viewRadius RotateTowards or away from target based on healthPercent > target's healthPercent and start running
                else
                {
                    if (healthPercent >= target.GetComponent<CollisionSystem>().GetHealthPercent())
                    {
                        RotateTowards(target, true);
                    }
                    else
                    {
                        RotateTowards(target, false);
                    }
                    running = true;
                }
                
                break;
            case CurrentState.Moving:
                // If currentState is Moving, RotateTowards target and run or walk based off healthPercent > 0.6
                RotateTowards(target, true);
                CheckHealth();

                break;
        }
    }

    void SetPriority(Target[] allTargets, string tag)
    {
        foreach (Target currentPriority in allTargets)
        {
            if (currentPriority.CompareTag(tag))
            {
                float dstToPriority = Vector3.Distance(transform.position, currentPriority.transform.position);

                if (dstToPriority < viewRadius)
                {
                    dstToClosestTarget = dstToPriority;
                    target = currentPriority;

                }
            }
        }
    }

    void FollowTarget(float dstToClosestTarget)
    {
        if (dstToClosestTarget > viewRadius - 2)
        {
            currentState = CurrentState.Moving;
        }
        else
        {
            if (currentState == CurrentState.Moving)
            {
                StartCoroutine(Seek(2, 4, false));
            }
        }
    }

    void RotateTowards(Target target, bool yes)
    {
        if (target != null)
        {
            if (yes)
            {
                Vector3 direction = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z) - transform.position;
                Quaternion rotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, turnSpeed * Time.deltaTime);
            }
            else
            {
                Vector3 direction = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z) - transform.position;
                Quaternion rotation = Quaternion.LookRotation(-direction);
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, turnSpeed * Time.deltaTime);
            }
        }
    }

    void CheckHealth()
    {
        switch (healthPercent > 0.6)
        {
            case true:
                walking = 1;
                running = false;

                break;
            case false:
                walking = 0;
                running = true;

                break;
        }
    }

    IEnumerator Seek(int min, int max, bool Run)
    {
        int seekTime = Random.Range(min, max);

        currentState = CurrentState.Seeking;

        if (Run)
        {
            running = true;
            walking = 0;
        }
        else
        {
            running = false;
            walking = 1;
        }
        yield return new WaitForSeconds(seekTime);
        currentState = CurrentState.Idle;
    }

    IEnumerator Wander()
    {
        int wanderDelay = Random.Range(1, 6);
        int walkTime = Random.Range(0, 8);
        int rotateTime = Random.Range(0, 25);

        currentState = CurrentState.Wandering;

        Vector3 direction = new Vector3(Random.rotation.x, 0, Random.rotation.z);
        Quaternion rotation = Quaternion.LookRotation(direction);
        while (rotateTime > 0)
        {
            rotateTime -= 1;
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, turnSpeed * Time.deltaTime);
            yield return null;

        }

        walking = 1;
        yield return new WaitForSeconds(walkTime);
        walking = 0;
        yield return new WaitForSeconds(wanderDelay);
        currentState = CurrentState.Idle;
    }

    void Move()
    {
        float targetSpeed = ((running) ? runSpeed : walkSpeed * walking);
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, GetModifiedSmoothTime(speedSmoothTime));

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
