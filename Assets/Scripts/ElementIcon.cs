using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ElementIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
  [SerializeField] private ElementData elementData;
  [SerializeField] private PlacementController placementController;

  private Image image;
  void Awake()
  {
    image = GetComponent<Image>();
    if (elementData != null && elementData.icon != null)
      image.sprite = elementData.icon;
  }
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
