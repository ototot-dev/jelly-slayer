using Game;
using UnityEngine;

public class EffectCenter
{
    public static GameObject CreateEffect(int idEffect, Vector3 pos) 
    {
        var map = MainTable.EffectInfo.GetDictionary();
        if (map.TryGetValue(idEffect, out var info))
        {
            if (info == null)
            {
                Debug.Log(string.Format("EffectInfo Error : {0}", idEffect));
                return null;
            }
            Object objRes = Resources.Load(info.resPath);
            if (objRes == null)
            {
                Debug.Log(string.Format("Effect Res Error : {0} : {1}", idEffect, info.resPath));
                return null;
            }
            GameObject objEffect = (GameObject)GameObject.Instantiate(objRes);
            if (objEffect != null)
            {
                objEffect.transform.position = pos;
            }
            return objEffect;
        }
        return null;
    }
}
