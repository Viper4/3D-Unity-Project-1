using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public GameObject loadingScreen;
    public Animator transition;
    public float transitionTime;
    public Slider slider;
    public Text progressText;

    public void LoadScene(string sceneName)
    {
        if (Time.timeScale != 1)
        {
            Time.timeScale = 1;
        }
        StartCoroutine(LoadAsynchronously(sceneName));
    }

    IEnumerator LoadAsynchronously(string sceneName)
    {
        slider.gameObject.SetActive(false);

        transition.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        slider.gameObject.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            slider.value = progress;
            progressText.text = Mathf.Round(progress * 100f) + "%";
            Debug.Log(sceneName + ": " + progress);

            yield return null;
        }
    }

    public void LoadFromSave(string save)
    {
        Destroy(GameObject.FindWithTag("Master"));
        Instantiate(SaveSystem.LoadScene(save));
    }
}
