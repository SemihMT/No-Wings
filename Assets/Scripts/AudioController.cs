using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioController : MonoBehaviour
{
  [Header("Audio Sources")]
  [SerializeField] private AudioSource musicSource;
  [SerializeField] private AudioSource sfxSource;

  void Awake()
  {
    GameManager.Instance.RegisterAudioController(this);
  }

  public void PlayMusic(AudioClip clip, bool loop = true)
  {
    if (musicSource.clip == clip) return;
    musicSource.clip = clip;
    musicSource.loop = loop;
    musicSource.Play();
  }

  public void StopMusic() => musicSource.Stop();

  public void SetMusicVolume(float volume) => musicSource.volume = Mathf.Clamp01(volume);

  public void PlaySFX(AudioClip clip) => sfxSource.PlayOneShot(clip);

  public void SetSFXVolume(float volume) => sfxSource.volume = Mathf.Clamp01(volume);
}
