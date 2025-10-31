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

    private int currentImportance = 0;
    public void PlaySoundByName(string name, int importance=0)
    {
        if(audioNames.Contains(name) == false)
        {
            Debug.LogWarning("AudioManager: Sound name '" + name + "' not found!");
            return;
        }
        int index = audioNames.IndexOf(name);
        if (index >= 0 && index < listAudio.Count)
            PlaySFX(listAudio[index], importance);
    }

    public void PlaySFX(AudioClip audioClip, int importance = 0)
    {
        sfxAudioSource.PlayOneShot(audioClip);
        //if (sfxAudioSource.isPlaying)
        //    sfxAudioSource.Stop();
        //if(importance >= currentImportance)
        //    sfxAudioSource.PlayOneShot(audioClip);
        //currentImportance = importance;
    }

    public void StopSFX(string byName="")
    {
        if (byName!="" && audioNames.Contains(byName) && sfxAudioSource.isPlaying)
        {
            int index = audioNames.IndexOf(byName);
            if (sfxAudioSource.clip == listAudio[index])
            {
                sfxAudioSource.Stop();
            }
        }
        else
            sfxAudioSource.Stop();
    }
}