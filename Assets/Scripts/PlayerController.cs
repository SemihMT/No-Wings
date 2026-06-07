using UnityEngine;

public class PlayerController : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private Rigidbody2D rb;
  [SerializeField] private TrailRenderer trailRenderer;

  [Header("Settings")]
  [SerializeField] private float maxVelocity = 20f;

  [Header("Sprites")]
  [SerializeField] private SpriteRenderer spriteRenderer;
  [SerializeField] private Sprite idleSprite;
  [SerializeField] private Sprite fallingSprite;

  public Rigidbody2D Rb => rb;
  public Vector2 Velocity => rb.linearVelocity;
  public Vector2 Position => rb.position;

  public bool IsSimulating { get; private set; }

  public event System.Action OnPlayerLost;

  void Awake()
  {
    rb.simulated = false;
    if (spriteRenderer == null)
      spriteRenderer = GetComponentInChildren<SpriteRenderer>();

    if (trailRenderer == null)
      trailRenderer = GetComponentInChildren<TrailRenderer>();

    ApplyIdleSprite();
  }

  public void StartSimulation()
  {
    IsSimulating = true;
    rb.simulated = true;
    trailRenderer.enabled = true;
    ApplyFallingSprite();
  }

  public void StopSimulation()
  {
    IsSimulating = false;
    rb.simulated = false;
    rb.linearVelocity = Vector2.zero;
    trailRenderer.enabled = false;
    ApplyIdleSprite();
  }

  private void ApplyIdleSprite()
  {
    if (spriteRenderer == null || idleSprite == null) return;
    spriteRenderer.sprite = idleSprite;
    spriteRenderer.transform.localRotation = Quaternion.identity;
  }

  private void ApplyFallingSprite()
  {
    if (spriteRenderer == null || fallingSprite == null) return;
    spriteRenderer.sprite = fallingSprite;
  }

  private void UpdateSpriteDirection()
  {
    if (spriteRenderer == null) return;

    if (!IsSimulating || rb.linearVelocity.sqrMagnitude < 0.01f)
    {
      spriteRenderer.transform.localRotation = Quaternion.identity;
      return;
    }

    float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
    spriteRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
  }

  void FixedUpdate()
  {
    if (!IsSimulating) return;

    rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxVelocity);
    UpdateSpriteDirection();

    // Hook for future input (tilt or nudge)
    ApplyPlayerInput();
  }

  private void ApplyPlayerInput()
  {
  }

  public void ResetToPosition(Vector2 position)
  {
    StopSimulation();
    rb.position = position;
    rb.rotation = 0f;
    rb.linearVelocity = Vector2.zero;
    rb.angularVelocity = 0f;
    transform.SetPositionAndRotation(position, Quaternion.identity);
  }

  // Player is out of camera bounds, treat as lost
  public void OnBecameInvisibleCallback()
  {
    TriggerLost();
  }
  // Called by anything that detects player loss (floor, spikes, etc)
  public void TriggerLost()
  {
    if (!IsSimulating) return;
    OnPlayerLost?.Invoke();
  }
}
