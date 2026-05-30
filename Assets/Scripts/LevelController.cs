using UnityEngine;
using Unity.Cinemachine;
public class LevelController : MonoBehaviour
{
  [Header("Prefabs")]
  [SerializeField] private GameObject playerPrefab;
  [SerializeField] private GameObject platformPrefab;


  [SerializeField] private Transform goalTransform;
  [SerializeField] private PlacementController placementController;
  [SerializeField] private CinemachineCamera virtualCamera;
  [SerializeField] private CameraController cameraController;

  private PlayerController playerController;
  private LevelData currentLevel;

  void Start()
  {
    LoadLevel(GameManager.Instance.SelectedLevel);
  }
  void OnDestroy()
  {
    if (playerController != null)
      playerController.OnPlayerLost -= HandlePlayerLost;
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
    playerController.OnPlayerLost += HandlePlayerLost;

    // Position goal
    goalTransform.position = currentLevel.goalPosition;

    cameraController.PlayIntro(
        playerInstance.transform,
        goalTransform,
        currentLevel.obstacles);


    // Spawn obstacles
    foreach (var obstacleData in currentLevel.obstacles)
      SpawnObstacle(obstacleData);

    placementController.SetBudget(ElementType.Blower, currentLevel.blowerCount);

    // Notify game state
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
    virtualCamera.Follow = playerController.transform;
    playerController.StartSimulation();
    GameManager.Instance.GameStateController.SetState(GameState.Playing);
  }

  private void HandlePlayerLost()
  {
    GameManager.Instance.GameStateController.SetState(GameState.GameOver);
  }

  public LevelData GetCurrentLevel() => currentLevel;
}
