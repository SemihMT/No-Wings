using UnityEngine;
using UnityEngine.EventSystems;

public class ElementIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
  [SerializeField] private ElementData elementData;
  [SerializeField] private PlacementController placementController;

  public void OnBeginDrag(PointerEventData eventData)
  {
    placementController.BeginDragFromPanel(elementData);
  }

  public void OnDrag(PointerEventData eventData)
  {
    placementController.UpdateDrag(eventData.position);
  }

  public void OnEndDrag(PointerEventData eventData)
  {
    placementController.EndDrag(eventData.position);
  }
}
