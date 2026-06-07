using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
  public const string MainMenu = "MainMenu";
  public const string LevelSelect = "LevelSelect";
  public const string Game = "Game";
  public const string Settings = "Settings";

  void Awake()
  {
    GameManager.Instance.RegisterSceneController(this);
  }

  public void LoadMainMenu() => LoadScene(MainMenu);
  public void LoadLevelSelect() => LoadScene(LevelSelect);
  public void LoadGame() => LoadScene(Game);
  public void LoadSettings() => LoadScene(Settings);

  private void LoadScene(string sceneName)
  {
    Debug.Log($"Loading scene: {sceneName}");
    GameManager.Instance.GameStateController.SetState(GetStateForScene(sceneName));
    Debug.Log($"Set game state to: {GameManager.Instance.GameStateController.CurrentState}");
    SceneManager.LoadScene(sceneName);
  }
  public void RetryGame()
  {
    GameManager.Instance.LevelController.RetryLevel();
  }

  private GameState GetStateForScene(string sceneName)
  {
    return sceneName switch
    {
      MainMenu => GameState.MainMenu,
      LevelSelect => GameState.LevelSelect,
      Settings => GameState.Settings,
      Game => GameState.LevelEditing,
      _ => GameState.MainMenu
    };
  }
}
