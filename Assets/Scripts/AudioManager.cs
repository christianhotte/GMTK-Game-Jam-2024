using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string name;

    public AudioClip clip;

    public bool loop;

    internal AudioSource source;

}

public class AudioManager : MonoBehaviour
{
    public AudioMixerGroup bgmMixer, sfxMixer;
    public Sound[] sounds;

    private void Awake()
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.pitch = 1;
            s.source.loop = s.loop;

            //If the sound does not loop, treat as a sound effect and give it a SFX audio mixer
            if (!s.loop)
            {
                if (sfxMixer != null)
                    s.source.outputAudioMixerGroup = sfxMixer;
            }
            //If the sound loops, treat as background music and give it a BGM audio mixer
            else
            {
                if (bgmMixer != null)
                    s.source.outputAudioMixerGroup = bgmMixer;
            }
        }
    }

    public void Play(string name, float audioVol, float pitchMin = 1, float pitchMax = 1)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Play();
        s.source.volume = audioVol;
        s.source.pitch = UnityEngine.Random.Range(pitchMin, pitchMax);
    }

    public void PlayOneShot(string name, float audioVol, float pitchMin = 1, float pitchMax = 1)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.PlayOneShot(s.clip, audioVol);
        s.source.pitch = UnityEngine.Random.Range(pitchMin, pitchMax);
    }

    public bool IsPlaying(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        return s.source.isPlaying;
    }

    public void ChangeVolume(string name, float audioVol)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.volume = audioVol;
    }

    public void ChangePitch(string name, float audioPitch)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.pitch = audioPitch;
    }

    public void Pause(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Pause();
    }

    public void PauseAllSounds()
    {
        foreach (Sound s in sounds)
        {
            s.source.Pause();
        }
    }

    public void Resume(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.UnPause();
    }

    public void ResumeAllSounds()
    {
        foreach (Sound s in sounds)
        {
            s.source.UnPause();
        }
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Stop();
    }

    public void StopAllSounds()
    {
        foreach (Sound s in sounds)
        {
            s.source.Stop();
        }
    }
}
