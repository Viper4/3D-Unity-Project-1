using UnityEngine;
using UnityEngine.UI;

public class Displays : MonoBehaviour
{
    public Transform redBar;
    public Text healthText;
    public Text killsText;
    public Text pointsText;
    public Text scoreText;

    // Update is called once per frame
    void Update()
    {
        float health = GetComponentInParent<CollisionSystem>().health;
        float maxHealth = GetComponentInParent<CollisionSystem>().maxHealth;

        transform.rotation = Camera.main.transform.rotation;
        redBar.localScale = new Vector3(health / maxHealth, 1, 1);

        if (transform.parent.name == "Player")
        {
            PlayerSystem playerSystem = GameObject.Find("Player").GetComponent<PlayerSystem>();

            healthText.text = "Health: " + health + " / " + maxHealth;
            killsText.text = "Kills: " + playerSystem.kills;
            pointsText.text = "Points: " + playerSystem.points;
            scoreText.text = "High Score: " + playerSystem.highScore + "\nScore: " + playerSystem.points;
        }
    }
}
