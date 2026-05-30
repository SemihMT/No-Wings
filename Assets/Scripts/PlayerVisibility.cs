using UnityEngine;

public class PlayerVisibility : MonoBehaviour
{
  private PlayerController playerController;

  void Awake()
  {
    playerController = GetComponentInParent<PlayerController>();
  }

  void OnBecameInvisible()
  {
    playerController.OnBecameInvisibleCallback();
  }
}
