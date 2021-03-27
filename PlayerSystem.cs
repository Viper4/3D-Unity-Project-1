using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerSystem : MonoBehaviour
{
    public GameObject sceneLoader;

    public float kills = 0;
    public float points = 0;
    public float highScore = 0;

    // Player controller
    public float walkSpeed = 2;
    public float runSpeed = 6;
    public float gravity = -12;
    public float jumpHeight = 1;
    [Range(0, 1)]
    public float airControlPercent;

    public float turnSmoothTime = 0.2f;
    float turnSmoothVelocity;

    public float speedSmoothTime = 0.1f;
    float speedSmoothVelocity;
    float currentSpeed;
    float velocityY;

    Animator animator;
    Transform cameraT;
    CharacterController controller;

    // UI
    public Canvas gameMenu, optionsMenu, controlsMenu, gameOver, popup;
    public Text up, left, down, right, run, jump, attack, heal, interact;
    GameObject currentKey;

    public Dropdown difficultyDropdown, perspectiveDropdown;
    public Slider FOVSlider, mouseSensitivitySlider, referenceSlider;

    public string UISelection;
    public bool keepConfiguration;

    public Dictionary<string, KeyCode> keys = new Dictionary<string, KeyCode>();

    //GameObject[] allSelectors;

    public int difficulty, perspective;
    public float mouseSensitivity, fov;

    void Awake()
    {
        // Player controller
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        cameraT = Camera.main.transform;

        LoadPlayer();

        // UI 
        if (keepConfiguration == false)
        {
            gameMenu = GameObject.Find("GameMenu").GetComponent<Canvas>();
            optionsMenu = GameObject.Find("Options").GetComponent<Canvas>();
            controlsMenu = GameObject.Find("Controls").GetComponent<Canvas>();
            gameOver = GameObject.Find("GameOver").GetComponent<Canvas>();
            popup = GameObject.Find("Popup").GetComponent<Canvas>();

            UISelection = "UI0";
        }

        currentKey = null;

        CheckUI();
    }

    void Update()
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "TitleScreen":
                // Exiting game
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    popup.gameObject.SetActive(true);
                    popup.transform.Find("ConfirmExit").gameObject.SetActive(true);
                    CheckUI();
                }

                break;
            case "Scene0":
                // Player Controller
                if (transform.CompareTag("Dead"))
                {
                    if (points > highScore)
                        highScore = points;

                    kills = 0;
                    points = 0;

                    SaveSystem.SaveData(this);
                }
                // Input
                Vector2 input = new Vector2(GetInputAxis("Horizontal"), GetInputAxis("Vertical"));
                Vector2 inputDir = input.normalized;
                bool running;
                if (perspective != 0)
                {
                    running = Input.GetKey(keys["Run"]);
                }
                else
                {
                    running = Input.GetKey(keys["Run"]) && Input.GetKey(keys["Up"]) && !Input.GetKey(keys["Down"]);
                }
                if (!transform.CompareTag("Dead"))
                {
                    Move(inputDir, running);
                    if (Input.GetKeyDown(keys["Jump"]))
                    {
                        Jump();
                    }
                }
                // Animator
                float animationSpeedPercent = running ? currentSpeed / runSpeed : currentSpeed / walkSpeed * .5f;
                animator.SetFloat("speedPercent", animationSpeedPercent, speedSmoothTime, Time.deltaTime);

                // UI
                // Pausing
                if (!transform.CompareTag("Dead") & Input.GetKeyDown(KeyCode.Escape))
                {
                    PauseGame();
                    CheckUI();
                }
                // Game over
                if (transform.CompareTag("Dead") & !gameOver.gameObject.activeSelf)
                {
                    Time.timeScale = 0;
                    gameOver.gameObject.SetActive(true);
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    CheckUI();
                }
                break;
        }
    }

    void LoadPlayer()
    {
        GameData data = SaveSystem.LoadData(this);

        difficulty = data.difficulty;
        perspective = data.perspective;
        mouseSensitivity = data.mouseSensitivity;
        fov = data.fov;
        kills = data.kills;
        points = data.points;
        highScore = data.highScore;

        keys.Clear();
        for (int i = 0; i < data.playerKeys.Count; i++) // Count of playerKeys should equal playerValues. If not, setup of the keys and values are incorrect
        {
            keys.Add(data.playerKeys[i], data.playerValues[i]);
        }
        if (keys.Count == 0)
        {
            keys.Add("Up", KeyCode.W);
            keys.Add("Left", KeyCode.A);
            keys.Add("Down", KeyCode.S);
            keys.Add("Right", KeyCode.D);
            keys.Add("Run", KeyCode.LeftShift);
            keys.Add("Jump", KeyCode.Space);
            keys.Add("Attack", KeyCode.Mouse0);
            keys.Add("Heal", KeyCode.Mouse1);
            keys.Add("Interact", KeyCode.E);

            SaveSystem.SaveData(this);
        }
        up.text = keys["Up"].ToString();
        left.text = keys["Left"].ToString();
        down.text = keys["Down"].ToString();
        right.text = keys["Right"].ToString();
        run.text = keys["Run"].ToString();
        jump.text = keys["Jump"].ToString();
        attack.text = keys["Attack"].ToString();
        heal.text = keys["Heal"].ToString();
        interact.text = keys["Interact"].ToString();
        
        difficultyDropdown.value = difficulty;
        perspectiveDropdown.value = perspective;
        mouseSensitivitySlider.value = mouseSensitivity;
        FOVSlider.value = fov;
    }

    /*void LoadScene(string sceneName)
    {
        try
        {
            sceneLoader.GetComponent<SceneLoader>().LoadScene("Scene0");
            sceneLoader.GetComponent<SceneLoader>().LoadFromSave(sceneName);
        }
        catch
        {
            Debug.LogError("Could not load " + sceneName);

            popup.gameObject.SetActive(true);
            popup.transform.Find("Ok").gameObject.SetActive(true);
            popup.transform.Find("Ok").GetComponent<Text>().text = "Error: Could not load " + sceneName + "!";
            CheckUI();
        }
    }*/

    private float GetInputAxis(string axis)
    {
        float horizontal = 0;
        float vertical = 0;

        switch (axis)
        {
            case "Horizontal":
                if (Input.GetKey(keys["Right"]))
                {
                    horizontal += 1;
                }
                if (Input.GetKey(keys["Left"]))
                {
                    horizontal -= 1;
                }
                return horizontal;
            case "Vertical":
                if (Input.GetKey(keys["Up"]))
                {
                    vertical += 1;
                }
                if (Input.GetKey(keys["Down"]))
                {
                    vertical -= 1;
                }
                return vertical;
        }
        return 0;
    }

    private void Move(Vector2 inputDir, bool running)
    {
        if (inputDir != Vector2.zero)
        {
            float targetRotation = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, getModifiedSmoothTime(turnSmoothTime));
        }

        float targetSpeed = running ? runSpeed : walkSpeed * inputDir.magnitude;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, getModifiedSmoothTime(speedSmoothTime));

        velocityY += Time.deltaTime * gravity;

        Vector3 velocity = transform.forward * currentSpeed + Vector3.up * velocityY;

        controller.Move(velocity * Time.deltaTime);
        currentSpeed = new Vector2(controller.velocity.x, controller.velocity.z).magnitude;

        if (controller.isGrounded)
        {
            velocityY = 0;
        }
    }

    void Jump()
    {
        if (controller.isGrounded)
        {
            float jumpVelocity = Mathf.Sqrt(-2 * gravity * jumpHeight);
            velocityY = jumpVelocity;
        }
    }

    float getModifiedSmoothTime(float smoothTime)
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

    // UI
    private void OnGUI()
    {
        if (currentKey != null)
        {
            Event e = Event.current;
            if (e.isKey && e.keyCode != KeyCode.Escape)
            {
                currentKey.transform.GetChild(0).GetComponent<Text>().text = e.keyCode.ToString();
                keys[currentKey.name] = e.keyCode;

                currentKey = null;
            }
            if (e.isMouse)
            {
                currentKey.transform.GetChild(0).GetComponent<Text>().text = "Mouse" + e.button;
                keys[currentKey.name] = (KeyCode)Enum.Parse(typeof(KeyCode), "Mouse" + e.button);

                currentKey = null;
            }
        }
    }

    /*void ToggleSelectors(bool state)
    {
        if (allSelectors == null)
        {
            allSelectors = GameObject.FindGameObjectsWithTag("Selector");
        }
        foreach (GameObject selector in allSelectors)
        {
            selector.SetActive(state);
        }
    }*/

    public IEnumerator Popup(string text)
    {
        popup.GetComponentInChildren<Text>().text = text;
        popup.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        popup.gameObject.SetActive(false);
    }

    public void PauseGame()
    {
        if (Time.timeScale == 0)
        {
            Cursor.lockState = CursorLockMode.Locked;
            gameMenu.gameObject.SetActive(false);
            optionsMenu.gameObject.SetActive(false);
            controlsMenu.gameObject.SetActive(false);
            Cursor.visible = false;
            Time.timeScale = 1;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            gameMenu.gameObject.SetActive(true);
            Cursor.visible = true;
            SaveSystem.SaveData(this);
            Time.timeScale = 0;
        }
    }

    // On value change for all UI elements
    public void UIGameObjects(GameObject current)
    {
        Transform currentParent = current.transform.parent;

        // Other UI elements
        switch (current.name)
        {
            // Buttons
            /*case "Selector":
                sceneLoader.GetComponent<SceneLoader>().LoadScene("Scene0");
                sceneLoader.GetComponent<SceneLoader>().LoadFromSave("Game0-Empty");
                switch (currentParent.name)
                {
                    case "SaveSlot1":
                        SaveSystem.SaveScene(GameObject.Find("Game0-Save1"));
                        break;
                    case "SaveSlot2":
                        SaveSystem.SaveScene(GameObject.Find("Game0-Save2"));
                        break;
                    case "SaveSlot3":
                        SaveSystem.SaveScene(GameObject.Find("Game0-Save3"));
                        break;
                }
                
                ToggleSelectors(false);
                break;*/
            case "Done":
                SaveSystem.SaveData(this);

                cameraT.GetComponent<Camera>().fieldOfView = fov;
                cameraT.GetComponent<CameraControl>().perspective = perspective;
                cameraT.GetComponent<CameraControl>().mouseSensitivity = mouseSensitivity;

                gameMenu.gameObject.SetActive(true);
                currentParent.transform.parent.gameObject.SetActive(false);
                break;
            /*case "NewGame":
                gameMenu.gameObject.SetActive(false);
                saveMenu.gameObject.SetActive(true);
                popup.gameObject.SetActive(true);
                popup.transform.Find("Ok").gameObject.SetActive(true);
                popup.transform.Find("Ok").GetComponent<Text>().text = "Select a Slot to overwrite.";
                ToggleSelectors(true);
                CheckUI();
                break;
            case "LoadGame":
                gameMenu.gameObject.SetActive(false);
                saveMenu.gameObject.SetActive(true);
                ToggleSelectors(false);
                CheckUI();
                break;
            case "Load":
                switch (currentParent.name)
                {
                    case "SaveSlot1":
                        LoadScene("Game0-Save1");
                        break;
                    case "SaveSlot2":
                        LoadScene("Game0-Save2");
                        break;
                    case "SaveSlot3":
                        LoadScene("Game0-Save3");
                        break;
                }
                break;
            case "Delete":
                popup.gameObject.SetActive(true);
                switch (currentParent.name)
                {
                    case "SaveSlot1":
                        deleteSelection = 1;
                        break;
                    case "SaveSlot2":
                        deleteSelection = 2;
                        break;
                    case "SaveSlot3":
                        deleteSelection = 3;
                        break;
                }
                popup.transform.Find("ConfirmDelete").gameObject.SetActive(true);
                popup.transform.Find("ConfirmDelete").GetComponent<Text>().text = "Are you sure you want to delete Save" + deleteSelection + "?";
                
                CheckUI();
                break;*/
            case "Play":
                sceneLoader.GetComponent<SceneLoader>().LoadScene("Scene0");
                break;
            case "Options":
                gameMenu.gameObject.SetActive(false);
                optionsMenu.gameObject.SetActive(true);
                CheckUI();
                break;
            case "Controls":
                gameMenu.gameObject.SetActive(false);
                controlsMenu.gameObject.SetActive(true);
                CheckUI();
                break;
            case "Exit":
                popup.gameObject.SetActive(true);
                popup.transform.Find("ConfirmExit").gameObject.SetActive(true);
                CheckUI();
                break;
            case "LoadCheckpoint":
                break;
            case "RestartGame":
                sceneLoader.GetComponent<SceneLoader>().LoadScene("Scene0");
                break;
            case "Save&Quit":
                SaveSystem.SaveData(this);

                //SaveSystem.SaveScene(GameObject.FindWithTag("Master"));
                sceneLoader.GetComponent<SceneLoader>().LoadScene("TitleScreen");
                break;
            // Dropdowns
            case "Difficulty":
                difficulty = current.gameObject.GetComponent<Dropdown>().value;
                break;
            case "Perspective":
                perspective = current.gameObject.GetComponent<Dropdown>().value;
                break;
            // Sliders
            case "FOV":
                fov = current.gameObject.GetComponent<Slider>().value;
                break;
            case "MouseSensitivity":
                mouseSensitivity = current.gameObject.GetComponent<Slider>().value;
                break;
            // Scrollbars
            case "Reset":
                keys["Up"] = KeyCode.W;
                keys["Left"] = KeyCode.A;
                keys["Down"] = KeyCode.S;
                keys["Right"] = KeyCode.D;
                keys["Run"] = KeyCode.LeftShift;
                keys["Jump"] = KeyCode.Space;
                keys["Attack"] = KeyCode.Mouse0;
                keys["Heal"] = KeyCode.Mouse1;
                keys["Interact"] = KeyCode.E;

                up.text = keys["Up"].ToString();
                left.text = keys["Left"].ToString();
                down.text = keys["Down"].ToString();
                right.text = keys["Right"].ToString();
                run.text = keys["Run"].ToString();
                jump.text = keys["Jump"].ToString();
                attack.text = keys["Attack"].ToString();
                heal.text = keys["Heal"].ToString();
                interact.text = keys["Interact"].ToString();
                break;
            case "Cancel":
                currentParent.gameObject.SetActive(false);
                currentParent.transform.parent.gameObject.SetActive(false);
                break;
            case "Ok":
                currentParent.gameObject.SetActive(false);
                currentParent.transform.parent.gameObject.SetActive(false);
                break;
            case "Yes":
                switch (currentParent.name)
                {
                    case "ConfirmExit":
                        SaveSystem.SaveData(this);
                        Application.Quit();
                        break;
                        /*case "ConfirmDelete":
                            if (File.Exists("Assets/Saves/Game0-Save" + deleteSelection))
                            {
                                File.Delete("Assets/Saves/Game0-Save" + deleteSelection);

                                currentParent.gameObject.SetActive(false);
                                popup.transform.Find("Ok").gameObject.SetActive(true);
                                popup.transform.Find("Ok").GetComponent<Text>().text = "Successfully deleted Game0-Save" + deleteSelection + ".";
                                CheckUI();
                            }
                            else
                            {
                                Debug.LogError("Save file not found in Assets/Saves/Game0-Save" + deleteSelection);

                                currentParent.gameObject.SetActive(false);
                                popup.transform.Find("Ok").gameObject.SetActive(true);
                                popup.transform.Find("Ok").GetComponent<Text>().text = "Error: Could not delete Game0-Save" + deleteSelection + "!";
                                CheckUI();
                            }
                            break;*/
                }
                break;
        }
        if (current.gameObject.GetComponent<Target>())
        {
            currentKey = current;
        }
    }

    public void CheckUI()
    {
        switch (UISelection)
        {
            case "UI0":
                Text[] allTexts = FindObjectsOfType<Text>();
                foreach (Text currentTarget in allTexts)
                {
                    switch (currentTarget.name)
                    {
                        case "Text0":
                            Text0(currentTarget);
                            currentTarget.fontSize = 30;
                            break;
                        case "Text1":
                            Text1(currentTarget);
                            currentTarget.fontSize = 18;
                            break;
                        case "Text2":
                            Text0(currentTarget);
                            currentTarget.fontSize = 18;
                            break;
                        case "Text3":
                            Text0(currentTarget);
                            currentTarget.fontSize = 20;
                            break;
                    }
                }

                Button[] allButtons = FindObjectsOfType<Button>();
                foreach (Button currentTarget in allButtons)
                {
                    switch (currentTarget.tag)
                    {
                        case "Style0":
                            currentTarget.colors = Style0(currentTarget.colors);
                            break;
                        case "Style1":
                            currentTarget.colors = Style1(currentTarget.colors);
                            break;
                        case "Selector":
                            currentTarget.colors = Selector(currentTarget.colors);
                            break;
                    }
                }

                Slider[] allSliders = FindObjectsOfType<Slider>();
                foreach (Slider currentTarget in allSliders)
                {
                    switch (currentTarget.tag)
                    {
                        case "Style0":
                            currentTarget.colors = Style0(currentTarget.colors);
                            break;
                        case "Style1":
                            currentTarget.colors = Style1(currentTarget.colors);
                            break;
                    }
                }

                Dropdown[] allDropdowns = FindObjectsOfType<Dropdown>();
                foreach (Dropdown currentTarget in allDropdowns)
                {
                    switch (currentTarget.tag)
                    {
                        case "Style0":
                            currentTarget.colors = Style0(currentTarget.colors);
                            break;
                        case "Style1":
                            currentTarget.colors = Style1(currentTarget.colors);
                            break;
                    }
                }

                InputField[] allInputFields = FindObjectsOfType<InputField>();
                foreach (InputField currentTarget in allInputFields)
                {
                    switch (currentTarget.tag)
                    {
                        case "Style0":
                            currentTarget.colors = Style0(currentTarget.colors);
                            break;
                        case "Style1":
                            currentTarget.colors = Style1(currentTarget.colors);
                            break;
                    }
                }

                Scrollbar[] allScrollbars = FindObjectsOfType<Scrollbar>();
                foreach (Scrollbar currentTarget in allScrollbars)
                {
                    switch (currentTarget.tag)
                    {
                        case "Style0":
                            currentTarget.colors = Style0(currentTarget.colors);
                            break;
                        case "Style1":
                            currentTarget.colors = Style1(currentTarget.colors);
                            break;
                    }
                }
                break;
            case "UI1":

                break;
        }
    }

    // Styling and color
    public void Text0(Text current)
    {
        current.color = new Color32(60, 60, 60, 255);
        current.fontStyle = FontStyle.Normal;
        current.fontSize = 30;
    }

    public void Text1(Text current)
    {
        current.color = new Color32(255, 255, 120, 255);
        current.fontStyle = FontStyle.Normal;
        current.fontSize = 18;
    }

    public void Text2(Text current)
    {
        Text0(current);
        current.fontSize = 18;
    }

    public ColorBlock Style0(ColorBlock current)
    {
        current.pressedColor = new Color32(200, 200, 200, 127);
        current.highlightedColor = new Color32(160, 160, 160, 255);
        current.normalColor = new Color32(100, 100, 100, 255);
        current.selectedColor = new Color32(80, 80, 80, 255);
        current.disabledColor = new Color32(80, 80, 80, 120);
        return current;
    }

    public ColorBlock Style1(ColorBlock current)
    {
        current.disabledColor = new Color32(200, 200, 200, 120);
        current.normalColor = new Color32(160, 160, 160, 255);
        current.highlightedColor = new Color32(100, 100, 100, 255);
        current.pressedColor = new Color32(80, 80, 80, 127);
        return current;
    }

    public ColorBlock Selector(ColorBlock current)
    {
        current.pressedColor = new Color32(200, 200, 200, 0);
        current.highlightedColor = new Color32(160, 160, 160, 120);
        current.normalColor = new Color32(100, 100, 100, 120);
        current.selectedColor = new Color32(80, 80, 80, 120);
        current.disabledColor = new Color32(80, 80, 80, 0);
        return current;
    }
}
