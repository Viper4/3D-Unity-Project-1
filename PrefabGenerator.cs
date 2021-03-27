using UnityEngine;

public class PrefabGenerator : MonoBehaviour
{
    public PrefabTypes[] prefabTypes;

    float minAmountMultiplier = 1;
    float maxAmountMultiplier = 1;
    float chanceMultiplier = 1;

    public Transform viewer;

    Vector2 viewerPosition;
    Vector2 viewerPositionOld;

    [HideInInspector]
    public int difficulty;

    void Awake()
    {
        GameData data = SaveSystem.LoadData(GameObject.Find("Player").GetComponent<PlayerSystem>());
        difficulty = data.difficulty;

        if (viewer == null)
        {
            try
            {
                viewer = GameObject.Find("Player").transform;
                Debug.LogWarning("Player Transform used to replace null viewer Transform");
            }
            catch
            {
                Debug.LogError("No Player transform found to replace null viewer Transform");
            }
        }
    }
    
    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        if (viewerPosition != viewerPositionOld)
        {
            if (prefabTypes.Length != 0)
            {
                foreach (GameObject validSpawn in GameObject.FindGameObjectsWithTag("ValidSpawn"))
                {
                    for (int i = 0; i < prefabTypes.Length; i++) // For each prefabType
                    {
                        int rollAmount = Random.Range(prefabTypes[i].minRoll, prefabTypes[i].maxRoll);
                        for (int j = 0; j < rollAmount; j++)
                        {
                            for (int k = 0; k < prefabTypes[i].prefabList.Length; k++) // For each prefabList in prefabTypes
                            {
                                DifficultyScaling(prefabTypes, i);
                                
                                if (Random.value + 0.0001 <= prefabTypes[i].prefabList[k].chance * chanceMultiplier)
                                {
                                    int randomAmount = Random.Range(Mathf.RoundToInt(prefabTypes[i].prefabList[k].minAmount * minAmountMultiplier), Mathf.RoundToInt(prefabTypes[i].prefabList[k].maxAmount * maxAmountMultiplier));
                                    for (int l = 0; l <= randomAmount; l++)
                                    {
                                        Renderer renderer = validSpawn.GetComponent<Renderer>();
                                        
                                        float randomX = Random.Range(renderer.bounds.min.x, renderer.bounds.max.x);
                                        float randomZ = Random.Range(renderer.bounds.min.z, renderer.bounds.max.z);
                                        float randomScale = Random.Range(prefabTypes[i].prefabList[k].minScale, prefabTypes[i].prefabList[k].maxScale);
                                        Physics.Raycast(new Vector3(randomX, renderer.bounds.max.y - 5f, randomZ), Vector3.down, out RaycastHit hit);
                                        if (Physics.Raycast(new Vector3(randomX, renderer.bounds.max.y - 5f, randomZ), Vector3.down, out hit))
                                        {
                                            GameObject newPrefab = Instantiate(prefabTypes[i].prefabList[k].prefab, hit.point, Quaternion.Euler(0, Random.Range(prefabTypes[i].prefabList[k].minRotationY, prefabTypes[i].prefabList[k].maxRotationY), 0), transform.GetChild(i));

                                            newPrefab.tag = prefabTypes[i].type;
                                            if (newPrefab.CompareTag("Healer"))
                                            {
                                                newPrefab.transform.localScale = new Vector3(randomScale, randomScale / 4f, randomScale);

                                            }
                                            else
                                            {
                                                newPrefab.transform.localScale = new Vector3(randomScale, randomScale, randomScale);

                                            }
                                            newPrefab.gameObject.SetActive(false);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    validSpawn.tag = "Untagged";
                }
            }

            viewerPositionOld = viewerPosition;
        }
    }

    void DifficultyScaling(PrefabTypes[] prefabTypes, int index) // Representing rational divisions as decimals, and repeating or irrational divisions as 1 / x
    {
        switch (difficulty)
        {
            case 0: // Easy
                switch (prefabTypes[index].type)
                {
                    case "Hostile":
                        chanceMultiplier = 1 / 1.5f;
                        minAmountMultiplier = 0.625f;
                        maxAmountMultiplier = 0.625f;
                        break;
                    case "Friendly":
                        chanceMultiplier = 1.5f;
                        minAmountMultiplier = 1.6f;
                        maxAmountMultiplier = 1.6f;
                        break;
                    case "Healer":
                        chanceMultiplier = 1.4f;
                        minAmountMultiplier = 1.4f;
                        maxAmountMultiplier = 1.4f;
                        break;
                    case "Structure":
                        chanceMultiplier = 1.4f;
                        minAmountMultiplier = 1.4f;
                        maxAmountMultiplier = 1.4f;
                        break;
                    case "Obstacle":
                        chanceMultiplier = 1 / 1.2f;
                        minAmountMultiplier = 1 / 1.4f;
                        maxAmountMultiplier = 1 / 1.4f;
                        break;
                }
                break;
            case 1: // Normal
                    // Keep current values
                break;
            case 2: // Hard
                switch (prefabTypes[index].type)
                {
                    case "Hostile":
                        chanceMultiplier = 1.5f;
                        minAmountMultiplier = 1.6f;
                        maxAmountMultiplier = 1.6f;
                        break;
                    case "Friendly":
                        chanceMultiplier = 1 / 1.5f;
                        minAmountMultiplier = 0.625f;
                        maxAmountMultiplier = 0.625f;
                        break;
                    case "Healer":
                        chanceMultiplier = 1 / 1.4f;
                        minAmountMultiplier = 1 / 1.4f;
                        maxAmountMultiplier = 1 / 1.4f;
                        break;
                    case "Structure":
                        chanceMultiplier = 1 / 1.4f;
                        minAmountMultiplier = 1 / 1.4f;
                        maxAmountMultiplier = 1 / 1.4f;
                        break;
                    case "Obstacle":
                        chanceMultiplier = 1.2f;
                        minAmountMultiplier = 1.4f;
                        maxAmountMultiplier = 1.4f;
                        break;
                }
                break;
            case 3: // Extreme
                switch (prefabTypes[index].type)
                {
                    case "Hostile":
                        chanceMultiplier = 1.5f;
                        minAmountMultiplier = 2f;
                        maxAmountMultiplier = 2f;
                        break;
                    case "Friendly":
                        chanceMultiplier = 1 / 1.5f;
                        minAmountMultiplier = 1 / 1.8f;
                        maxAmountMultiplier = 1 / 1.8f;
                        break;
                    case "Healer":
                        chanceMultiplier = 1 / 1.4f;
                        minAmountMultiplier = 0.625f;
                        maxAmountMultiplier = 0.625f;
                        break;
                    case "Structure":
                        chanceMultiplier = 1 / 1.4f;
                        minAmountMultiplier = 1 / 1.5f;
                        maxAmountMultiplier = 1 / 1.5f;
                        break;
                    case "Obstacle":
                        chanceMultiplier = 1.4f;
                        minAmountMultiplier = 1.6f;
                        maxAmountMultiplier = 1.6f;
                        break;
                }
                break;
        }
    }

    [System.Serializable]
    public struct PrefabTypes
    {
        public string type;

        public int minRoll;
        public int maxRoll;

        public PrefabInfo[] prefabList;
    }

    [System.Serializable]
    public struct PrefabInfo
    {
        public GameObject prefab;

        [Range(0, 1)]
        public float chance;

        public int minAmount;
        public int maxAmount;

        [Range(0, 360)]
        public float minRotationY;
        [Range(0, 360)]
        public float maxRotationY;
        public float minScale;
        public float maxScale;
    }
}
