using UnityEngine;

public class InputHandler : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private PlacementController placementController;
  [SerializeField] private Camera mainCamera;

  [Header("Settings")]
  [SerializeField] private float holdThreshold = 0.3f;

  private float touchDownTime;
  private bool isHeld;
  private PlaceableElement touchedElement;
  private bool wasPressed;

  void Update()
  {
    if (!GameManager.Instance.GameStateController.IsState(GameState.LevelEditing)) return;

#if UNITY_EDITOR
    HandleMouseInput();
#else
        HandleTouchInput();
#endif
  }

  private void HandleMouseInput()
  {
    Vector2 position = Input.mousePosition;
    bool isPressed = Input.GetMouseButton(0);

    if (Input.GetMouseButtonDown(0))
      OnInputBegan(position);
    else if (isPressed)
    {
      OnInputMoved(position);
      OnInputStationary(position);
    }
    else if (Input.GetMouseButtonUp(0))
      OnInputEnded(position);
  }

  private void HandleTouchInput()
  {
    if (Input.touchCount == 0) return;

    Touch touch = Input.GetTouch(0);
    Vector2 position = touch.position;

    switch (touch.phase)
    {
      case TouchPhase.Began:
        OnInputBegan(position);
        break;
      case TouchPhase.Moved:
        OnInputMoved(position);
        break;
      case TouchPhase.Stationary:
        OnInputStationary(position);
        break;
      case TouchPhase.Ended:
      case TouchPhase.Canceled:
        OnInputEnded(position);
        break;
    }
  }

  private void OnInputBegan(Vector2 position)
  {
    touchDownTime = Time.time;
    isHeld = false;
    touchedElement = GetElementAtScreenPosition(position);
    Debug.Log($"Input began, element found: {touchedElement?.gameObject.name ?? "null"}");
  }

  private void OnInputMoved(Vector2 position)
  {
    CheckForHold(position);
    placementController.UpdateDrag(position);

    if (isHeld)
      placementController.UpdateMove(position);
  }

  private void OnInputStationary(Vector2 position)
  {
    CheckForHold(position);
  }

  private void OnInputEnded(Vector2 position)
  {
    if (touchedElement != null && !isHeld)
      touchedElement.Rotate();

    placementController.EndDrag(position);

    if (isHeld)
      placementController.EndMove(position);

    touchedElement = null;
    isHeld = false;
  }

  private void CheckForHold(Vector2 position)
  {
    if (isHeld) return;
    if (touchedElement == null) return;
    if (!(Time.time - touchDownTime >= holdThreshold)) return;

    isHeld = true;
    placementController.BeginMove(touchedElement);
  }

  private PlaceableElement GetElementAtScreenPosition(Vector2 screenPosition)
  {
    Vector2 worldPos = mainCamera.ScreenToWorldPoint(screenPosition);
    int layer = LayerMask.GetMask("PlaceableElement");
    Collider2D hit = Physics2D.OverlapPoint(worldPos, layer);

    if (hit == null) return null;
    return hit.GetComponentInParent<PlaceableElement>();
  }
}
