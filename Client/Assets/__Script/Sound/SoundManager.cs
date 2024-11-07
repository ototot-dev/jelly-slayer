using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Game;

public class SoundManager : MonoSingleton<SoundManager>
{
	public Transform _trRoot;
	public Dictionary<SoundID, SoundData> _dicSoundData = new();
#if UNITY_EDITOR
	public List<SoundData> _listSoundData = new();
#endif

    public float _volumeSFX = 1;
    public float _volumeBGM = 1;
    public float _volumeVCE = 1;

	public SoundID _curBGM = 0;
    public SoundID _idClick = 0;
	public int _count = 0;

	public SoundObject Play(SoundID id, bool i_isLoop = false, float i_delay = 0) //, SoundInfo.TYPE i_soundType = SoundInfo.TYPE.SFX
	{
        SoundData data = GetData(id);
#if UNITY_EDITOR
        if (data == null)
        {
            //info = GetInfo(0);
        }
#endif
        if (data == null)
        {
			Debug.Log("SoundPlay Error 1 : " + id + ", " + i_isLoop);
			return null;
        }
		SoundObject sound = data.Create();
        if (sound == null)
        {
			Debug.Log("SoundPlay Error 2 : " + id + ", " + i_isLoop);
			return null;
        }

		if (sound.transform.parent == null)
			sound.transform.SetParent(transform);

//		sound.Volume = (i_isSFX == true) ? _volumeSFX : _volumeBGM;
		switch (data._info.type)
		{
			case SoundType.SFX: sound.Volume = _volumeSFX; break;
			case SoundType.BGM: sound.Volume = _volumeBGM; break;
			case SoundType.VCE: sound.Volume = _volumeVCE; break;
			default:
				Debug.Log("Sound type is not correct!");
				return null;
		}
		sound.SetLoop(i_isLoop);

		if (i_delay > 0)
		{
			sound.Invoke("Play", i_delay);
		}
		else
		{
			sound.Play();
		}
#if UNITY_EDITOR
        _count = transform.childCount;
#endif		
		return sound;
	}
    public SoundObject PlayWithPath(string i_resPath, bool i_isLoop = false, bool i_isSFX = true, float i_volumeRate = 1.0f)
    {
        //Debug.Log("SoundPlay : " + i_sfxType + ", " + i_isLoop);
        SoundObject sound = CreateSoundObject();

        if (sound._trRoot.parent == null)
            sound._trRoot.SetParent(_trRoot);

        float volume = (i_isSFX == true) ? _volumeSFX : _volumeBGM;
        volume *= i_volumeRate;
        sound.Volume = volume;
        sound.SetLoop(i_isLoop);
        sound._lifeTime = 20.0f;
        sound.PlayWithPath(i_resPath);

#if UNITY_EDITOR
        _count = transform.childCount;
#endif
        return sound;
    }
    public SoundObject PlayWithClip(AudioClip i_clip, bool i_isLoop = false, bool i_isSFX = true, float i_volumeRate = 1.0f)
    {
        //Debug.Log("SoundPlay : " + i_sfxType + ", " + i_isLoop);
        SoundObject sound = CreateSoundObject();

        if (sound._trRoot.parent == null)
            sound._trRoot.SetParent(_trRoot);

        float volume = (i_isSFX == true) ? _volumeSFX : _volumeBGM;
        volume *= i_volumeRate;
        sound.Volume = volume;
        sound.SetLoop(i_isLoop);
        sound._lifeTime = 20.0f;
        sound.PlayWithClip(i_clip);

#if UNITY_EDITOR
        _count = transform.childCount;
#endif
        return sound;
    }
    public static SoundObject CreateSoundObject() 
    {
        GameObject prefObj = Resources.Load("Prefabs/SoundObject") as GameObject;
        GameObject obj = GameObject.Instantiate(prefObj);

        return obj.GetComponent<SoundObject>();
    }
	public void PlayClickSound()
	{
		//Debug.Log("PlayClickSound");
		Play(_idClick, false);
	}
	public SoundObject PlayBGM(SoundID i_id)
	{
		if (_curBGM == i_id)
			return null;

		// 현재 Play 중인 BGM 이 있다면... Fade Out
		if (_curBGM >= 0)
		{
			SoundData data = GetData(_curBGM);
			data.FadeOut();
		}
		_curBGM = i_id;
		SoundObject sound = Play(i_id, true);
        if (sound != null)
        {
			sound.Volume = 0;
            sound.FadeIn(_volumeBGM);
        }
		return sound;
	}
    public void StopBGM()
    {
        if (_curBGM >= 0)
        {
            SoundData data = GetData(_curBGM);
            data.FadeOut();
        }
        _curBGM = SoundID.NONE;
    }
    public void FadeOut(SoundID i_id)
	{
		SoundData data = GetData(i_id);
        data.FadeOut();
	}
	public SoundData GetData(SoundID i_id)
	{
		if (_dicSoundData.ContainsKey(i_id) == false)
			return null;

		return _dicSoundData[i_id];
	}
	void Awake()
	{
		//_prefSound = Resources.Load("Prefabs/Effect/SoundObject") as GameObject;
		//LoadSoundInfo();

        if (_trRoot == null) 
        {
            _trRoot = transform;
        }
		//PlayBGM();
	}
	public void Init() 
	{
		//var list = DataTables.GetSoundInfoList();
		var list = MainTable.SoundInfo.SoundInfoList;
        foreach (var info in list)
		{
			SoundData data = new (info);
            _dicSoundData.Add(data._info.soundId, data);
#if UNITY_EDITOR
			_listSoundData.Add(data);
#endif
		}
    }
    public void ResetVolume()
	{
		int count = transform.childCount;
		Transform child;
		SoundObject sound;
		for (int ia = 0; ia < count; ia++)
		{

			child = transform.GetChild(ia);
			sound = child.GetComponent<SoundObject>();
			if (sound && sound.IsPlaying)
			{
				//sound.Volume = (sound.IsBGM) ? 0.01f * UserData.Instance.ValueBGM : 0.01f * UserData.Instance.ValueSFX;
				switch (sound._data._info.type)
				{
					case SoundType.SFX: sound.Volume = _volumeSFX; break;
					case SoundType.BGM: sound.Volume = _volumeBGM; break;
					case SoundType.VCE: sound.Volume = _volumeVCE; break;
					default: Debug.Log("Sound type is not correct!"); break;
				}
			}
		}
	}
}
