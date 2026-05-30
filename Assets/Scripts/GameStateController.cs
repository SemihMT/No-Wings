using UnityEngine;

public enum GameState
{
  MainMenu,
  LevelSelect,
  Settings,
  LevelEditing,
  Playing,
  Paused,
  LevelComplete,
  GameOver
}

public class GameStateController : MonoBehaviour
{
  public GameState CurrentState { get; private set; }
  public GameState PreviousState { get; private set; }

  public event System.Action<GameState, GameState> OnStateChanged;

  void Awake()
  {
    GameManager.Instance.RegisterGameStateController(this);
    CurrentState = GameState.MainMenu;
  }

  public void SetState(GameState newState)
  {
    if (newState == CurrentState) return;

    PreviousState = CurrentState;
    CurrentState = newState;
    OnStateChanged?.Invoke(PreviousState, CurrentState);

    Time.timeScale = newState == GameState.Paused ? 0f : 1f;
  }

  public void ResumeFromPause()
  {
    if (CurrentState != GameState.Paused) return;
    SetState(PreviousState);
  }

  public bool IsState(GameState state) => CurrentState == state;
}
