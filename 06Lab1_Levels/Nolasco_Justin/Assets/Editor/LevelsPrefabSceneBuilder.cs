#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Automatically creates modular 3D Prefabs in Assets/Prefabs/ and builds
/// two (2) complete prefab-based levels (Level1.unity and Level2.unity)
/// for IT2107 06 Laboratory Exercise 1 (Levels).
/// </summary>
[InitializeOnLoad]
public static class LevelsPrefabSceneBuilder
{
    private const string AutoInitKey = "06Lab1_Levels_Initialized_v1";

    static LevelsPrefabSceneBuilder()
    {
        EditorApplication.delayCall += () =>
        {
            if (Application.isPlaying) return;
            if (!SessionState.GetBool(AutoInitKey, false))
            {
                SessionState.SetBool(AutoInitKey, true);
                if (!File.Exists("Assets/Prefabs/PlayerKartPrefab.prefab") ||
                    !File.Exists("Assets/Scenes/Level1.unity") ||
                    !File.Exists("Assets/Scenes/Level2.unity"))
                {
                    BuildAllPrefabsAndLevels();
                }
            }
        };
    }

    [MenuItem("Tools/06Lab1 Levels/Build Prefabs & 2 Game Levels (Level1 & Level2)")]
    public static void BuildAllPrefabsAndLevels()
    {
        EnsureDirectories();
        CreateAllPrefabs();
        BuildLevel2Scene();
        BuildLevel1Scene();

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/Level1.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Level2.unity", true)
        };

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[06Lab1 Levels] Successfully built 8 Prefabs and 2 Game Levels (Level1.unity & Level2.unity)!");
    }

    private static void EnsureDirectories()
    {
        string[] dirs = { "Assets/Materials", "Assets/Prefabs", "Assets/Scenes", "Assets/Scripts" };
        foreach (string dir in dirs)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
    }

    private static Material GetOrCreateMaterial(string name, Color color)
    {
        string path = $"Assets/Materials/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            mat = new Material(shader) { color = color };
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            AssetDatabase.CreateAsset(mat, path);
        }
        return mat;
    }

    private static void CreateAllPrefabs()
    {
        Material floorMat = GetOrCreateMaterial("TrackFloorMat", new Color(0.22f, 0.25f, 0.30f));
        Material wallMat = GetOrCreateMaterial("WallBarrierMat", new Color(0.15f, 0.48f, 0.85f));
        Material rampMat = GetOrCreateMaterial("BoostRampMat", new Color(0.95f, 0.55f, 0.12f));
        Material orbMat = GetOrCreateMaterial("CollectibleOrbMat", new Color(1.0f, 0.85f, 0.15f));
        Material hazardMat = GetOrCreateMaterial("MovingHazardMat", new Color(0.90f, 0.18f, 0.22f));
        Material archMat = GetOrCreateMaterial("CheckpointArchMat", new Color(0.35f, 0.80f, 0.95f));
        Material goalMat = GetOrCreateMaterial("GoalPortalMat", new Color(0.18f, 0.92f, 0.42f));
        Material playerMat = GetOrCreateMaterial("PlayerKartMat", new Color(0.20f, 0.88f, 0.75f));

        // 1. TrackFloorPrefab
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "TrackFloorPrefab";
        floor.transform.localScale = new Vector3(10f, 1f, 12f);
        floor.GetComponent<Renderer>().sharedMaterial = floorMat;
        PrefabUtility.SaveAsPrefabAsset(floor, "Assets/Prefabs/TrackFloorPrefab.prefab");
        Object.DestroyImmediate(floor);

        // 2. WallBarrierPrefab
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "WallBarrierPrefab";
        wall.transform.localScale = new Vector3(0.8f, 2.2f, 12f);
        wall.GetComponent<Renderer>().sharedMaterial = wallMat;
        PrefabUtility.SaveAsPrefabAsset(wall, "Assets/Prefabs/WallBarrierPrefab.prefab");
        Object.DestroyImmediate(wall);

        // 3. BoostRampPrefab
        GameObject ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ramp.name = "BoostRampPrefab";
        ramp.transform.localScale = new Vector3(4f, 0.5f, 4f);
        ramp.transform.localRotation = Quaternion.Euler(-18f, 0f, 0f);
        ramp.GetComponent<Renderer>().sharedMaterial = rampMat;
        LevelPrefabItem rampItem = ramp.AddComponent<LevelPrefabItem>();
        rampItem.itemType = LevelPrefabItem.PrefabType.BoostRamp;
        PrefabUtility.SaveAsPrefabAsset(ramp, "Assets/Prefabs/BoostRampPrefab.prefab");
        Object.DestroyImmediate(ramp);

        // 4. CollectibleOrbPrefab
        GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        orb.name = "CollectibleOrbPrefab";
        orb.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        orb.GetComponent<Collider>().isTrigger = true;
        orb.GetComponent<Renderer>().sharedMaterial = orbMat;
        LevelPrefabItem orbItem = orb.AddComponent<LevelPrefabItem>();
        orbItem.itemType = LevelPrefabItem.PrefabType.CollectibleOrb;
        PrefabUtility.SaveAsPrefabAsset(orb, "Assets/Prefabs/CollectibleOrbPrefab.prefab");
        Object.DestroyImmediate(orb);

        // 5. MovingHazardPrefab
        GameObject hazard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hazard.name = "MovingHazardPrefab";
        hazard.transform.localScale = new Vector3(2.4f, 1.8f, 1.2f);
        hazard.GetComponent<Renderer>().sharedMaterial = hazardMat;
        LevelPrefabItem hazardItem = hazard.AddComponent<LevelPrefabItem>();
        hazardItem.itemType = LevelPrefabItem.PrefabType.MovingHazard;
        hazardItem.moveAmplitude = 3.2f;
        PrefabUtility.SaveAsPrefabAsset(hazard, "Assets/Prefabs/MovingHazardPrefab.prefab");
        Object.DestroyImmediate(hazard);

        // 6. CheckpointArchPrefab
        GameObject archRoot = new GameObject("CheckpointArchPrefab");
        BoxCollider archTrigger = archRoot.AddComponent<BoxCollider>();
        archTrigger.isTrigger = true;
        archTrigger.size = new Vector3(8f, 4f, 1f);
        archTrigger.center = new Vector3(0f, 2f, 0f);
        LevelPrefabItem archItem = archRoot.AddComponent<LevelPrefabItem>();
        archItem.itemType = LevelPrefabItem.PrefabType.CheckpointArch;

        GameObject leftPillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        leftPillar.name = "LeftPillar";
        leftPillar.transform.SetParent(archRoot.transform);
        leftPillar.transform.localPosition = new Vector3(-4f, 2f, 0f);
        leftPillar.transform.localScale = new Vector3(0.6f, 2f, 0.6f);
        leftPillar.GetComponent<Renderer>().sharedMaterial = archMat;

        GameObject rightPillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rightPillar.name = "RightPillar";
        rightPillar.transform.SetParent(archRoot.transform);
        rightPillar.transform.localPosition = new Vector3(4f, 2f, 0f);
        rightPillar.transform.localScale = new Vector3(0.6f, 2f, 0.6f);
        rightPillar.GetComponent<Renderer>().sharedMaterial = archMat;

        GameObject topBeam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        topBeam.name = "TopBanner";
        topBeam.transform.SetParent(archRoot.transform);
        topBeam.transform.localPosition = new Vector3(0f, 4.1f, 0f);
        topBeam.transform.localScale = new Vector3(9f, 0.6f, 0.8f);
        topBeam.GetComponent<Renderer>().sharedMaterial = archMat;

        PrefabUtility.SaveAsPrefabAsset(archRoot, "Assets/Prefabs/CheckpointArchPrefab.prefab");
        Object.DestroyImmediate(archRoot);

        // 7. GoalPortalPrefab
        GameObject goal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        goal.name = "GoalPortalPrefab";
        goal.transform.localScale = new Vector3(3.5f, 2.2f, 3.5f);
        goal.GetComponent<Collider>().isTrigger = true;
        goal.GetComponent<Renderer>().sharedMaterial = goalMat;
        LevelPrefabItem goalItem = goal.AddComponent<LevelPrefabItem>();
        goalItem.itemType = LevelPrefabItem.PrefabType.GoalPortal;
        PrefabUtility.SaveAsPrefabAsset(goal, "Assets/Prefabs/GoalPortalPrefab.prefab");
        Object.DestroyImmediate(goal);

        // 8. PlayerKartPrefab
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "PlayerKartPrefab";
        player.tag = "Player";
        player.GetComponent<Renderer>().sharedMaterial = playerMat;

        GameObject visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visor.name = "KartFrontNose";
        visor.transform.SetParent(player.transform);
        visor.transform.localPosition = new Vector3(0f, 0.2f, 0.6f);
        visor.transform.localScale = new Vector3(0.8f, 0.4f, 0.8f);
        Object.DestroyImmediate(visor.GetComponent<Collider>());
        visor.GetComponent<Renderer>().sharedMaterial = orbMat;

        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.mass = 1.5f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        player.AddComponent<LevelPlayerController>();

        PrefabUtility.SaveAsPrefabAsset(player, "Assets/Prefabs/PlayerKartPrefab.prefab");
        Object.DestroyImmediate(player);
    }

    private static void SetupSceneLightingAndCamera(int levelNumber, string nextScene, Color skyColor)
    {
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = skyColor;
        camObj.AddComponent<AudioListener>();
        camObj.transform.position = new Vector3(0f, 5f, -26f);
        camObj.transform.rotation = Quaternion.Euler(18f, 0f, 0f);

        GameObject lightObj = new GameObject("Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        GameObject gmObj = new GameObject("LevelGameManager");
        LevelGameManager gm = gmObj.AddComponent<LevelGameManager>();
        gm.levelNumber = levelNumber;
        gm.nextLevelSceneName = nextScene;
    }

    private static GameObject SpawnPrefab(string prefabPath, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
        instance.transform.position = position;
        instance.transform.rotation = rotation;
        if (parent != null) instance.transform.SetParent(parent, true);
        return instance;
    }

    private static void BuildLevel1Scene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SetupSceneLightingAndCamera(1, "Level2", new Color(0.14f, 0.22f, 0.36f));

        GameObject envRoot = new GameObject("Level1_PrefabEnvironment");

        // Build 5 straight modular floor & wall segments using Prefabs
        for (int i = 0; i < 5; i++)
        {
            float z = -16f + i * 12f;
            SpawnPrefab("Assets/Prefabs/TrackFloorPrefab.prefab", new Vector3(0f, -0.5f, z), Quaternion.identity, envRoot.transform);
            SpawnPrefab("Assets/Prefabs/WallBarrierPrefab.prefab", new Vector3(-5.2f, 1.0f, z), Quaternion.identity, envRoot.transform);
            SpawnPrefab("Assets/Prefabs/WallBarrierPrefab.prefab", new Vector3(5.2f, 1.0f, z), Quaternion.identity, envRoot.transform);
        }

        // Player Prefab at start
        SpawnPrefab("Assets/Prefabs/PlayerKartPrefab.prefab", new Vector3(0f, 1.1f, -18f), Quaternion.identity);

        // Ramps, Checkpoints, Orbs, Hazards, and Goal Portal (All Prefabs!)
        SpawnPrefab("Assets/Prefabs/CollectibleOrbPrefab.prefab", new Vector3(0f, 1.2f, -10f), Quaternion.identity, envRoot.transform);
        SpawnPrefab("Assets/Prefabs/BoostRampPrefab.prefab", new Vector3(0f, 0.4f, -3f), Quaternion.Euler(-16f, 0f, 0f), envRoot.transform);
        SpawnPrefab("Assets/Prefabs/CollectibleOrbPrefab.prefab", new Vector3(0f, 2.4f, 0f), Quaternion.identity, envRoot.transform);
        SpawnPrefab("Assets/Prefabs/CheckpointArchPrefab.prefab", new Vector3(0f, 0f, 8f), Quaternion.identity, envRoot.transform);
        SpawnPrefab("Assets/Prefabs/MovingHazardPrefab.prefab", new Vector3(0f, 1.0f, 15f), Quaternion.identity, envRoot.transform);
        SpawnPrefab("Assets/Prefabs/CollectibleOrbPrefab.prefab", new Vector3(0f, 1.2f, 22f), Quaternion.identity, envRoot.transform);
        SpawnPrefab("Assets/Prefabs/GoalPortalPrefab.prefab", new Vector3(0f, 1.8f, 30f), Quaternion.identity, envRoot.transform);

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Level1.unity");
    }

    private static void BuildLevel2Scene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SetupSceneLightingAndCamera(2, "Level1", new Color(0.24f, 0.14f, 0.34f));

        GameObject envRoot = new GameObject("Level2_PrefabEnvironment");

        // Multi-tier elevated course using Prefabs
        for (int i = 0; i < 6; i++)
        {
            float z = -16f + i * 12f;
            float y = (i >= 3) ? 2.5f : -0.5f;
            SpawnPrefab("Assets/Prefabs/TrackFloorPrefab.prefab", new Vector3(0f, y, z), Quaternion.identity, envRoot.transform);
            SpawnPrefab("Assets/Prefabs/WallBarrierPrefab.prefab", new Vector3(-5.2f, y + 1.5f, z), Quaternion.identity, envRoot.transform);
            SpawnPrefab("Assets/Prefabs/WallBarrierPrefab.prefab", new Vector3(5.2f, y + 1.5f, z), Quaternion.identity, envRoot.transform);
        }

        // Player Prefab at start of Level 2
        SpawnPrefab("Assets/Prefabs/PlayerKartPrefab.prefab", new Vector3(0f, 1.1f, -18f), Quaternion.identity);

        // Level 2 Gauntlet Prefabs (Double Hazards, Elevated Ramp Climb, 4 Orbs, Checkpoints, Final Goal)
        SpawnPrefab("Assets/Prefabs/CollectibleOrbPrefab.prefab", new Vector3(-2f, 1.2f, -10f), Quaternion.identity, envRoot.transform);
        SpawnPrefab("Assets/Prefabs/MovingHazardPrefab.prefab", new Vector3(0f, 1.0f, -4f), Quaternion.identity, envRoot.transform);
        SpawnPrefab("Assets/Prefabs/CheckpointArchPrefab.prefab", new Vector3(0f, 0f, 4f), Quaternion.identity, envRoot.transform);

        // Big ramp bridging lower deck to upper deck (i=2 to i=3)
        GameObject bigRamp = SpawnPrefab("Assets/Prefabs/BoostRampPrefab.prefab", new Vector3(0f, 1.2f, 13f), Quaternion.Euler(-22f, 0f, 0f), envRoot.transform);
        bigRamp.transform.localScale = new Vector3(6f, 0.6f, 8f);

        SpawnPrefab("Assets/Prefabs/CollectibleOrbPrefab.prefab", new Vector3(0f, 3.8f, 16f), Quaternion.identity, envRoot.transform);
        SpawnPrefab("Assets/Prefabs/CheckpointArchPrefab.prefab", new Vector3(0f, 3.0f, 22f), Quaternion.identity, envRoot.transform);
        SpawnPrefab("Assets/Prefabs/MovingHazardPrefab.prefab", new Vector3(0f, 4.0f, 28f), Quaternion.identity, envRoot.transform);
        SpawnPrefab("Assets/Prefabs/CollectibleOrbPrefab.prefab", new Vector3(2f, 4.2f, 33f), Quaternion.identity, envRoot.transform);
        SpawnPrefab("Assets/Prefabs/MovingHazardPrefab.prefab", new Vector3(0f, 4.0f, 37f), Quaternion.identity, envRoot.transform);
        SpawnPrefab("Assets/Prefabs/CollectibleOrbPrefab.prefab", new Vector3(0f, 4.2f, 41f), Quaternion.identity, envRoot.transform);
        SpawnPrefab("Assets/Prefabs/GoalPortalPrefab.prefab", new Vector3(0f, 4.8f, 46f), Quaternion.identity, envRoot.transform);

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Level2.unity");
    }
}
#endif
