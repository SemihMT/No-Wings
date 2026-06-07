using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class LevelDesignTool : EditorWindow
{
    // Tool state
    private enum PlacementMode { None, BirdStart, Goal, Obstacle, Blower }
    private PlacementMode currentMode = PlacementMode.None;

    // References
    private LevelDesignRoot designRoot;
    private LevelData targetLevelData;

    // Prefabs
    private GameObject blowerPrefab;
    private GameObject platformPrefab;
    private GameObject playerPrefab;
    private GameObject goalPrefab;

    // Simulation
    private List<Vector2> trajectoryPoints = new List<Vector2>();
    private int simulationSteps = 500;
    private float simulationTimeStep = 0.02f;
    private int lastHash;

    // Scroll
    private Vector2 scrollPos;

    [MenuItem("No Wings/Level Design Tool")]
    public static void OpenWindow()
    {
        var window = GetWindow<LevelDesignTool>("Level Design Tool");
        window.minSize = new Vector2(300, 500);
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        EditorApplication.update += EditorUpdate;
        FindOrCreateDesignRoot();
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        EditorApplication.update -= EditorUpdate;
    }

    private void EditorUpdate()
    {
        if (designRoot == null)
            return;

        int hash = CalculateSceneHash();

        if (hash != lastHash)
        {
            lastHash = hash;
            SimulateTrajectory();
            Repaint();
        }
    }

    private int CalculateSceneHash()
    {
        unchecked
        {
            int hash = 17;

            if (designRoot.BirdStart != null)
            {
                hash = hash * 23 + designRoot.BirdStart.position.GetHashCode();
            }

            foreach (var blower in designRoot.GetBlowers())
            {
                if (blower == null)
                    continue;

                hash = hash * 23 + blower.transform.position.GetHashCode();
                hash = hash * 23 + blower.transform.rotation.GetHashCode();
                hash = hash * 23 + blower.BlowForce.GetHashCode();
                hash = hash * 23 + blower.BlowRange.GetHashCode();
            }

            return hash;
        }
    }

    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        DrawPrefabSection();
        EditorGUILayout.Space();
        DrawTargetLevelSection();
        EditorGUILayout.Space();
        DrawPlacementSection();
        EditorGUILayout.Space();
        DrawSimulationSection();
        EditorGUILayout.Space();
        DrawExportSection();
        EditorGUILayout.Space();
        DrawDangerSection();

        EditorGUILayout.EndScrollView();
    }

    private void DrawPrefabSection()
    {
        EditorGUILayout.LabelField("Prefabs", EditorStyles.boldLabel);
        blowerPrefab = (GameObject)EditorGUILayout.ObjectField("Blower", blowerPrefab, typeof(GameObject), false);
        platformPrefab = (GameObject)EditorGUILayout.ObjectField("Platform", platformPrefab, typeof(GameObject), false);
        playerPrefab = (GameObject)EditorGUILayout.ObjectField("Player", playerPrefab, typeof(GameObject), false);
        goalPrefab = (GameObject)EditorGUILayout.ObjectField("Goal", goalPrefab, typeof(GameObject), false);
    }

    private void DrawTargetLevelSection()
    {
        EditorGUILayout.LabelField("Target Level", EditorStyles.boldLabel);
        targetLevelData = (LevelData)EditorGUILayout.ObjectField("Level Data", targetLevelData, typeof(LevelData), false);

        if (targetLevelData != null && GUILayout.Button("Load Into Scene"))
            LoadLevelIntoScene();
    }

    private void DrawPlacementSection()
    {
        EditorGUILayout.LabelField("Placement", EditorStyles.boldLabel);

        DrawModeButton("Set Bird Start", PlacementMode.BirdStart);
        DrawModeButton("Set Goal", PlacementMode.Goal);
        DrawModeButton("Place Platform", PlacementMode.Obstacle);
        DrawModeButton("Place Blower", PlacementMode.Blower);

        if (currentMode != PlacementMode.None)
        {
            EditorGUILayout.HelpBox($"Click in Scene view to place: {currentMode}", MessageType.Info);
            if (GUILayout.Button("Cancel"))
                currentMode = PlacementMode.None;
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Clear All Placed Objects"))
            ClearDesignRoot();
    }

    private void DrawModeButton(string label, PlacementMode mode)
    {
        GUI.backgroundColor = currentMode == mode ? Color.green : Color.white;
        if (GUILayout.Button(label))
            currentMode = currentMode == mode ? PlacementMode.None : mode;
        GUI.backgroundColor = Color.white;
    }

    private void DrawSimulationSection()
    {
        EditorGUILayout.LabelField("Trajectory Preview", EditorStyles.boldLabel);

        simulationSteps = EditorGUILayout.IntSlider(
            "Steps",
            simulationSteps,
            100,
            2000);

        simulationTimeStep = EditorGUILayout.Slider(
            "Time Step",
            simulationTimeStep,
            0.005f,
            0.05f);

        if (GUILayout.Button("Simulate Trajectory"))
            SimulateTrajectory();

        if (GUILayout.Button("Clear Trajectory"))
            trajectoryPoints.Clear();
    }

    private void DrawExportSection()
    {
        EditorGUILayout.LabelField("Export", EditorStyles.boldLabel);

        if (GUILayout.Button("Export to LevelData"))
            ExportToLevelData();

        if (GUILayout.Button("Create New LevelData"))
            CreateNewLevelData();
    }

    private void DrawDangerSection()
    {
        EditorGUILayout.LabelField("Danger Zone", EditorStyles.boldLabel);
        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("Destroy Design Root"))
        {
            if (EditorUtility.DisplayDialog("Destroy Design Root",
                "This will delete all placed objects. Are you sure?", "Yes", "Cancel"))
                DestroyDesignRoot();
        }
        GUI.backgroundColor = Color.white;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        // Always draw trajectory
        if (trajectoryPoints.Count > 1)
        {
            Handles.color = new Color(1f, 0.5f, 0f, 0.8f);

            for (int i = 0; i < trajectoryPoints.Count - 1; i++)
            {
                Handles.DrawLine(
                    trajectoryPoints[i],
                    trajectoryPoints[i + 1]);
            }
        }

        if (currentMode == PlacementMode.None)
            return;

        Event e = Event.current;

        HandleUtility.AddDefaultControl(
            GUIUtility.GetControlID(FocusType.Passive));

        if (e.type == EventType.MouseDown &&
            e.button == 0 &&
            !e.alt)
        {
            Plane plane = new Plane(Vector3.forward, Vector3.zero);
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            if (plane.Raycast(ray, out float distance))
            {
                Vector3 worldPos = ray.GetPoint(distance);
                HandlePlacement(worldPos);
                e.Use();
            }
        }
    }

    private void HandlePlacement(Vector2 worldPos)
    {
        FindOrCreateDesignRoot();

        Undo.RegisterCompleteObjectUndo(
            designRoot.gameObject,
            "Level Design Change");

        switch (currentMode)
        {
            case PlacementMode.BirdStart:
                designRoot.SetBirdStart(worldPos, playerPrefab);
                currentMode = PlacementMode.None;
                break;

            case PlacementMode.Goal:
                designRoot.SetGoal(worldPos, goalPrefab);
                currentMode = PlacementMode.None;
                break;

            case PlacementMode.Obstacle:
                if (platformPrefab != null)
                    designRoot.AddObstacle(worldPos, platformPrefab);
                break;

            case PlacementMode.Blower:
                if (blowerPrefab != null)
                    designRoot.AddBlower(worldPos, blowerPrefab);
                break;
        }

        EditorSceneManager.MarkSceneDirty(
            SceneManager.GetActiveScene());
    }

    private void SimulateTrajectory()
    {
        trajectoryPoints.Clear();

        if (designRoot == null || designRoot.BirdStart == null)
        {
            Debug.LogWarning("No bird start position set.");
            return;
        }

        Vector2 pos = designRoot.BirdStart.position;
        Vector2 vel = Vector2.zero;

        trajectoryPoints.Add(pos);

        var blowers = designRoot.GetBlowers();

        for (int i = 0; i < simulationSteps; i++)
        {
            vel += Physics2D.gravity * simulationTimeStep;

            foreach (var blower in blowers)
            {
                if (blower == null)
                    continue;

                if (Vector2.Distance(
                    pos,
                    blower.transform.position) <= blower.BlowRange)
                {
                    vel +=
                        ((Vector2)blower.transform.up *
                        blower.BlowForce) *
                        simulationTimeStep;
                }
            }

            pos += vel * simulationTimeStep;

            trajectoryPoints.Add(pos);

            if (pos.y < -20f)
                break;
        }

        SceneView.RepaintAll();
    }

    private void FindOrCreateDesignRoot()
    {
        if (designRoot != null) return;
        designRoot = FindFirstObjectByType<LevelDesignRoot>();
        if (designRoot == null)
        {
            var go = new GameObject("LevelDesignRoot");
            designRoot = go.AddComponent<LevelDesignRoot>();
        }
    }

    private void ClearDesignRoot()
    {
        FindOrCreateDesignRoot();
        designRoot.ClearAll();
        trajectoryPoints.Clear();
    }

    private void DestroyDesignRoot()
    {
        var root = FindFirstObjectByType<LevelDesignRoot>();
        if (root != null) DestroyImmediate(root.gameObject);
        designRoot = null;
        trajectoryPoints.Clear();
    }

    private void ExportToLevelData()
    {
        if (targetLevelData == null)
        {
            Debug.LogWarning("No LevelData asset selected.");
            return;
        }

        if (designRoot == null)
        {
            Debug.LogWarning("No design root in scene.");
            return;
        }

        designRoot.ExportTo(targetLevelData);
        EditorUtility.SetDirty(targetLevelData);
        AssetDatabase.SaveAssets();
        Debug.Log($"Exported to {targetLevelData.name}");
    }

    private void CreateNewLevelData()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create Level Data",
            "Level_00",
            "asset",
            "Choose where to save the new LevelData",
            "Assets/Resources/Levels"
        );

        if (string.IsNullOrEmpty(path)) return;

        var newLevel = CreateInstance<LevelData>();
        AssetDatabase.CreateAsset(newLevel, path);
        AssetDatabase.SaveAssets();
        targetLevelData = newLevel;
        Repaint();
    }

    private void LoadLevelIntoScene()
    {
        if (targetLevelData == null) return;
        FindOrCreateDesignRoot();
        designRoot.ClearAll();
        designRoot.LoadFrom(targetLevelData, playerPrefab, goalPrefab, platformPrefab, blowerPrefab);
    }
}
