using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Game;

public class DamageTextManager : MonoBehaviour
{
    [SerializeField]
    GameObject _prefDamageText;
    [SerializeField]
    RectTransform _canvasRect;
    [SerializeField]
    Transform _trRoot;

    Stack<DamageText> _stack = new();

    [Space(10)]
    public float _xDamp = 0.01f;
    public float _yDamp = 0.03f;
    public float _yMin = 2.0f;
    public float _yMax = 2.5f;

#if UNITY_EDITOR
    [Space(10)]
    [SerializeField]
    PawnBrainController _testPawn;
#endif
    //public List<DamageText> _damageTexts = new List<DamageText>();

    // Start is called before the first frame update
    void Start()
    {

    }
    DamageText Alloc()
    {
        if (_stack.Count > 0)
        {
            return _stack.Pop();
        }
        GameObject obj = GameObject.Instantiate(_prefDamageText);

        var dmgText = obj.GetComponent<DamageText>();
        dmgText.Manager = this;
        dmgText._rtRoot.SetParent(_trRoot);

        return dmgText;
    }
    
    public DamageText Create(string text, Vector3 vPos, float scale, Color color) 
    {
        var dmgText = Alloc();
        if (dmgText == null)
            return null;

        //float scale = 0.2f;
        //vPos.x += scale * Random.Range(-1.0f, 1.0f);
        //vPos.y += 0.3f * scale * Random.Range(-1.0f, 1.0f);
        //vPos.z += 0.3f * scale * Random.Range(-1.0f, 1.0f);             

        dmgText.SetText(text, vPos, scale, color);

        return dmgText;
    }
    public void Die(DamageText damageText) 
    {
        _stack.Push(damageText);
    }
    public Vector3 GetCanvasPos(Vector3 pos)
    {
        //first you need the RectTransform component of your canvas
        //RectTransform CanvasRect = Canvas.GetComponent<RectTransform>();

        //then you calculate the position of the UI element
        //0,0 for the canvas is at the center of the screen,
        //whereas WorldToViewPortPoint treats the lower left corner as 0,0. Because of this,
        //you need to subtract the height / width of the canvas * 0.5 to get the correct position.

        Vector2 ViewportPosition = GameContext.Instance.cameraCtrler.viewCamera.WorldToViewportPoint(pos);
        Vector2 WorldObject_ScreenPosition = new (
            ((ViewportPosition.x * _canvasRect.sizeDelta.x) - (_canvasRect.sizeDelta.x * 0.5f)),
            ((ViewportPosition.y * _canvasRect.sizeDelta.y) - (_canvasRect.sizeDelta.y * 0.5f)));

        //now you can set the position of the ui element
        return WorldObject_ScreenPosition;
    }
#if UNITY_EDITOR
    private void Update()
    {
        if (_testPawn == null)
            return;

        //if (Input.GetKeyDown(KeyCode.N))
        {
            ///Vector3 vPos = _testPawn.coreColliderHelper.transform.position + (2 * Vector3.up);
            //Create("Test", vPos, Color.white);
        }
    }
#endif
}
