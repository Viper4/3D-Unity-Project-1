using System.Collections.Generic;
using UnityEngine;

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
    public List<string> playerKeys = new List<string>();
    public List<KeyCode> playerValues = new List<KeyCode>();
    public GameData(PlayerSystem player)
    {
        difficulty = player.difficulty;
        perspective = player.perspective;
        mouseSensitivity = player.mouseSensitivity;
        fov = player.fov;
        kills = player.kills;
        points = player.points;
        highScore = player.highScore;
        playerKeys = new List<string>(player.keys.Keys);
        playerValues = new List<KeyCode>(player.keys.Values);
    }
}
