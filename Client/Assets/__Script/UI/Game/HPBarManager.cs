using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBarManager : MonoBehaviour
{
    static HPBarManager _instance;
    public static HPBarManager Instance { 
        get { return _instance; } 
    }
    public Camera _camera;
    public RectTransform _rtCanvas;
    public GameObject _prefHPBar;
    public CameraController _cameraController;

    private void Awake()
    {
        _instance = this;
    }
    void Start()
    {
        if(_camera == null)
        {
            _camera = Camera.main;
        }
        if(_cameraController == null)
        {
            _cameraController = _camera.GetComponent<CameraController>();
        }
    }
    public void Delete(HPBarPanel panel) 
    { 
        panel.gameObject.SetActive(false);
    }
    public void Create(PawnBrainController pawn, bool isStamina) 
    {
        GameObject obj = Instantiate(_prefHPBar);

        var panel = obj.GetComponent<HPBarPanel>();
        panel._rtRoot.SetParent(transform);
        // panel._camera = _camera;
        // panel._cameraController = _cameraController;
        panel._rtCanvas = _rtCanvas;
        panel.SetPawn(pawn, isStamina);
    }
}
