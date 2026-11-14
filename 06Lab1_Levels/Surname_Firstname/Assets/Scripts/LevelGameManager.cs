using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages objectives, collectible prefabs, checkpoints, and level transitions
/// between Level 1 and Level 2 for IT2107 06 Laboratory Exercise 1 (Levels).
/// </summary>
public class LevelGameManager : MonoBehaviour
{
    public static LevelGameManager Instance { get; private set; }

    [Header("Level Configuration")]
    public int levelNumber = 1;
    public string nextLevelSceneName = "Level2";
    public int totalOrbsInLevel = 3;
    public int collectedOrbs = 0;
    public bool levelCompleted = false;

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

    private void Start()
    {
        LevelPrefabItem[] orbs = FindObjectsByType<LevelPrefabItem>(FindObjectsSortMode.None);
        int count = 0;
        foreach (LevelPrefabItem item in orbs)
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
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartCurrentLevel();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SceneManager.LoadScene("Level1");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SceneManager.LoadScene("Level2");
        }
        else if (levelCompleted && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            LoadNextOrRestart();
        }
    }

    public void CollectOrb()
    {
        collectedOrbs++;
    }

    public void ReachGoalPortal()
    {
        if (levelCompleted) return;
        levelCompleted = true;

        if (!string.IsNullOrEmpty(nextLevelSceneName) && levelNumber == 1)
        {
            Invoke(nameof(LoadNextOrRestart), 1.5f);
        }
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
            fontSize = 19,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
        _textStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.92f, 0.96f, 1f) }
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

        GUI.Box(new Rect(15, 15, 470, 110), GUIContent.none, _boxStyle);
        GUI.Label(new Rect(28, 22, 440, 28), $"LEVEL {levelNumber}: Reach the Green Goal Portal!", _titleStyle);
        GUI.Label(new Rect(28, 50, 440, 24), $"Energy Orbs Collected: {collectedOrbs} / {totalOrbsInLevel}", _textStyle);
        GUI.Label(new Rect(28, 72, 440, 24), "Controls: [W/A/S/D or Arrows] Move & Turn | [SPACE] Jump", _textStyle);
        GUI.Label(new Rect(28, 94, 440, 24), "Switch Level: [1] Level 1  |  [2] Level 2  |  [R] Restart", _textStyle);

        if (levelCompleted)
        {
            float w = 520f;
            float h = 110f;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;

            GUI.color = new Color(0.15f, 0.75f, 0.35f, 0.95f);
            string msg = levelNumber == 1
                ? "LEVEL 1 COMPLETE!\nLoading Level 2..."
                : "ALL 2 LEVELS COMPLETED! VICTORY!\nPress [SPACE] to Play Again from Level 1";
            GUI.Box(new Rect(x, y, w, h), msg, _bannerStyle);
            GUI.color = Color.white;
        }
    }
}
