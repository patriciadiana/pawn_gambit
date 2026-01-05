using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum SoundType
{
    MOVEPIECE,
    ATTACKPIECE
}

public enum MusicType
{
    BACKGROUNDMUSIC,
    QUEENTHEME,
    KINGTHEME,
    ROOKTHEME,
    BISHOPTHEME,
    KNIGHTTHEME
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    private AudioClip pausedClip;
    private float pausedTime;

    [Header("---------- Audio Source ----------")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [SerializeField] private AudioClip[] soundList;
    [SerializeField] private AudioClip[] musicList;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        PlayMusic(MusicType.BACKGROUNDMUSIC, 0.1f);
    }

    public static void PlaySound(SoundType sound, float volume)
    {
        if (Instance.soundList.Length > (int)sound)
        {
            Instance.sfxSource.PlayOneShot(Instance.soundList[(int)sound], volume);
        }
        else
        {
            Debug.LogWarning("Sound clip not found for: " + sound);
        }
    }

    public static void PlayMusic(MusicType music, float volume)
    {
        if (Instance.musicList.Length > (int)music)
        {
            Instance.musicSource.clip = Instance.musicList[(int)music];
            Instance.musicSource.volume = volume;
            Instance.musicSource.loop = true;
            Instance.musicSource.Play();
        }
        else
        {
            Debug.LogWarning("Music clip not found for index: " + music);
        }
    }

    public void PauseMusic()
    {
        if (musicSource.isPlaying)
        {
            pausedClip = musicSource.clip;      
            pausedTime = musicSource.time;      
            musicSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (pausedClip != null)
        {
            musicSource.clip = pausedClip;      
            musicSource.time = pausedTime;      
            musicSource.Play();
            pausedClip = null;                  
        }
    }
}