using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

[RequireComponent(typeof(MainSingleton))]
public class AudioManager : MonoBehaviour
{
    public AudioClip[] clips;
    public AudioClip[] sfx;
    public AudioSource sfxSource;
    public AudioSource musicSource;

    public event Action<string> OnMusicChanged;
    private void Start()
    {
        DialogueRunner runner = MainSingleton.Instance.dialogueRunner;
        runner.AddCommandHandler<string>("PlayMusic", PlayMusic);
        runner.AddCommandHandler<string, float>("PlaySoundEffect", PlaySoundEffect);
        runner.AddCommandHandler("StopMusic", StopMusic);
    }
    public void PlaySoundEffect(string soundName)
    {
        PlaySoundEffect(soundName, 1);
    }

    public void PlaySoundEffect(string soundName, float volume = 1)
    {
        var audioClip = FetchAudio(soundName, sfx);
        sfxSource.PlayOneShot(audioClip, volume);
    }

    /// <summary>
    /// PlayMusic
    /// Assumes loop is true
    /// </summary>
    public void PlayMusic(string soundName)
    {

        var audioClip = FetchAudio(soundName, clips);

        musicSource.Stop();
        musicSource.clip = audioClip;
        musicSource.loop = true;
        musicSource.Play();

        OnMusicChanged?.Invoke(soundName);
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    private AudioClip FetchAudio(string assetName, AudioClip[] search)
    {
        foreach (var ac in search)
        {
            if (ac.name == assetName)
            {
                return ac;
            }
        }
        return null;
    }
}
