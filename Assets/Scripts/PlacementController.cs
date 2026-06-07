using System.Collections.Generic;
using UnityEngine;

public class PlacementController : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private Camera mainCamera;
  [SerializeField] private RectTransform elementPanel;

  [Header("Settings")]
  [SerializeField] private float overlapRadius = 0.4f;

  private Dictionary<ElementType, int> budget = new Dictionary<ElementType, int>();
  private Dictionary<ElementType, int> remaining = new Dictionary<ElementType, int>();
  private List<PlaceableElement> placedElements = new List<PlaceableElement>();

  private PlaceableElement ghostElement;
  private ElementData activeDragData;
  private PlaceableElement heldElement;
  private Vector3 heldElementOriginalPosition;

  public event System.Action<ElementType, int> OnBudgetChanged;

  public void SetBudget(ElementType type, int count)
  {
    budget[type] = count;
    remaining[type] = count;
    OnBudgetChanged?.Invoke(type, count);
  }

  public int GetRemaining(ElementType type)
  {
    return remaining.TryGetValue(type, out int count) ? count : 0;
  }

  public void BeginDragFromPanel(ElementData data)
  {
    if (GetRemaining(data.type) <= 0)
    {
      Debug.Log("No remaining elements of type: " + data.type);
      return;
    }

    activeDragData = data;
    ghostElement = Instantiate(data.prefab).GetComponent<PlaceableElement>();
    ghostElement.Initialize(data.type, this);
    ghostElement.SetGhost(true);
  }

  public void UpdateDrag(Vector2 screenPosition)
  {
    if (ghostElement == null) return;

    Vector3 worldPos = ScreenToWorld(screenPosition);
    ghostElement.transform.position = worldPos;
  }

  public void EndDrag(Vector2 screenPosition)
  {
    if (ghostElement == null) return;

    Vector3 worldPos = ScreenToWorld(screenPosition);

    if (IsOverPanel(screenPosition) || HasOverlapAt(worldPos, ghostElement))
    {
      Destroy(ghostElement.gameObject);
    }
    else
    {
      ConfirmPlacement(worldPos);
    }

    ghostElement = null;
    activeDragData = null;
  }

  public void BeginMove(PlaceableElement element)
  {
    heldElement = element;
    heldElementOriginalPosition = element.transform.position;
    element.SetGhost(true);
  }

  public void UpdateMove(Vector2 screenPosition)
  {
    if (heldElement == null) return;

    Vector3 worldPos = ScreenToWorld(screenPosition);
    heldElement.transform.position = worldPos;
  }

  public void EndMove(Vector2 screenPosition)
  {
    if (heldElement == null) return;

    if (IsOverPanel(screenPosition))
    {
      DeleteElement(heldElement);
    }
    else
    {
      Vector3 worldPos = ScreenToWorld(screenPosition);

      if (HasOverlapAt(worldPos, heldElement))
      {
        heldElement.transform.position = heldElementOriginalPosition;
        heldElement.SetGhost(false);
      }
      else
      {
        heldElement.transform.position = worldPos;
        heldElement.SetGhost(false);
      }
    }

    heldElement = null;
  }

  private void ConfirmPlacement(Vector3 worldPos)
  {
    ghostElement.transform.position = worldPos;
    ghostElement.SetGhost(false);
    placedElements.Add(ghostElement);

    remaining[activeDragData.type]--;
    OnBudgetChanged?.Invoke(activeDragData.type, remaining[activeDragData.type]);
  }

  private void DeleteElement(PlaceableElement element)
  {
    placedElements.Remove(element);
    remaining[element.ElementType]++;
    OnBudgetChanged?.Invoke(element.ElementType, remaining[element.ElementType]);
    Destroy(element.gameObject);
  }

  private bool HasOverlapAt(Vector3 worldPos, PlaceableElement ignore = null)
  {
    int layer = LayerMask.GetMask("PlaceableElement");
    Collider2D[] hits = Physics2D.OverlapCircleAll(worldPos, overlapRadius, layer);
    foreach (var hit in hits)
    {
      PlaceableElement el = hit.GetComponentInParent<PlaceableElement>();
      if (el != null && el != ignore) return true;
    }
    return false;
  }

  private bool IsOverPanel(Vector2 screenPosition)
  {
    return RectTransformUtility.RectangleContainsScreenPoint(elementPanel, screenPosition);
  }

  private Vector3 ScreenToWorld(Vector2 screenPosition)
  {
    Vector3 pos = mainCamera.ScreenToWorldPoint(screenPosition);
    pos.z = 0;
    return pos;
  }

  public List<PlaceableElement> GetPlacedElements() => placedElements;

  public void ClearAll()
  {
    foreach (var element in placedElements)
      if (element != null) Destroy(element.gameObject);

    placedElements.Clear();

    foreach (var key in new List<ElementType>(budget.Keys))
      remaining[key] = budget[key];
  }
}
