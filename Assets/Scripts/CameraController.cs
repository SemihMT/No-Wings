using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private CinemachineCamera cinemachineCamera;
  [SerializeField] private CinemachineConfiner2D cinemachineConfiner;
  [SerializeField] private Camera mainCamera;

  [Header("Zoom Settings")]
  [SerializeField] private float minZoom = 3f;
  [SerializeField] private float maxZoom = 15f;

  [Header("Intro Timing")]
  [SerializeField] private float overviewDuration = 2f;
  [SerializeField] private float birdToGoalDuration = 2f;
  [SerializeField] private float returnDuration = 1.5f;
  [SerializeField] private float pauseDuration = 0.75f;

  [Header("Overview")]
  [SerializeField] private float overviewPadding = 2f;
  [SerializeField] private float gameplayZoom = 5f;

  private Transform cameraTarget;

  private void Awake()
  {
    cameraTarget = new GameObject("Camera Target").transform;
    cinemachineCamera.Follow = cameraTarget;
  }

  private void InvalidateConfinerLensCache()
  {
    cinemachineConfiner.InvalidateLensCache();
  }

  public void PlayIntro(Transform bird, Transform goal, ObstacleData[] obstacles)
  {
    if (GameManager.Instance.HasPlayedIntro)
    {
      SkipToGameplay(bird);
      return;
    }
    StartCoroutine(IntroSequence(bird, goal, obstacles));
  }
  public void ResetToGameplayPosition(Transform bird)
  {
    Vector3 startPos = bird.position;
    startPos.y = gameplayZoom;
    startPos.z = 0f;
    cameraTarget.position = startPos;
    cinemachineCamera.Follow = cameraTarget;
    cinemachineCamera.Lens.OrthographicSize = gameplayZoom;
    cinemachineCamera.ForceCameraPosition(startPos, Quaternion.identity);
    InvalidateConfinerLensCache();
  }
  private void SkipToGameplay(Transform bird)
  {
    ResetToGameplayPosition(bird);
    GameManager.Instance.GameStateController.SetState(GameState.LevelEditing);
  }

  private IEnumerator IntroSequence(Transform bird, Transform goal, ObstacleData[] obstacles)
  {
    GameManager.Instance.GameStateController.SetState(GameState.Intro);

    Bounds bounds = CalculateLevelBounds(bird.position, goal.position, obstacles);
    Vector3 overviewPos = bounds.center;
    overviewPos.z = 0f;
    float overviewSize = CalculateOrthographicSize(bounds);

    cameraTarget.position = overviewPos;
    cinemachineCamera.Lens.OrthographicSize = overviewSize;
    cinemachineCamera.ForceCameraPosition(overviewPos, Quaternion.identity);
    InvalidateConfinerLensCache();

    yield return new WaitForSeconds(overviewDuration);

    yield return MoveCamera(bird.position, gameplayZoom, birdToGoalDuration);
    yield return new WaitForSeconds(pauseDuration);

    yield return MoveCamera(goal.position, gameplayZoom, birdToGoalDuration);
    yield return new WaitForSeconds(pauseDuration);

    Vector3 launchPos = bird.position;
    launchPos.y = gameplayZoom;
    yield return MoveCamera(launchPos, gameplayZoom, returnDuration);

    GameManager.Instance.HasPlayedIntro = true;
    GameManager.Instance.GameStateController.SetState(GameState.LevelEditing);
  }

  private IEnumerator MoveCamera(Vector3 targetPos, float targetZoom, float duration)
  {
    Vector3 startPos = cameraTarget.position;
    startPos.z = 0f;
    targetPos.z = 0f;

    float startZoom = cinemachineCamera.Lens.OrthographicSize;
    float elapsed = 0f;

    while (elapsed < duration)
    {
      elapsed += Time.deltaTime;
      float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

      cameraTarget.position = Vector3.Lerp(startPos, targetPos, t);
      cinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(startZoom, targetZoom, t);
      InvalidateConfinerLensCache();

      yield return null;
    }

    cameraTarget.position = targetPos;
    cinemachineCamera.Lens.OrthographicSize = targetZoom;
    InvalidateConfinerLensCache();
  }

  private Bounds CalculateLevelBounds(Vector2 birdPos, Vector2 goalPos, ObstacleData[] obstacles)
  {
    Bounds bounds = new Bounds(birdPos, Vector3.zero);
    bounds.Encapsulate(goalPos);
    foreach (var obstacle in obstacles)
      bounds.Encapsulate(obstacle.position);
    bounds.Expand(overviewPadding * 2f);
    return bounds;
  }

  private float CalculateOrthographicSize(Bounds bounds)
  {
    float verticalSize = bounds.size.y * 0.5f;
    float horizontalSize = bounds.size.x / mainCamera.aspect * 0.5f;
    return Mathf.Max(verticalSize, horizontalSize);
  }

  public void DragCamera(Vector2 worldDelta)
  {
    Vector3 newPosition = cameraTarget.position + new Vector3(worldDelta.x, worldDelta.y, 0f);

    cameraTarget.position = newPosition;
    //cinemachineCamera.ForceCameraPosition(cameraTarget.position, Quaternion.identity);
  }
  public void SetFollowBird(bool follow, Transform bird = null)
  {
    cinemachineCamera.Follow = follow ? bird : cameraTarget;
  }

  public void Zoom(float delta)
  {
    float newSize = Mathf.Clamp(
        cinemachineCamera.Lens.OrthographicSize + delta,
        minZoom,
        maxZoom
    );
    cinemachineCamera.Lens.OrthographicSize = newSize;
    InvalidateConfinerLensCache();
  }

}
