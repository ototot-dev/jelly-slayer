using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;

public class SoundObject : MonoBehaviour
{
    public SoundData _data;
    public AudioSource _source;

    public Transform _trRoot;
    // public PitchRandomizer _randomPitch;

    [SerializeField]
    bool _isPlay = false;

    [SerializeField]
    bool _isFadeOut = false;
    bool _isFadeIn = false;
    
    float _targetVolume = 1.0f;

    public float _lifeTime = 30;

    float _timePlayStart = 0;


	// Use this for initialization
	void Start () {
		
	}
    public bool IsPlaying {
        get {
            return (_source) ? _source.isPlaying : false;
        }
    }
    public bool IsFadeOut {
        get { return _isFadeOut; }
    }
    public bool IsFadeIn { 
        get { return _isFadeIn; } 
    }
    public float Volume {
        get {
            return (_source != null) ? _source.volume : 0;
        }
        set {
            if (_source != null) { // && _isFadeOut == false) {
                _source.volume = value;
            }
        }
    }
/*     
    public bool IsBGM {
        get {
            return (_info._playType == SoundInfo.TYPE.BGM) ? true : false;
        }
    }
*/    
    public void SetLoop(bool i_isLoop) {
        if (_source) {
            _source.loop = i_isLoop;
        }
    }
    void OnApplicationPause(bool pauseStatus)
    {
        _timePlayStart = Time.time;
        //if (pauseStatus == true) {  }
        //else { }
    }
    public void Play()
    {
        if (_source.clip == null)
        {
            _source.clip = Resources.Load(_data._info.resPath) as AudioClip;
            if (_source.clip == null)
            {
                Debug.Log("Clip Load Failure : " + _data._info.resPath);
            }
        }
        // _randomPitch.Play();
        _source.Play();

        _isPlay = true;
        _timePlayStart = Time.time;
    }
    public void PlayWithPath(string i_resPath)
    {
        if (_source.clip == null)
        {
            _source.clip = Resources.Load(i_resPath) as AudioClip;
        }
        // _randomPitch.Play();
        _source.Play();

        _isPlay = true;
        _timePlayStart = Time.time;
    }
    public void PlayWithClip(AudioClip i_clip)
    {
        _source.clip = i_clip;
        // _randomPitch.Play();
        _source.Play();

        _isPlay = true;
        _timePlayStart = Time.time;
    }    
    public void Stop() 
    {
        if(_isPlay == false)
            return;

        _source.Stop();

        _isPlay = false;
        _isFadeOut = false;
        _isFadeIn = false;
        _timePlayStart = Time.time;
    }
    public void FadeOut() 
    {
        if(IsPlaying == false || _isFadeOut == true)
            return;

        _isFadeOut = true;
        StartCoroutine(CoFadeOut());
    }
    public void FadeIn(float i_volume) 
    {
        if(IsPlaying == false || _isFadeIn == true)
            return;

        _isFadeIn = true;
        _targetVolume = i_volume;
        Debug.Log("FadeIn : " + _targetVolume);

        StartCoroutine(CoFadeIn());
    } 
    IEnumerator CoFadeOut() 
    {
        float vol = this.Volume;
        while(vol > 0.005f) 
        {
            vol = 0.7f * vol;
            this.Volume = vol;
            yield return new WaitForSeconds(0.02f);
        }
        this.Stop();
        _isFadeOut = false;
    }
    IEnumerator CoFadeIn() 
    {
        float vol = this.Volume;
        while((_targetVolume - vol) > 0.005f) 
        {
            vol += 0.3f * (_targetVolume - vol);
            this.Volume = vol;
            yield return new WaitForSeconds(0.05f);
        }
        this.Volume = _targetVolume;
        // Debug.Log("CoFadeIn : " + _targetVolume);

        _isFadeIn = false;
    }    
	// Update is called once per frame
	void Update ()
    {
        if (_isPlay == false)
            return;

        if (_source.isPlaying == false)
        {
            if (Time.time - _timePlayStart > _lifeTime)
            {
                if (_data != null)
                {
                    _data.Delete(this);
                }
                else 
                {
                    GameObject.Destroy(gameObject);
                }
            }
        }
	}
}
