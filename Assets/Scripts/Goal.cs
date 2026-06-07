using UnityEngine;

public class Goal : MonoBehaviour
{
  public event System.Action OnGoalReached;

  void OnTriggerEnter2D(Collider2D other)
  {
    Rigidbody2D rb = other.GetComponentInParent<Rigidbody2D>();
    if (rb == null) return;
    if (!GameManager.Instance.GameStateController.IsState(GameState.Playing)) return;

    OnGoalReached?.Invoke();
  }
}
