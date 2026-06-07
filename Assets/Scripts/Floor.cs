using UnityEngine;

public class Floor : MonoBehaviour
{
  void OnCollisionEnter2D(Collision2D collision)
  {
    PlayerController player = collision.gameObject.GetComponentInParent<PlayerController>();
    if (player == null) return;
    if (!GameManager.Instance.GameStateController.IsState(GameState.Playing)) return;

    GameManager.Instance.AudioController.PlaySFX(GameManager.Instance.PlayerHit);
    player.TriggerLost();
  }
}
