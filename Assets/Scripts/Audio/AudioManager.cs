using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Audio;
using FMOD.Studio;
using UnityEngine;
using FMODUnity;
using Managers;
using Unity.VisualScripting;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class AudioManager : MonoBehaviour
{
    #region Serialized Fields

    [Header("Master Bank")]
    [SerializeField] private AudioBank _bank;

    [Header("Sounds")] 
    [SerializeField] private EventReference _defaultDash;
    [SerializeField] private MusicSounds _defaultMusic;

    [Header("Volume")] 
    [SerializeField][Range(0f, 1f)] private float _masterVolume = 1;
    [SerializeField][Range(0f, 1f)] private float _musicVolume = 1;
    [SerializeField][Range(0f, 1f)] private float _sfxVolume = 1;

    #endregion

    #region Non-Serialized Fields

    private static AudioManager _instance;
    private HashSet<IOnBeatListener> _beatListeners = new();

    private List<EventInstance> _instances = new();
    private EventInstance _musicEventInstance;
        
    private static readonly string _musicEventName = "Section";

    private Bus _masterBus;
    private Bus _musicBus;
    private Bus _sfxBus;

    private EVENT_CALLBACK cb;

    private float _lastBeat;
    private EventReference _dashEvent;

    #endregion

    #region Properties

    public static float Tempo { get; private set; }
    public static float TimeFactor { get; private set; } = 0;

    public static EventReference DashEvent
    {
        get => _instance._dashEvent;
        set => _instance._dashEvent = value.IsNull ? _instance._defaultDash : value;
    }

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

        DashEvent = _defaultDash;
        SetMusic(MusicSounds.Lobby);
        
        transform.SetParent(null);
        //DontDestroyOnLoad(_instance.gameObject);
        
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

    public static void RegisterBeatListener(IOnBeatListener l)
    {
        if(_instance._beatListeners.Contains(l))
            return;
        _instance._beatListeners.Add(l);
    }
    
    public static void UnRegisterBeatListener(IOnBeatListener l)
    {
        if(!_instance._beatListeners.Contains(l))
            return;
        _instance._beatListeners.Remove(l);
    }

    public static void PlayNoise()
    {
        RuntimeManager.PlayOneShot(_instance._bank[SoundType.Sfx, (int)SfxSounds.Noise]);
    }

    public static void Transition(MusicSounds to)
    {
        PlayNoise();
        SetMusic(to);
    }

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

    public static void PlayDash()
    {
        RuntimeManager.PlayOneShot(_instance._dashEvent);
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

    public static void SetMusic( MusicSounds music)
    {
        _instance._musicEventInstance.setParameterByName(_musicEventName, (float) music);
    }

    #endregion

    #region Private Methods

    private void InitializeMusicEventInstance(EventReference reference)
    {
        _musicEventInstance = CreateEventInstance_Inner(reference);
        cb = new FMOD.Studio.EVENT_CALLBACK(OnBeatCallback);
        _musicEventInstance.setCallback(cb, EVENT_CALLBACK_TYPE.NESTED_TIMELINE_BEAT);
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

        _musicEventInstance.stop(STOP_MODE.IMMEDIATE);
        _musicEventInstance.release();
    }
    
    public FMOD.RESULT OnBeatCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr eventInstance, IntPtr parameters)
    {
        if (type == EVENT_CALLBACK_TYPE.NESTED_TIMELINE_BEAT)
        {
            TIMELINE_BEAT_PROPERTIES beat = ((TIMELINE_NESTED_BEAT_PROPERTIES)Marshal.PtrToStructure(parameters, typeof(TIMELINE_NESTED_BEAT_PROPERTIES))).properties;
            Tempo = beat.tempo;

            if (_lastBeat == 0)
            {
                _lastBeat = Time.time;
            }
            else if(TimeFactor == 0)
            {
                float delta = Time.time - _lastBeat;
                TimeFactor = delta / (Tempo / 60);
            }
            else
            {
                foreach (var l in _beatListeners)
                {
                    l.OnBeat(beat.beat);
                }
            }
        }
        return FMOD.RESULT.OK;
    }

    #endregion
}































