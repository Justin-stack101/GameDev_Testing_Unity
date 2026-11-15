using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Manages Objectives, Rules, Victory Conditions, and Loss Conditions for:
/// - IT2012 05 Laboratory Exercise 1 (Gameplay: Rules, Victory Condition, Loss Condition)
/// - IT2107 06 Laboratory Exercise 1 (Levels: Level 1 & Level 2 with Prefabs)
/// </summary>
public class LevelGameManager : MonoBehaviour
{
    public static LevelGameManager Instance { get; private set; }

    [Header("Level & Objective Configuration")]
    public int levelNumber = 1;
    public string nextLevelSceneName = "Level2";
    public int totalOrbsInLevel = 3;
    public int collectedOrbs = 0;

    [Header("Gameplay Rules (05Lab1 Requirement 3a)")]
    [Tooltip("Rule 1: Player must collect all Energy Orbs to unlock the Goal Portal.")]
    public bool requireAllOrbsForVictory = true;
    [Tooltip("Rule 2: Player must finish the level before the countdown timer reaches 0.")]
    public float timeLimitSeconds = 60f;
    public float timeRemaining = 60f;
    [Tooltip("Rule 3: Touching a red Moving Hazard or falling off the track costs 1 Life (max 3 lives).")]
    public int maxLives = 3;
    public int currentLives = 3;

    [Header("Victory & Loss States (05Lab1 Requirements 3b & 3c)")]
    public bool levelCompleted = false;
    public bool isGameOver = false;
    public string lossReason = "";
    public string statusNotice = "";
    private float _statusNoticeTimer = 0f;

    private GUIStyle _boxStyle;
    private GUIStyle _titleStyle;
    private GUIStyle _textStyle;
    private GUIStyle _ruleStyle;
    private GUIStyle _bannerStyle;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Time.timeScale = 1f;
        timeRemaining = timeLimitSeconds;
        currentLives = maxLives;
    }

    private void Start()
    {
        LevelPrefabItem[] items = FindObjectsByType<LevelPrefabItem>(FindObjectsInactive.Exclude);
        int count = 0;
        foreach (LevelPrefabItem item in items)
        {
            if (item.itemType == LevelPrefabItem.PrefabType.CollectibleOrb)
                count++;
        }
        if (count > 0)
        {
            totalOrbsInLevel = count;
        }
    }

    private void Update()
    {
        if (_statusNoticeTimer > 0f)
        {
            _statusNoticeTimer -= Time.unscaledDeltaTime;
            if (_statusNoticeTimer <= 0f) statusNotice = "";
        }

        // Countdown Timer Rule (Loss Condition 1: Time Runs Out)
        if (!levelCompleted && !isGameOver)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                TriggerGameOver("TIME EXPIRED! You ran out of time before completing the objective.");
            }
        }

        bool restartPressed = false;
        bool level1Pressed = false;
        bool level2Pressed = false;
        bool continuePressed = false;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            restartPressed = Keyboard.current.rKey.wasPressedThisFrame;
            level1Pressed = Keyboard.current.digit1Key.wasPressedThisFrame;
            level2Pressed = Keyboard.current.digit2Key.wasPressedThisFrame;
            continuePressed = Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame;
        }
#else
        restartPressed = Input.GetKeyDown(KeyCode.R);
        level1Pressed = Input.GetKeyDown(KeyCode.Alpha1);
        level2Pressed = Input.GetKeyDown(KeyCode.Alpha2);
        continuePressed = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return);
#endif

        if (restartPressed)
        {
            RestartCurrentLevel();
        }
        else if (level1Pressed)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Level1");
        }
        else if (level2Pressed)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Level2");
        }
        else if (isGameOver && continuePressed)
        {
            RestartCurrentLevel();
        }
        else if (levelCompleted && continuePressed)
        {
            LoadNextOrRestart();
        }
    }

    public void CollectOrb()
    {
        if (levelCompleted || isGameOver) return;
        collectedOrbs++;
        if (collectedOrbs >= totalOrbsInLevel)
        {
            ShowStatusNotice("All Energy Orbs Collected! Goal Portal is now UNLOCKED!");
        }
        else
        {
            ShowStatusNotice($"Orb Collected! ({collectedOrbs}/{totalOrbsInLevel})");
        }
    }

    public void OnPlayerHitHazardOrFell(string reason)
    {
        if (levelCompleted || isGameOver) return;

        currentLives--;
        if (currentLives <= 0)
        {
            currentLives = 0;
            TriggerGameOver(reason);
        }
        else
        {
            ShowStatusNotice($"{reason} (-1 Life | {currentLives} Lives Left)");
        }
    }

    public void ReachGoalPortal()
    {
        if (levelCompleted || isGameOver) return;

        // Enforce Rule 1: Must collect all orbs before Goal Portal triggers Victory
        if (requireAllOrbsForVictory && collectedOrbs < totalOrbsInLevel)
        {
            int remaining = totalOrbsInLevel - collectedOrbs;
            ShowStatusNotice($"PORTAL LOCKED! Rule: Collect {remaining} more Energy Orb(s) first!");
            return;
        }

        // Victory Condition Met!
        levelCompleted = true;
        if (!string.IsNullOrEmpty(nextLevelSceneName) && levelNumber == 1)
        {
            Invoke(nameof(LoadNextOrRestart), 2.0f);
        }
    }

    public void TriggerGameOver(string reason)
    {
        if (isGameOver || levelCompleted) return;
        isGameOver = true;
        lossReason = reason;
        Time.timeScale = 0f;
    }

    public void ShowStatusNotice(string message, float duration = 3f)
    {
        statusNotice = message;
        _statusNoticeTimer = duration;
    }

    public void RestartCurrentLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadNextOrRestart()
    {
        Time.timeScale = 1f;
        if (levelNumber == 1 && !string.IsNullOrEmpty(nextLevelSceneName))
        {
            SceneManager.LoadScene(nextLevelSceneName);
        }
        else
        {
            SceneManager.LoadScene("Level1");
        }
    }

    private void InitStyles()
    {
        if (_boxStyle != null) return;

        _boxStyle = new GUIStyle(GUI.skin.box);
        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
        _textStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.92f, 0.96f, 1f) }
        };
        _ruleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(1f, 0.90f, 0.45f) }
        };
        _bannerStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = 21,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
    }

    private void OnGUI()
    {
        InitStyles();

        // Top-Left HUD showing Rules, Victory Condition, Loss Condition, Timer, Lives, and Orbs
        GUI.Box(new Rect(15, 15, 510, 150), GUIContent.none, _boxStyle);
        GUI.Label(new Rect(28, 20, 480, 26), $"LEVEL {levelNumber} — MICROGAME GAMEPLAY & RULES", _titleStyle);
        GUI.Label(new Rect(28, 46, 480, 22), $"Orbs: {collectedOrbs} / {totalOrbsInLevel}   |   Time Left: {timeRemaining:0.0}s   |   Lives: {currentLives} / {maxLives}", _textStyle);
        GUI.Label(new Rect(28, 70, 480, 22), "VICTORY CONDITION: Collect all Orbs & enter the Green Goal Portal", _ruleStyle);
        GUI.Label(new Rect(28, 92, 480, 22), "LOSS CONDITION: Lose all 3 Lives (Hazards/Fall) or Timer hits 0s", _ruleStyle);
        GUI.Label(new Rect(28, 116, 480, 22), "RULES / CONTROLS: [WASD/Arrows] Drive & Steer  |  [SPACE] Jump", _textStyle);
        GUI.Label(new Rect(28, 136, 480, 22), "Switch Level: [1] Level 1   |   [2] Level 2   |   [R] Restart", _textStyle);

        // Temporary status notification banner
        if (!string.IsNullOrEmpty(statusNotice) && !levelCompleted && !isGameOver)
        {
            GUI.color = new Color(0.15f, 0.25f, 0.45f, 0.95f);
            GUI.Box(new Rect(Screen.width * 0.5f - 260f, 20f, 520f, 45f), statusNotice, _bannerStyle);
            GUI.color = Color.white;
        }

        // VICTORY SCREEN (Requirement 3b)
        if (levelCompleted)
        {
            float w = 540f;
            float h = 120f;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;

            GUI.color = new Color(0.12f, 0.72f, 0.32f, 0.95f);
            string msg = levelNumber == 1
                ? "VICTORY CONDITION MET! (LEVEL 1 COMPLETE)\nAll Orbs Collected & Goal Reached! Loading Level 2..."
                : "VICTORY! ALL LEVELS & OBJECTIVES COMPLETED!\nPress [SPACE] or [R] to Play Again";
            GUI.Box(new Rect(x, y, w, h), msg, _bannerStyle);
            GUI.color = Color.white;
        }

        // LOSS SCREEN (Requirement 3c)
        if (isGameOver)
        {
            float w = 560f;
            float h = 135f;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;

            GUI.color = new Color(0.85f, 0.15f, 0.18f, 0.95f);
            string msg = $"GAME OVER — LOSS CONDITION TRIGGERED!\n{lossReason}\nPress [SPACE] or [R] to Try Again";
            GUI.Box(new Rect(x, y, w, h), msg, _bannerStyle);
            GUI.color = Color.white;
        }
    }
}
