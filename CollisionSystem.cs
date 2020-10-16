using System.Collections.Generic;
using UnityEngine;
public class CollisionSystem : MonoBehaviour
{
    PlayerSystem playerSystem;

    public float attackDelay = 80;
    public float healDelay = 80;
    public int attackAmount = 10;
    public int healAmount = 10;
    public float maxHealth = 100;
    public float health = 100;

    float attackTimer;
    float healTimer;

    private Dictionary<string, KeyCode> keys = new Dictionary<string, KeyCode>();

    void Awake()
    {
        playerSystem = GameObject.Find("Player").GetComponent<PlayerSystem>();

    }

    void Update()
    {
        keys = playerSystem.keys;
    }

    public float GetHealthPercent()
    {
        return health / maxHealth;
    }

    // Remove health
    public void Attack(int attackAmount)
    {
        health -= attackAmount;

        if (health <= 0)
        {
            health = 0;

            if (transform.name == "Player")
            {
                transform.tag = "Dead";
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    // Add health
    public void Heal(int healAmount)
    {
        health += healAmount;
        
        if (health > maxHealth)
        {
            health = maxHealth;
        }
    }

    public void Timer()
    {
        while (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
            break;
        }
        while (healTimer > 0)
        {
            healTimer -= Time.deltaTime;
            break;
        }
    }

    public void OnTriggerStay(Collider collider)
    {
        if (collider.transform != transform & !collider.CompareTag("NonInteract") & !collider.CompareTag("Obstacle"))
        {
            Timer();

            if (transform.name == "Player")
            {
                PlayerOnCollide(collider);
            }
            else
            {
                if (attackTimer <= 0 || healTimer <= 0)
                {
                    switch (transform.tag)
                    {
                        case "Healer":
                            if (collider.name != "Player")
                            {
                                collider.GetComponent<CollisionSystem>().Heal(healAmount);
                            }
                            break;
                        case "Hostile":
                            if (collider.CompareTag("Friendly"))
                            {
                                collider.GetComponent<CollisionSystem>().Attack(healAmount);

                                switch (transform.name)
                                {
                                    case "Husk(Clone)":
                                        Heal(healAmount / 2);
                                        break;
                                }
                            }
                            break;
                        case "Friendly":
                            if (collider.CompareTag("Hostile"))
                            {
                                collider.GetComponent<CollisionSystem>().Attack(healAmount);
                            }
                            break;
                        case "Structure":
                            switch (transform.name)
                            {
                                case "Campfire":
                                    collider.GetComponent<CollisionSystem>().Attack(attackAmount);
                                    break;
                                case "Campfire1":
                                    if (transform.Find("Campfire").gameObject.activeSelf)
                                    {
                                        collider.GetComponent<CollisionSystem>().Heal(healAmount);
                                    }
                                    break;
                                case "House1(Clone)":
                                    collider.GetComponent<CollisionSystem>().Heal(healAmount);
                                    break;
                            }
                            break;
                    }
                }
            }
        }
        if (attackTimer <= 0)
        {
            attackTimer = attackDelay;
        }
        if (healTimer <= 0)
        {
            healTimer = healDelay;
        }
    }

    void PlayerOnCollide(Collider collider)
    {
        switch (transform.tag)
        {
            case "Friendly":
                switch (collider.tag)
                {
                    case "Hostile":
                        Popup(keys["Attack"].ToString() + " to attack");
                        if (Input.GetKeyDown(keys["Attack"]))
                        {
                            if (collider.GetComponent<CollisionSystem>().health - attackAmount <= 0)
                            {
                                playerSystem.kills++;
                                Destroy(collider);
                            }
                            collider.transform.GetComponent<CollisionSystem>().Attack(attackAmount);
                        }
                        break;
                }
                break;
            case "Hostile":
                switch (collider.tag)
                {
                    case "Friendly":
                        Popup(keys["Attack"].ToString() + " to attack");
                        if (Input.GetKeyDown(keys["Attack"]))
                        {
                            if (collider.GetComponent<CollisionSystem>().health - attackAmount <= 0)
                            {
                                playerSystem.kills++;
                                Destroy(collider);
                            }
                            collider.GetComponent<CollisionSystem>().Attack(attackAmount);                            
                        }
                        break;
                }
                break;
        }
        switch (collider.transform.tag)
        {
            case "Structure":
                switch (collider.name)
                {
                    case "Campfire1":
                        Transform campfire = collider.transform.Find("Campfire");
                        if (campfire.gameObject.activeSelf && playerSystem.kills != 0)
                        {
                            playerSystem.points = playerSystem.points + playerSystem.kills / 2;
                            playerSystem.kills = 0;
                        }
                        Popup(keys["Interact"].ToString() + " to interact");
                        if (Input.GetKeyDown(keys["Interact"]))
                        {
                            if (campfire.gameObject.activeSelf)
                            {
                                campfire.gameObject.SetActive(false);
                            }
                            else
                            {
                                campfire.gameObject.SetActive(true);
                            }
                        }
                        break;
                }
                break;
            case "Interactable":
                switch (collider.name)
                {
                    case "Door1":
                        Popup(keys["Interact"].ToString() + " to open/close");
                        if (Input.GetKeyDown(keys["Interact"]))
                        {
                            Vector3 direction = new Vector3(collider.transform.parent.position.x, transform.position.y, collider.transform.parent.position.z) - transform.position;
                            Quaternion rotation = Quaternion.LookRotation(-direction);

                            StartCoroutine(collider.transform.parent.GetComponent<Door>().Open(rotation));
                        }
                        break;
                }
                break;
            case "Healer":
                Popup(keys["Heal"].ToString() + " to heal");
                if (Input.GetKeyDown(keys["Heal"]))
                {
                    healAmount = collider.GetComponent<CollisionSystem>().healAmount;
                    Heal(healAmount);
                }
                break;
        }
    }

    void Popup(string text)
    {
        if (!playerSystem.popup.gameObject.activeSelf)
        {
            StartCoroutine(playerSystem.Popup(text));
        }
    }
}
