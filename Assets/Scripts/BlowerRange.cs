using UnityEngine;

public class BlowerRange : MonoBehaviour
{
  private BlowerElement blowerElement;

  void Awake()
  {
    blowerElement = GetComponentInParent<BlowerElement>();
  }

  void OnTriggerEnter2D(Collider2D other)
  {
    Rigidbody2D rb = other.GetComponentInParent<Rigidbody2D>();
    if (rb != null) blowerElement.SetBirdRb(rb);
  }

  void OnTriggerExit2D(Collider2D other)
  {
    Rigidbody2D rb = other.GetComponentInParent<Rigidbody2D>();
    if (rb != null) blowerElement.ClearBirdRb(rb);
  }
}
