using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem
{
    // Save Scene
    public static void SaveScene(GameObject scene)
    {
        string path = "Assets/Saves/" + scene.name + ".saves";
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, scene);
        stream.Close();
    }

    // Load Scene
    public static GameObject LoadScene(string sceneName)
    {
        string path = "Assets/Saves/" + sceneName + ".saves";
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Open);

        if (File.Exists(path))
        {
            GameObject data = (GameObject)formatter.Deserialize(stream);
            stream.Close();
            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);

            stream.Close();
            return null;
        }
    }

    // Save Player
    public static void SavePlayer(PlayerSystem player)
    {
        string path = "Assets/Saves/player.saves";
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);

        GameData data = new GameData(player);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    // Load Player
    public static GameData LoadPlayer(PlayerSystem player)
    {
        string path = "Assets/Saves/player.saves";
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Open);

        GameData data;

        if (File.Exists(path))
        {
            if (new FileInfo(path).Length != 0)
            {
                data = formatter.Deserialize(stream) as GameData;
                stream.Close();
            }
            else
            {
                Debug.LogError("Save file is empty in " + path);
                data = new GameData(player);
                formatter.Serialize(stream, data);
                stream.Close();
            }

            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);

            data = new GameData(player);
            formatter.Serialize(stream, data);
            stream.Close();

            return data;
        }
    }
}
