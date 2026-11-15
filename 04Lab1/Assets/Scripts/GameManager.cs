using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Manages the game state, score objective, and HUD for 04Lab1 (Play2 scene).
/// Compatible with both Unity Input System and Legacy Input Manager.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Objective Settings")]
    [Tooltip("Number of obstacles to clear to achieve the game objective.")]
    public int targetScore = 10;

    [Header("Current State")]
    public int currentScore = 0;
    public bool isGameOver = false;
    public bool hasWon = false;

    private GUIStyle _boxStyle;
    private GUIStyle _titleStyle;
    private GUIStyle _textStyle;
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
    }

    private void Update()
    {
        bool restartPressed = false;
        bool continuePressed = false;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            restartPressed = Keyboard.current.rKey.wasPressedThisFrame;
            continuePressed = Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame;
        }
#else
        restartPressed = Input.GetKeyDown(KeyCode.R);
        continuePressed = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return);
#endif

        if (restartPressed)
        {
            RestartGame();
        }
        else if (isGameOver && continuePressed)
        {
            RestartGame();
        }
    }

    public void AddScore(int amount = 1)
    {
        if (isGameOver) return;

        currentScore += amount;
        if (currentScore >= targetScore && !hasWon)
        {
            hasWon = true;
        }
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }

    private void InitStyles()
    {
        if (_boxStyle != null) return;

        _boxStyle = new GUIStyle(GUI.skin.box);
        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        _textStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.95f, 0.95f, 0.95f) }
        };

        _bannerStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
    }

    private void OnGUI()
    {
        InitStyles();

        // Top-left HUD panel
        GUI.Box(new Rect(15, 15, 420, 90), GUIContent.none, _boxStyle);
        GUI.Label(new Rect(28, 22, 400, 30), $"Objective: Avoid Obstacles ({currentScore} / {targetScore})", _titleStyle);
        GUI.Label(new Rect(28, 52, 400, 25), "Jump: [SPACE] / [W] / [UP ARROW]", _textStyle);
        GUI.Label(new Rect(28, 74, 400, 25), "Move: [A / D] or [LEFT / RIGHT]  |  Restart: [R]", _textStyle);

        if (hasWon && !isGameOver)
        {
            GUI.color = new Color(0.15f, 0.75f, 0.25f, 0.95f);
            GUI.Box(new Rect(Screen.width / 2f - 220f, 20f, 440f, 55f), "OBJECTIVE COMPLETE! Keep Skating or Press [R]", _bannerStyle);
            GUI.color = Color.white;
        }

        if (isGameOver)
        {
            float w = 460f;
            float h = 140f;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;

            GUI.color = new Color(0.85f, 0.15f, 0.15f, 0.95f);
            GUI.Box(new Rect(x, y, w, h), GUIContent.none, _boxStyle);
            GUI.color = Color.white;

            GUIStyle centerTitle = new GUIStyle(_titleStyle)
            {
                fontSize = 26,
                alignment = TextAnchor.MiddleCenter
            };
            GUIStyle centerSub = new GUIStyle(_textStyle)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };

            GUI.Label(new Rect(x, y + 20f, w, 40f), "CRASHED INTO OBSTACLE!", centerTitle);
            GUI.Label(new Rect(x, y + 62f, w, 30f), $"Obstacles Cleared: {currentScore}", centerSub);
            GUI.Label(new Rect(x, y + 95f, w, 30f), "Press [SPACE] or [R] to Try Again", centerSub);
        }
    }
}
