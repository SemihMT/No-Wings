using UnityEngine;

public class PlayerController : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private Rigidbody2D rb;

  [Header("Settings")]
  [SerializeField] private float maxVelocity = 20f;

  public Rigidbody2D Rb => rb;
  public Vector2 Velocity => rb.linearVelocity;
  public Vector2 Position => rb.position;

  public bool IsSimulating { get; private set; }

  public event System.Action OnPlayerLost;

  void Awake()
  {
    rb.simulated = false;
  }

  public void StartSimulation()
  {
    IsSimulating = true;
    rb.simulated = true;
  }

  public void StopSimulation()
  {
    IsSimulating = false;
    rb.simulated = false;
    rb.linearVelocity = Vector2.zero;
  }

  void FixedUpdate()
  {
    if (!IsSimulating) return;

    rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxVelocity);

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
  }
  public void OnBecameInvisibleCallback()
  {
    if (!IsSimulating) return;
    Debug.Log("Player went out of bounds. Triggering loss condition.");
    OnPlayerLost?.Invoke();
  }
}
