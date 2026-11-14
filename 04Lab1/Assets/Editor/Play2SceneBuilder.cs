#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor utility that configures the 4 required sprites (04 Image 1..4)
/// and builds the Play2.unity scene for IT2012 04 Laboratory Exercise 1.
/// </summary>
[InitializeOnLoad]
public static class Play2SceneBuilder
{
    private const string AutoInitKey = "04Lab1_Play2_Initialized_v1";

    static Play2SceneBuilder()
    {
        EditorApplication.delayCall += () =>
        {
            if (Application.isPlaying) return;
            if (!SessionState.GetBool(AutoInitKey, false))
            {
                SessionState.SetBool(AutoInitKey, true);
                if (!File.Exists("Assets/Scenes/Play2.unity") || GameObject.Find("04 Image 2 (Player)") == null)
                {
                    BuildPlay2Scene();
                }
            }
        };
    }

    [MenuItem("Tools/04Lab1/Rebuild Play2 Scene")]
    public static void BuildPlay2Scene()
    {
        EnsureDirectories();
        ConfigureSprites();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 1. Main Camera (2D Orthographic)
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.51f, 0.69f, 0.80f, 1f);
        camObj.AddComponent<AudioListener>();
        camObj.transform.position = new Vector3(0f, 0f, -10f);

        // 2. GameManager
        GameObject gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.AddComponent<GameManager>();
        gm.targetScore = 10;

        // Load configured sprites
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/04_Image_1.png");
        Sprite playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/04_Image_2.png");
        Sprite obs1Sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/04_Image_3.png");
        Sprite obs2Sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/04_Image_4.png");

        // 3. Background (04 Image 1)
        GameObject bgObj = new GameObject("04 Image 1 (Background)");
        SpriteRenderer bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = bgSprite;
        bgSr.sortingOrder = -10;
        bgObj.transform.position = new Vector3(0f, 0f, 0f);
        // 400x224 @ 100 PPU = 4.0 x 2.24 units -> scale to cover 18 x 10 orthographic view
        bgObj.transform.localScale = new Vector3(4.5f, 4.5f, 1f);

        // 4. Sidewalk Ground Collider
        GameObject groundObj = new GameObject("Ground");
        groundObj.transform.position = new Vector3(0f, -4.25f, 0f);
        BoxCollider2D groundCol = groundObj.AddComponent<BoxCollider2D>();
        groundCol.size = new Vector2(30f, 1.5f);

        // 5. Player (04 Image 2)
        GameObject playerObj = new GameObject("04 Image 2 (Player)");
        playerObj.tag = "Player";
        SpriteRenderer playerSr = playerObj.AddComponent<SpriteRenderer>();
        playerSr.sprite = playerSprite;
        playerSr.sortingOrder = 5;
        // 172x258 @ 100 PPU = 1.72 x 2.58 units -> scale 0.9 => ~1.55 x 2.32 units
        playerObj.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
        playerObj.transform.position = new Vector3(-5.2f, -2.25f, 0f);

        Rigidbody2D playerRb = playerObj.AddComponent<Rigidbody2D>();
        playerRb.gravityScale = 2.8f;
        playerRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        playerRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        BoxCollider2D playerCol = playerObj.AddComponent<BoxCollider2D>();
        playerCol.size = new Vector2(1.35f, 2.48f);
        playerCol.offset = new Vector2(0f, 0f);

        PlayerController playerController = playerObj.AddComponent<PlayerController>();
        playerController.moveSpeed = 5.5f;
        playerController.jumpForce = 12.8f;
        playerController.groundYThreshold = -2.15f;

        // 6. Obstacle 1: Concrete Triangle (04 Image 3)
        GameObject obs1Obj = new GameObject("04 Image 3 (Obstacle 1)");
        SpriteRenderer obs1Sr = obs1Obj.AddComponent<SpriteRenderer>();
        obs1Sr.sprite = obs1Sprite;
        obs1Sr.sortingOrder = 3;
        // 672x636 @ 100 PPU = 6.72 x 6.36 units -> scale 0.24 => ~1.61 x 1.52 units
        obs1Obj.transform.localScale = new Vector3(0.24f, 0.24f, 1f);
        obs1Obj.transform.position = new Vector3(0.5f, -2.72f, 0f);

        PolygonCollider2D obs1Col = obs1Obj.AddComponent<PolygonCollider2D>();
        obs1Col.points = new Vector2[]
        {
            new Vector2(0.0f, 2.95f),
            new Vector2(-2.95f, -2.85f),
            new Vector2(2.95f, -2.85f)
        };

        ObstacleMover obs1Mover = obs1Obj.AddComponent<ObstacleMover>();
        obs1Mover.scrollObstacle = true;
        obs1Mover.moveSpeed = 4.2f;
        obs1Mover.allowGrindOnTop = false;
        obs1Mover.playerTransform = playerObj.transform;

        // 7. Obstacle 2: Brick Ledge (04 Image 4)
        GameObject obs2Obj = new GameObject("04 Image 4 (Obstacle 2)");
        SpriteRenderer obs2Sr = obs2Obj.AddComponent<SpriteRenderer>();
        obs2Sr.sprite = obs2Sprite;
        obs2Sr.sortingOrder = 3;
        // 269x200 @ 100 PPU = 2.69 x 2.00 units -> scale 0.65 => ~1.75 x 1.30 units
        obs2Obj.transform.localScale = new Vector3(0.65f, 0.65f, 1f);
        obs2Obj.transform.position = new Vector3(6.2f, -2.85f, 0f);

        BoxCollider2D obs2Col = obs2Obj.AddComponent<BoxCollider2D>();
        obs2Col.size = new Vector2(2.45f, 1.75f);

        ObstacleMover obs2Mover = obs2Obj.AddComponent<ObstacleMover>();
        obs2Mover.scrollObstacle = true;
        obs2Mover.moveSpeed = 4.2f;
        obs2Mover.allowGrindOnTop = true;
        obs2Mover.playerTransform = playerObj.transform;

        // Link obstacles to each other for spacing on respawn
        obs1Mover.otherObstacle = obs2Obj.transform;
        obs2Mover.otherObstacle = obs1Obj.transform;

        // Save Play2.unity in both Assets/Scenes/Play2.unity and Assets/Play2.unity
        string scenePath = "Assets/Scenes/Play2.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        EditorSceneManager.SaveScene(scene, "Assets/Play2.unity", true);

        // Register in Build Settings
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(scenePath, true)
        };

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[04Lab1] Successfully built Play2.unity scene!");
    }

    private static void EnsureDirectories()
    {
        if (!Directory.Exists("Assets/Scenes"))
            Directory.CreateDirectory("Assets/Scenes");
        if (!Directory.Exists("Assets/Sprites"))
            Directory.CreateDirectory("Assets/Sprites");
    }

    private static void ConfigureSprites()
    {
        string[] spritePaths =
        {
            "Assets/Sprites/04_Image_1.png",
            "Assets/Sprites/04_Image_2.png",
            "Assets/Sprites/04_Image_3.png",
            "Assets/Sprites/04_Image_4.png"
        };

        foreach (string path in spritePaths)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
        }
    }
}
#endif
