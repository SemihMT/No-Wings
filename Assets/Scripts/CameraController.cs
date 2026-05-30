using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private CinemachineCamera cinemachineCamera;
  [SerializeField] private Camera mainCamera;

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

  public void PlayIntro(Transform bird, Transform goal, ObstacleData[] obstacles)
  {
    StartCoroutine(IntroSequence(bird, goal, obstacles));
  }

  private IEnumerator IntroSequence(Transform bird, Transform goal, ObstacleData[] obstacles)
  {
    Bounds bounds = CalculateLevelBounds(bird.position, goal.position, obstacles);

    Vector3 overviewPos = bounds.center;
    overviewPos.z = 0f;
    float overviewSize = CalculateOrthographicSize(bounds);

    // Teleport target and lens, then force Cinemachine to snap immediately
    cameraTarget.position = overviewPos;
    cinemachineCamera.Lens.OrthographicSize = overviewSize;
    cinemachineCamera.ForceCameraPosition(overviewPos, Quaternion.identity);

    yield return new WaitForSeconds(overviewDuration);

    yield return MoveCamera(bird.position, gameplayZoom, birdToGoalDuration);
    yield return new WaitForSeconds(pauseDuration);

    yield return MoveCamera(goal.position, gameplayZoom, birdToGoalDuration);
    yield return new WaitForSeconds(pauseDuration);

    Vector3 launchPos = bird.position;
    launchPos.y = gameplayZoom;
    yield return MoveCamera(launchPos, gameplayZoom, returnDuration);

  }

  private IEnumerator MoveCamera(Vector3 targetPos, float targetZoom, float duration)
  {
    Vector3 startPos = cameraTarget.position;
    startPos.z = 0f;
    targetPos.z = 0f;

    // Read from Cinemachine lens, not mainCamera
    float startZoom = cinemachineCamera.Lens.OrthographicSize;
    float elapsed = 0f;

    while (elapsed < duration)
    {
      elapsed += Time.deltaTime;
      float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

      cameraTarget.position = Vector3.Lerp(startPos, targetPos, t);
      cinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(startZoom, targetZoom, t); // Fix

      yield return null;
    }

    cameraTarget.position = targetPos;
    cinemachineCamera.Lens.OrthographicSize = targetZoom;

    Debug.Log($"Camera moved to {targetPos} with zoom {targetZoom}");
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
}
