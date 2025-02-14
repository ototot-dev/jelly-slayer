using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;

[System.Serializable]
public class SoundData
{
#if UNITY_EDITOR
    public string _name;
#endif
    public MainTable.SoundInfo _info;
    public List<SoundObject> _listObj = new List<SoundObject>();

    public SoundData(MainTable.SoundInfo info) 
    {
        _info = info;

#if UNITY_EDITOR
        _name = _info.name;
#endif
    }

    SoundObject ReusebleObject()
    {
        foreach (SoundObject sound in _listObj)
        {
            if (sound.IsPlaying == false)
            {
                return sound;
            }
        }
        if (_listObj.Count >= _info.maxChannel)
            return null;

        GameObject prefObj = Resources.Load("Sound/SoundObject") as GameObject;
        if (prefObj == null)
            return null;

        GameObject obj = GameObject.Instantiate(prefObj);
        SoundObject soundObj = obj.GetComponent<SoundObject>();

        _listObj.Add(soundObj);

        return soundObj;
    }
    public SoundObject Create()
    {
        SoundObject sound = ReusebleObject();
        if (sound != null)
        {
            sound._data = this;
        }
        return sound;
    }
    public void Delete(SoundObject i_obj)
    {   
        _listObj.Remove(i_obj);
        GameObject.Destroy(i_obj.gameObject);
    }
    public void FadeOut() 
    {
        if(_listObj == null)
            return;

        foreach(SoundObject sound in _listObj) 
        {
            if (sound != null) 
                sound.FadeOut();
        }
    }
    public void AllStop() 
    {
        int count = _listObj.Count;
        //Debug.Log("AllStop : " + count);

        for (int ia = 0; ia < count; ia++) 
        {
            //_listObj[ia].Stop();
        }
    }    
}
