using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : GSingleton<SoundManager>
{
    // DontDestroyOnLoad로 로드되는 사운드매니저에 아무생각없이 Dictionary에 지역을 이동할때마다 캐싱하게 된다면 메모리 누수가 생기기 때문에 Clear함수로 메모리를 관리해야한다.
    private AudioSource[] _audioSources = new AudioSource[(int)Sound.MaxCount];
    private Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();

    private GameObject _soundRoot = null;

    // 볼륨 값을 저장할 변수
    private float[] _volumeValues = new float[(int)Sound.MaxCount] { 1f, 1f, 1f };


     
    //! 오디오 소스를 담아둘 빈 게임 오브젝트를 생성해서 타입이름에 해당하는 오브젝트에 오디오소스를 넣어주는 코드
    protected override void Init()
    {
        if (_soundRoot == null)
        {
            _soundRoot = GameObject.Find("@SoundRoot");
            if (_soundRoot == null)
            {
                _soundRoot = new GameObject { name = "@SoundRoot" };
                UnityEngine.Object.DontDestroyOnLoad(_soundRoot);

                string[] SoundTypeNames = System.Enum.GetNames(typeof(Sound));
                for (int count = 0; count < SoundTypeNames.Length - 1; count++)
                {
                    GameObject go = new GameObject { name = SoundTypeNames[count] };
                    _audioSources[count] = go.AddComponent<AudioSource>();
                    go.transform.parent = _soundRoot.transform;
                }

                _audioSources[(int)Sound.Bgm].loop = true;
            }
        }
        for (int i = 0; i < (int)Sound.MaxCount; i++)
        {
            SetVolume((Sound)i, GameManager.Instance._volumeValues[i]);
        }

    }

    //! 오디오 소스 딕셔너리 안에 들어있는 오디오 소스를 모두 초기화해서 비워준다.
    public override void Clear()
    { 
        foreach (AudioSource audioSource in _audioSources)
        { 
            audioSource.Stop(); 
        }
        _audioClips.Clear();
    }
    public void Stop(Sound type)
    {
        AudioSource audioSource = _audioSources[(int)type];
        audioSource.Stop();
    }
    public void Play(Sound type)
    {
        AudioSource audioSource = _audioSources[(int)type];
        audioSource.Play();
    }

    public void Play(Sound type, string key, float pitch = 1.0f)
    {

        AudioSource audioSource = _audioSources[(int)type];

        if (type == Sound.Bgm)
        {
            LoadAudioClip(key, (audioClip) =>
            {
                if (audioSource.isPlaying)
                { 
                    audioSource.Stop();
                }

                audioSource.volume = GameManager.Instance._volumeValues[(int)Sound.Bgm];
                audioSource.clip = audioClip;
                if (GameManager.Instance.BGMOn)
                { 
                    audioSource.Play();
                }
            });
        }
        else if (type == Sound.SFX)
        {
            LoadAudioClip(key, (audioClip) =>
            {
                audioSource.volume = GameManager.Instance._volumeValues[(int)Sound.SFX];
                audioSource.clip = audioClip;
                if (GameManager.Instance.SFXOn)
                    audioSource.PlayOneShot(audioClip);
            });
        }
        else
        {
            LoadAudioClip(key, (audioClip) =>
            {
                audioSource.volume = GameManager.Instance._volumeValues[(int)Sound.UI_SFX];
                audioSource.pitch = pitch;
                if (GameManager.Instance.UI_SFXOn)
                    audioSource.PlayOneShot(audioClip);
            });
        }
    }

    public void Play(Sound type, AudioClip audioClip, float pitch = 1.0f)
    {
        AudioSource audioSource = _audioSources[(int)type];

        if (type == Sound.Bgm)
        {
            if (audioSource.isPlaying)
            { 
                audioSource.Stop();
            }

            audioSource.clip = audioClip;
            if (GameManager.Instance.BGMOn)
            {
                audioSource.Play();
            }
        }
        else if (type == Sound.SFX)
        {
            audioSource.pitch = pitch;
            audioSource.volume = GameManager.Instance._volumeValues[(int)Sound.SFX];
            if (GameManager.Instance.UI_SFXOn)
            {
                audioSource.PlayOneShot(audioClip);
            }
        }
        else
        {
            audioSource.pitch = pitch;
            audioSource.volume = GameManager.Instance._volumeValues[(int)Sound.UI_SFX];
            if (GameManager.Instance.UI_SFXOn)
            {
                audioSource.PlayOneShot(audioClip);
            }
        }
    }

    private void LoadAudioClip(string key, Action<AudioClip> callback)
    {
        AudioClip audioClip = null;
        if (_audioClips.TryGetValue(key, out audioClip))
        {
            callback?.Invoke(audioClip);
            return;
        }

        ResourceManager.Instance.LoadAsync<AudioClip>(key, (audioClip) =>
        {
            if (!_audioClips.ContainsKey(key))
                _audioClips.Add(key, audioClip);
            callback?.Invoke(audioClip);
        });
    }

    // 각 오디오 타입별 볼륨을 설정하는 메소드
    public void SetVolume(Sound type, float volumeValues)
    {
        GameManager.Instance._volumeValues[(int)type] = volumeValues;
        _audioSources[(int)type].volume = GameManager.Instance._volumeValues[(int)type];
    }

}
