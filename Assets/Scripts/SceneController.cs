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
    GameManager.Instance.GameStateController.SetState(GetStateForScene(sceneName));
    SceneManager.LoadScene(sceneName);
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
