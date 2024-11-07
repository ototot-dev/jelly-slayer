using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BackgroundTest : MonoBehaviour
{
    public Transform _trRoot;
    public int _width = 12;
    public int _height = 12;
    public float _scale = 1.5f;

    public List<GameObject> _prefBaseList;
    public List<GameObject> _prefWallList;
    public List<GameObject> _prefGrassList;
    public List<GameObject> _prefPilarList;

    GameObject CreatePref(GameObject prefObj, float tx, float ty, float tz) 
    {
        GameObject obj = GameObject.Instantiate(prefObj);
        obj.transform.SetParent(_trRoot);
        obj.transform.localScale = _scale * Vector3.one;
        obj.transform.localPosition = new Vector3(tx, ty, tz);

        return obj;
    }

    // Start is called before the first frame update
    void Awake()
    {
        int rand = 0;
        float scale2 = 4 * _scale;
        float halfSize = scale2 * _width / 2;
        int count = _prefBaseList.Count;
        GameObject obj;

        // Base
        for (int iy = 0; iy < _height; iy++) 
        {
            for (int ix = 0; ix < _width; ix++) 
            {
                rand = Random.Range(0, count);

                CreatePref(_prefBaseList[rand], -halfSize + scale2 * ix, 0, -halfSize + scale2 * iy);
            }
        }
        // Wall
        float ty = 0.9f;
        int count2 = _prefWallList.Count;
        for (int ix = 0; ix < _width; ix++) 
        {
            rand = Random.Range(0, count2);

            CreatePref(_prefWallList[rand], -halfSize + scale2 * ix, ty, -halfSize - 3f);
            CreatePref(_prefWallList[rand], -halfSize + scale2 * ix, ty, halfSize - 3f);
        }
        for (int iy = 0; iy < _height; iy++)
        {
            rand = Random.Range(0, count2);

            obj = CreatePref(_prefWallList[rand], -halfSize - 3f, ty, -halfSize + scale2 * iy);
            obj.transform.localRotation = Quaternion.Euler(0, 90, 0);

            obj = CreatePref(_prefWallList[rand], halfSize - 3f, ty, -halfSize + scale2 * iy);
            obj.transform.localRotation = Quaternion.Euler(0, 90, 0);
        }
        // Prop
        scale2 = 4 * _scale / 2.5f;
        count2 = _prefGrassList.Count;
        int prop;
        float tx, tz;
        float tx2, tz2;
        float randLImit = 50;
        _scale = 1.0f;

        for (int iy = 0; iy < 2.5f * _height; iy++)
        {
            for (int ix = 0; ix < 2.5f * _width; ix++)
            {
                rand = Random.Range(0, 100);
                if (rand < randLImit)
                {
                    prop = Random.Range(0, count2);

                    tx = 0.2f * scale2 * Random.Range(-1.0f, 1.0f);
                    tz = 0.2f * scale2 * Random.Range(-1.0f, 1.0f);
                    tx2 = tx - halfSize + scale2 * ix - (1.0f * scale2);
                    tz2 = tz - halfSize + scale2 * iy - (1.0f * scale2);

                    CheckClipping(ref tx2, ref tz2);
                    CreatePref(_prefGrassList[prop], tx2, 0, tz2);

                    randLImit = 60;
                }
                else
                {
                    randLImit = 20;
                }
            }
        }
        // Pilar
        _scale = 1.5f;
        scale2 = 4 * _scale;
        count2 = _prefPilarList.Count;
        for (int iy = 0; iy < _height; iy++)
        {
            for (int ix = 0; ix < _width; ix++)
            {
                rand = Random.Range(0, 100);
                if (rand < 10)
                {
                    prop = Random.Range(0, count2);

                    tx = 0.2f * scale2 * Random.Range(-1.0f, 1.0f);
                    tz = 0.2f * scale2 * Random.Range(-1.0f, 1.0f);
                    tx2 = tx - halfSize + scale2 * ix - (1.0f * scale2);
                    tz2 = tz - halfSize + scale2 * iy - (1.0f * scale2);

                    CheckClipping(ref tx2, ref tz2);
                    CreatePref(_prefPilarList[prop], tx2, 0, tz2);
                }
            }
        }
    }
    void CheckClipping(ref float tx, ref float tz) 
    {
        float limitMin = -22.0f;
        float limitMax = 16.5f;

        if (tx >= limitMax)
            tx = limitMax;
        else if (tx <= limitMin)
            tx = limitMin;

        if (tz >= limitMax)
            tz = limitMax;
        else if (tz <= limitMin)
            tz = limitMin;
    }
}
