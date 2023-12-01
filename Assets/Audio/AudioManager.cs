using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public static AudioManager instance;
    public AudioSource audioSource;
    public List<AudioClip> audioClips;

    void Start()
    {
        instance = this;
        Play(GetRandomTrack(AudioTypes.AMBIENCE));
        InvokeRepeating("BackgroundMusicGenerator", 0f, 10f);
    }

    public void Play(string trackName)
    {
        audioSource.clip = audioClips.Find(x => x.name == trackName);
        audioSource.Play();
    }

    public void PlayAudioCue(string searchCriteria)
    {
        if (string.IsNullOrEmpty(searchCriteria)) return;
        Play(GetRandomTrack((AudioTypes) Enum.Parse(typeof(AudioTypes), searchCriteria)));
    }

    public string GetRandomTrack(AudioTypes searchCriteria)
    {
        System.Random random = new System.Random();

        var filtered = audioClips.FindAll(x => x.name.Contains(searchCriteria.ToString()));

        int randomIndex = random.Next(filtered.Count);

        return filtered[randomIndex].name;
    }

    private void BackgroundMusicGenerator() 
    {
        if (!audioSource.isPlaying)
        {
            Play(GetRandomTrack(AudioTypes.AMBIENCE));
        }
    }
}

public enum AudioTypes
{ 
    AMBIENCE,
    SCARY,
    SUSPENSEFUL,
    EVIL,
    HEROIC,
    REST
}