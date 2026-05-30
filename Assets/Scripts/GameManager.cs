using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public int SelectedLevel { get; set; }

    public SceneController SceneController { get; private set; }
    public GameStateController GameStateController { get; private set; }
    public AudioController AudioController { get; private set; }

    public void RegisterSceneController(SceneController sc) => SceneController = sc;
    public void RegisterGameStateController(GameStateController gsc) => GameStateController = gsc;
    public void RegisterAudioController(AudioController ac) => AudioController = ac;
}
