using UnityEngine;
using Unity.Cinemachine;
using System;
public class LevelController : MonoBehaviour
{
  [Header("Prefabs")]
  [SerializeField] private GameObject playerPrefab;
  [SerializeField] private GameObject platformPrefab;


  [SerializeField] private Transform goalTransform;
  [SerializeField] private PlacementController placementController;
  [SerializeField] private CinemachineCamera virtualCamera;
  [SerializeField] private CameraController cameraController;
  [SerializeField] private TrailController trailController;

  private PlayerController playerController;
  private LevelData currentLevel;
  private Goal goal;

  void Awake()
  {
    GameManager.Instance.RegisterLevelController(this);
  }

  void Start()
  {
    LoadLevel(GameManager.Instance.SelectedLevel);
  }
  void OnDestroy()
  {
    if (playerController != null)
      playerController.OnPlayerLost -= HandlePlayerLost;
    if (goal != null)
      goal.OnGoalReached -= HandleGoalReached;
  }
  private void LoadLevel(int levelIndex)
  {
    currentLevel = Resources.Load<LevelData>($"Levels/Level_{levelIndex:D2}");

    if (currentLevel == null)
    {
      Debug.LogError($"LevelData not found for index {levelIndex}");
      return;
    }
    SetupLevel();
  }

  private void SetupLevel()
  {
    // Spawn and position bird
    GameObject playerInstance = Instantiate(playerPrefab, currentLevel.birdStartPosition, Quaternion.identity);
    playerController = playerInstance.GetComponent<PlayerController>();
    cameraController.SetFollowBird(false);
    playerController.OnPlayerLost += HandlePlayerLost;
    trailController.SetPlayerController(playerController);

    goal = goalTransform.GetComponent<Goal>();
    goal.OnGoalReached += HandleGoalReached;
    goalTransform.position = currentLevel.goalPosition;

    // Spawn obstacles
    foreach (var obstacleData in currentLevel.obstacles)
      SpawnObstacle(obstacleData);

    placementController.SetBudget(ElementType.Blower, currentLevel.blowerCount);

    // Notify game state
    GameManager.Instance.GameStateController.SetState(GameState.LevelEditing);

    cameraController.PlayIntro(
       playerInstance.transform,
       goalTransform,
       currentLevel.obstacles);
  }

  public void RetryLevel()
  {
    cameraController.SetFollowBird(false);
    playerController.ResetToPosition(currentLevel.birdStartPosition);
    cameraController.ResetToGameplayPosition(playerController.transform);
    GameManager.Instance.GameStateController.SetState(GameState.LevelEditing);
  }

  private void SpawnObstacle(ObstacleData data)
  {
    GameObject prefab = GetPrefabForType(data.type);

    if (prefab == null)
    {
      Debug.LogWarning($"No prefab found for obstacle type {data.type}");
      return;
    }

    Instantiate(prefab, data.position, Quaternion.Euler(0, 0, data.rotation));
  }
  public void ClearElements()
  {
    placementController.ClearAll();
  }

  private GameObject GetPrefabForType(ObstacleType type)
  {
    return type switch
    {
      ObstacleType.Platform => platformPrefab,
      _ => null
    };
  }

  public void StartSimulation()
  {
    cameraController.SetFollowBird(true, playerController.transform);
    playerController.StartSimulation();
    GameManager.Instance.GameStateController.SetState(GameState.Playing);
  }
  private void HandleGoalReached()
  {
    playerController.StopSimulation();
    GameManager.Instance.GameStateController.SetState(GameState.LevelComplete);
  }
  private void HandlePlayerLost()
  {
    trailController.RecordDeath(playerController.Position);
    playerController.StopSimulation();
    GameManager.Instance.GameStateController.SetState(GameState.GameOver);
  }

  public LevelData GetCurrentLevel() => currentLevel;
}
