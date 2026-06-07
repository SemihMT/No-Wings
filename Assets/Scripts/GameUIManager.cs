using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
  [Header("Canvases")]
  [SerializeField] private Canvas levelEditingCanvas;
  [SerializeField] private Canvas playingCanvas;
  [SerializeField] private Canvas pauseCanvas;
  [SerializeField] private Canvas levelCompleteCanvas;
  [SerializeField] private Canvas gameOverCanvas;

  [SerializeField] private LevelController levelController;

  private Canvas[] allCanvases;

  void Awake()
  {
    allCanvases = new Canvas[]
    {
            levelEditingCanvas,
            playingCanvas,
            pauseCanvas,
            levelCompleteCanvas,
            gameOverCanvas
    };
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
      case GameState.LevelEditing:
        Show(levelEditingCanvas);
        break;
      case GameState.Intro:
        HideAll();
        break;
      case GameState.Playing:
        Show(playingCanvas);
        break;
      case GameState.Paused:
        Show(pauseCanvas);
        break;
      case GameState.LevelComplete:
        Show(levelCompleteCanvas);
        break;
      case GameState.GameOver:
        Show(gameOverCanvas);
        break;
    }
  }

  // LevelEditing buttons
  public void OnSimulatePressed() => levelController.StartSimulation();
  public void OnClearPressed() => GameManager.Instance.LevelController.ClearElements();

  // Playing buttons
  public void OnPausePressed() =>
      GameManager.Instance.GameStateController.SetState(GameState.Paused);

  // Pause buttons
  public void OnResumePressed() =>
   GameManager.Instance.GameStateController.ResumeFromPause();
  public void OnPauseQuitPressed() =>
      GameManager.Instance.SceneController.LoadLevelSelect();

  // LevelComplete buttons
  public void OnNextLevelPressed()
  {
    GameManager.Instance.HasPlayedIntro = false;
    GameManager.Instance.SelectedLevel++;
    GameManager.Instance.SceneController.LoadGame();
  }
  public void OnLevelCompleteQuitPressed() =>
      GameManager.Instance.SceneController.LoadLevelSelect();

  // GameOver buttons
  public void OnRetryPressed() =>
      GameManager.Instance.SceneController.RetryGame();
  public void OnGameOverQuitPressed() =>
      GameManager.Instance.SceneController.LoadLevelSelect();

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
