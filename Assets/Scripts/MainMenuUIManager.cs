using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{
  [Header("Canvases")]
  [SerializeField] private Canvas mainMenuCanvas;
  [SerializeField] private Canvas settingsCanvas;

  private Canvas[] allCanvases;

  void Awake()
  {
    allCanvases = new Canvas[] { mainMenuCanvas, settingsCanvas };
  }

  void Start()
  {
    GameManager.Instance.GameStateController.OnStateChanged += HandleStateChanged;
    HandleStateChanged(GameState.MainMenu, GameManager.Instance.GameStateController.CurrentState);
  }

  void OnDisable()
  {
    if (GameManager.Instance == null) return;
    GameManager.Instance.GameStateController.OnStateChanged -= HandleStateChanged;
  }

  private void HandleStateChanged(GameState previous, GameState next)
  {
    HideAll();

    switch (next)
    {
      case GameState.MainMenu:
        Show(mainMenuCanvas);
        break;
      case GameState.Settings:
        Show(settingsCanvas);
        break;
    }
  }

  // MainMenu buttons
  public void OnPlayPressed() =>
      GameManager.Instance.SceneController.LoadLevelSelect();
  public void OnSettingsPressed() =>
      GameManager.Instance.GameStateController.SetState(GameState.Settings);

  // Settings buttons
  public void OnSettingsBackPressed() =>
      GameManager.Instance.GameStateController.SetState(GameState.MainMenu);

  private void Show(Canvas canvas)
  {
    if (canvas != null) canvas.enabled = true;
  }

  private void HideAll()
  {
    foreach (var canvas in allCanvases)
      if (canvas != null) canvas.enabled = false;
  }
}
