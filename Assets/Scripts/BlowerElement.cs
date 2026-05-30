using UnityEngine;

public class BlowerElement : PlaceableElement
{
  [Header("Blower Settings")]
  [SerializeField] private float blowForce = 5f;
  [SerializeField] private float range = 2f;

  public Vector3 BlowDirection => FacingDirection;
  public float BlowForce => blowForce;

  private Rigidbody2D birdRb;

  protected override void OnRotated() { }

  public void SetBirdRb(Rigidbody2D rb) => birdRb = rb;

  public void ClearBirdRb(Rigidbody2D rb)
  {
    if (birdRb == rb) birdRb = null;
  }

  void FixedUpdate()
  {
    if (birdRb == null) return;
    if (!GameManager.Instance.GameStateController.IsState(GameState.Playing)) return;
    birdRb.AddForce(BlowDirection * blowForce, ForceMode2D.Force);
  }

  void OnValidate()
  {
    CircleCollider2D[] colliders = GetComponentsInChildren<CircleCollider2D>();
    foreach (var col in colliders)
      if (col.isTrigger) col.radius = range;
  }
}
