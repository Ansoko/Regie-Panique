using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxAudioSource;

    [Header("Sound References")]
    public List<AudioClip> listAudio;
    public List<string> audioNames;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayMusic(AudioClip audioClip)
    {
        if (musicSource.isPlaying) musicSource.Stop();
        musicSource.PlayOneShot(audioClip);
    }

    public void PlaySoundByName(string name)
    {
        if(audioNames.Contains(name) == false)
        {
            Debug.LogWarning("AudioManager: Sound name '" + name + "' not found!");
            return;
        }
        int index = audioNames.IndexOf(name);
        if (index >= 0 && index < listAudio.Count)
            PlaySFX(listAudio[index]);
    }

    public void PlaySFX(AudioClip audioClip)
    {
        if (sfxAudioSource.isPlaying) sfxAudioSource.Stop();
        sfxAudioSource.PlayOneShot(audioClip);
    }

    public bool IsMusicPlaying()
    {
        return musicSource.isPlaying;
    }
}