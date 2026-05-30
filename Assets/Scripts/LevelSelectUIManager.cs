using UnityEngine;
using UnityEngine.UI;

public class LevelSelectUIManager : MonoBehaviour
{
  [Header("Canvases")]
  [SerializeField] private Canvas levelSelectCanvas;

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
    levelSelectCanvas.enabled = next == GameState.LevelSelect;
  }

  // Called by each level button, pass the level index from the inspector
  public void OnLevelSelected(int levelIndex)
  {
    GameManager.Instance.SelectedLevel = levelIndex;
    GameManager.Instance.SceneController.LoadGame();
  }

  public void OnBackPressed() =>
      GameManager.Instance.SceneController.LoadMainMenu();
}
