using System;
using System.Collections.Generic;
using Audio;
using FMOD.Studio;
using UnityEngine;
using FMODUnity;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class AudioManager : MonoBehaviour
{
    #region Serialized Fields

    [Header("Master Bank")]
    [SerializeField] private AudioBank _bank;

    [Header("Volume")] 
    [SerializeField][Range(0f, 1f)] private float _masterVolume = 1;
    [SerializeField][Range(0f, 1f)] private float _musicVolume = 1;
    [SerializeField][Range(0f, 1f)] private float _sfxVolume = 1;

    #endregion

    #region Non-Serialized Fields

    private static AudioManager _instance;

    private List<EventInstance> _instances = new();
    private EventInstance _musicEventInstance;
        
    private static readonly string _musicEventName = "Section";

    private Bus _masterBus;
    private Bus _musicBus;
    private Bus _sfxBus;

    #endregion

    #region Properties

    public static string PrecisionParameter { get; private set; } = "PrecisionMP";

    #endregion

    #region Function Events

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        
        transform.SetParent(null);
        DontDestroyOnLoad(_instance.gameObject);
        
        InitializeMusicEventInstance(_bank.MusicEventReference);

        _masterBus = RuntimeManager.GetBus("bus:/");
        _musicBus = RuntimeManager.GetBus("bus:/Music");
        _sfxBus = RuntimeManager.GetBus("bus:/SFX");
    }
    
    private void OnDestroy()
    {
        CleanUp();
    }

    #endregion

    #region Public Methods

    public static void PlayOneShot(SoundType type, int val)
    {
        try
        {
            RuntimeManager.PlayOneShot(_instance._bank[type, val]);
        }
        catch (ArgumentOutOfRangeException)
        {
            print($"Invalid sound {type}:{val}");
        }
    }

    private void Update()
    {
        _masterBus.setVolume(_masterVolume);
        _musicBus.setVolume(_musicVolume);
        _sfxBus.setVolume(_sfxVolume);
    }

    public static EventInstance CreateEventInstance(SoundType type, int val)
    {
        try
        {
            return _instance.CreateEventInstance_Inner(_instance._bank[type, val]);
        }
        catch (ArgumentOutOfRangeException)
        {
            print($"Invalid sound {type}:{val}");
        }

        return new EventInstance();
    }

    public static void SetMusic(MusicSounds music)
    {
        _instance._musicEventInstance.setParameterByName(_musicEventName, (float) music);
    }

    #endregion

    #region Private Methods

    private void InitializeMusicEventInstance(EventReference reference)
    {
        _musicEventInstance = CreateEventInstance_Inner(reference);
        _musicEventInstance.start();
    }

    private EventInstance CreateEventInstance_Inner(EventReference reference)
    {
        EventInstance instance = RuntimeManager.CreateInstance(reference);
        _instances.Add(instance);
        return instance;
    }
    
    private void CleanUp()
    {
        foreach (var instance in _instances)
        {
            instance.stop(STOP_MODE.IMMEDIATE);
            instance.release();
        }
    }
    
    #endregion
}































