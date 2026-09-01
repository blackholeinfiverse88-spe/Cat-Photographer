using UnityEngine;

public class MainMenuAudio : MonoBehaviour
{
    [Header("AUDIO SOURCES")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("SOUND CLIPS")]
    public AudioClip backgroundMusic;
    public AudioClip buttonClickSound;
    public AudioClip cameraButtonSound;

    private void Start()
    {
        // Start main menu music
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    // Normal buttons
    public void PlayButtonClick()
    {
        if (sfxSource != null && buttonClickSound != null)
        {
            sfxSource.PlayOneShot(buttonClickSound);
        }
    }

    // Camera button
    public void PlayCameraButtonSound()
    {
        if (sfxSource != null && cameraButtonSound != null)
        {
            sfxSource.PlayOneShot(cameraButtonSound);
        }
    }
}