using UnityEngine;

public class AudioController_sc : MonoBehaviour
{
    public static AudioController_sc Instance;

    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float soundEffectVolume = 1f;

    public AudioSource musicSource;
    public AudioSource soundEffectSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume;

        if (soundEffectSource != null)
            soundEffectSource.volume = soundEffectVolume;
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource == null || clip == null) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }


    public void PlaySfx(AudioClip clip)
    {
        if (soundEffectSource != null && clip != null)
            soundEffectSource.PlayOneShot(clip, soundEffectVolume);
    }
}
