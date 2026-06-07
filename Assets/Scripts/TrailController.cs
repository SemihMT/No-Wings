using UnityEngine;
using System.Collections.Generic;

public class TrailController : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private BirdTrailRenderer[] trailRenderers; // 3 elements

  [Header("Settings")]
  [SerializeField] private float recordInterval = 0.05f;
  [SerializeField] private float[] trailAlphas = { 0.2f, 0.4f, 0.8f }; // oldest to newest


  private PlayerController playerController;
  private List<TrailData> trails = new List<TrailData>();
  private TrailData currentTrail;
  private float timeSinceLastRecord;
  private bool isRecording;

  void Awake()
  {
    GameManager.Instance.GameStateController.OnStateChanged += HandleStateChanged;
  }

  void OnDestroy()
  {
    if (GameManager.Instance == null) return;
    GameManager.Instance.GameStateController.OnStateChanged -= HandleStateChanged;
  }

  private void HandleStateChanged(GameState previous, GameState next)
  {
    if (next == GameState.Playing)
      StartRecording();
    else if (previous == GameState.Playing)
      StopRecording();
  }

  private void StartRecording()
  {
    currentTrail = new TrailData();
    timeSinceLastRecord = 0f;
    isRecording = true;
  }

  private void StopRecording()
  {
    if (currentTrail == null) return;
    isRecording = false;

    // Add to trail history
    trails.Add(currentTrail);
    if (trails.Count > 3)
      trails.RemoveAt(0);

    currentTrail = null;
    RefreshDisplay();
  }

  void Update()
  {
    if (!isRecording) return;
    if (playerController == null) return;

    timeSinceLastRecord += Time.deltaTime;
    if (timeSinceLastRecord < recordInterval) return;

    timeSinceLastRecord = 0f;
    currentTrail.Points.Add(playerController.Position);
  }

  public void RecordDeath(Vector2 position)
  {
    if (currentTrail == null) return;
    currentTrail.DeathPosition = position;
    currentTrail.HasDied = true;
  }

  private void RefreshDisplay()
  {
    foreach (var renderer in trailRenderers)
      renderer.Hide();

    int trailStart = Mathf.Max(0, trails.Count - 3);
    int count = trails.Count - trailStart;

    // Offset into alphas array so newest always gets highest alpha
    int alphaOffset = 3 - count;

    int rendererIndex = 0;
    for (int i = trailStart; i < trails.Count; i++)
    {
      float alpha = trailAlphas[alphaOffset + rendererIndex];
      trailRenderers[rendererIndex].Display(trails[i], alpha);
      rendererIndex++;
    }
  }

  public void ClearTrails()
  {
    trails.Clear();
    foreach (var renderer in trailRenderers)
      renderer.Hide();
  }

  public void SetPlayerController(PlayerController controller)
  {
    playerController = controller;
  }
}
