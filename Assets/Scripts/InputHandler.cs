using UnityEngine;

public class InputHandler : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private PlacementController placementController;
  [SerializeField] private CameraController cameraController;
  [SerializeField] private Camera mainCamera;

  [Header("Settings")]
  [SerializeField] private float holdThreshold = 0.3f;

  private float touchDownTime;
  private bool isHeld;
  private bool isDraggingCamera;
  private PlaceableElement touchedElement;
  private Vector2 lastInputPosition;
  private float lastPinchDistance;

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

    // Scroll to zoom
    float scroll = Input.GetAxis("Mouse ScrollWheel");
    if (Mathf.Abs(scroll) > 0.01f)
      cameraController.Zoom(-scroll * 5f);

    if (Input.GetMouseButtonDown(0))
      OnInputBegan(position);
    else if (Input.GetMouseButton(0))
      OnInputMoved(position);
    else if (Input.GetMouseButtonUp(0))
      OnInputEnded(position);
  }
  private void HandleTouchInput()
  {
    // Pinch to zoom
    if (Input.touchCount == 2)
    {
      HandlePinchZoom();
      return;
    }

    if (Input.touchCount == 0) return;

    Touch touch = Input.GetTouch(0);

    switch (touch.phase)
    {
      case TouchPhase.Began:
        OnInputBegan(touch.position);
        break;
      case TouchPhase.Moved:
        OnInputMoved(touch.position);
        break;
      case TouchPhase.Stationary:
        OnInputStationary(touch.position);
        break;
      case TouchPhase.Ended:
      case TouchPhase.Canceled:
        OnInputEnded(touch.position);
        break;
    }
  }

  private void HandlePinchZoom()
  {
    Touch t0 = Input.GetTouch(0);
    Touch t1 = Input.GetTouch(1);

    float currentDistance = Vector2.Distance(t0.position, t1.position);

    if (t1.phase == TouchPhase.Began)
    {
      lastPinchDistance = currentDistance;
      return;
    }

    float delta = lastPinchDistance - currentDistance;
    cameraController.Zoom(delta * 0.05f);
    lastPinchDistance = currentDistance;
  }

  private void OnInputBegan(Vector2 position)
  {
    touchDownTime = Time.time;
    isHeld = false;
    isDraggingCamera = false;
    lastInputPosition = position;

    // UI check first — if over panel, ElementIcon handles it
    if (IsOverUI(position)) return;

    touchedElement = GetElementAtScreenPosition(position);

    // Nothing under finger — camera drag
    if (touchedElement == null)
      isDraggingCamera = true;
  }

  private void OnInputMoved(Vector2 position)
  {
    if (isDraggingCamera)
    {
      Vector2 delta = GetWorldDelta(position, lastInputPosition);
      cameraController.DragCamera(delta);
      lastInputPosition = position;
      return;
    }

    CheckForHold(position);
    placementController.UpdateDrag(position);

    if (isHeld)
      placementController.UpdateMove(position);

    lastInputPosition = position;
  }

  private void OnInputStationary(Vector2 position)
  {
    if (isDraggingCamera) return;
    CheckForHold(position);
  }

  private void OnInputEnded(Vector2 position)
  {
    if (isDraggingCamera)
    {
      isDraggingCamera = false;
      return;
    }

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

  private Vector2 GetWorldDelta(Vector2 current, Vector2 previous)
  {
    Vector3 currentWorld = mainCamera.ScreenToWorldPoint(current);
    Vector3 previousWorld = mainCamera.ScreenToWorldPoint(previous);
    return previousWorld - currentWorld;
  }

  private bool IsOverUI(Vector2 screenPosition)
  {
    return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
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
