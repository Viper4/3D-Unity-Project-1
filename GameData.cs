using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class GameData
{
    public int difficulty;
    public int perspective;
    public float mouseSensitivity;
    public float fov;
    public float kills;
    public float points;
    public float highScore;

    public GameData(PlayerSystem player)
    {
        difficulty = player.difficulty;
        perspective = player.perspective;
        mouseSensitivity = player.mouseSensitivity;
        fov = player.fov;
        kills = player.kills;
        points = player.points;
        highScore = player.highScore;
    }
}
