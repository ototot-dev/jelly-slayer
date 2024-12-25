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
    public RectTransform _rtCanvas;
    public GameObject _prefHPBar;

    [Header("Camera")]
    public CameraController _cameraController;
    public Camera _camera;

    public Dictionary<PawnBrainController, HPBarPanel> _dicPanel = new();

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

    public HPBarPanel GetPanel(PawnBrainController pawn)
    {
        if(_dicPanel.TryGetValue(pawn, out HPBarPanel panel))
        {
            return panel;
        }
        return null;
    }
    public void Delete(HPBarPanel panel) 
    { 
        panel.gameObject.SetActive(false);
    }
    public bool Create(PawnBrainController pawn, bool isStamina) 
    {
        if(_dicPanel.ContainsKey(pawn))
        {
            return false;
        }
        GameObject obj = Instantiate(_prefHPBar);

        var panel = obj.GetComponent<HPBarPanel>();
        panel._rtRoot.SetParent(transform);
        // panel._camera = _camera;
        // panel._cameraController = _cameraController;
        panel._rtCanvas = _rtCanvas;
        panel.SetPawn(pawn, isStamina);

        _dicPanel.Add(pawn, panel);

        return true;
    }
    public void PawnDamaged(ref PawnHeartPointDispatcher.DamageContext damageContext) 
    { 
        var panel = GetPanel(damageContext.receiverBrain);
        if(panel != null)
        {
            panel.PawnDamaged(ref damageContext);
        }
    }
}
