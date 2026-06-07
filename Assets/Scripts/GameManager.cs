using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : Singleton<GameManager>
{
    public int SelectedLevel { get; set; }
    public bool HasPlayedIntro { get; set; }

    [Header("AudioClips")]
    public AudioClip BackgroundMusic;
    public AudioClip PlayerHit;


    public SceneController SceneController { get; private set; }
    public GameStateController GameStateController { get; private set; }
    public AudioController AudioController { get; private set; }
    public LevelController LevelController { get; private set; }
    public void RegisterLevelController(LevelController lc) => LevelController = lc;
    public void RegisterSceneController(SceneController sc) => SceneController = sc;
    public void RegisterGameStateController(GameStateController gsc) => GameStateController = gsc;
    public void RegisterAudioController(AudioController ac) => AudioController = ac;
}
