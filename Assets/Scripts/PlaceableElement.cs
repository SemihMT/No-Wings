using UnityEngine;

public class PlaceableElement : MonoBehaviour
{
  private static readonly Vector3[] Directions = new Vector3[]
  {
        Vector3.up,
        new Vector3(1, 1, 0).normalized,
        Vector3.right,
        new Vector3(1, -1, 0).normalized,
        Vector3.down,
        new Vector3(-1, -1, 0).normalized,
        Vector3.left,
        new Vector3(-1, 1, 0).normalized
  };

  [Header("Settings")]
  [SerializeField] private float overlapRadius = 0.4f;

  public ElementType ElementType { get; private set; }
  public Vector3 FacingDirection { get; private set; } = Vector3.up;

  private int directionIndex = 0;
  private PlacementController placementController;

  public void Initialize(ElementType type, PlacementController controller)
  {
    ElementType = type;
    placementController = controller;
  }

  public void Rotate()
  {
    directionIndex = (directionIndex + 1) % Directions.Length;
    FacingDirection = Directions[directionIndex];
    transform.rotation = Quaternion.FromToRotation(Vector3.up, FacingDirection);
    OnRotated();
  }

  protected virtual void OnRotated() { }

  public bool HasOverlap()
  {
    return Physics2D.OverlapCircle(transform.position, overlapRadius);
  }

  public void SetGhost(bool isGhost)
  {
    var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    if (spriteRenderer == null) return;

    Color c = spriteRenderer.color;
    c.a = isGhost ? 0.5f : 1f;
    spriteRenderer.color = c;
  }
}
