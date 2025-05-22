using UnityEngine;
using Game;

public class DamageTextManager : MonoBehaviour
{
    [SerializeField]
    GameObject _prefDamageText;
    [SerializeField]
    RectTransform _canvasRect;
    [SerializeField]
    Transform _trRoot;

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

    void Awake()
    {
        GameContext.Instance.damageTextManager = this;
    }

    public DamageText Create(string text, Vector3 vPos, float scale, Color color) 
    {
        var dmgText = ObjectPoolingSystem.Instance.GetObject<DamageText>(_prefDamageText, Vector3.zero, Quaternion.identity);
        dmgText.SetText(text, vPos, scale, color);
        dmgText._rtRoot.SetParent(_trRoot);

        return dmgText;
    }

    public Vector3 GetCanvasPos(Vector3 pos)
    {
        //first you need the RectTransform component of your canvas
        //RectTransform CanvasRect = Canvas.GetComponent<RectTransform>();

        //then you calculate the position of the UI element
        //0,0 for the canvas is at the center of the screen,
        //whereas WorldToViewPortPoint treats the lower left corner as 0,0. Because of this,
        //you need to subtract the height / width of the canvas * 0.5 to get the correct position.

        Vector2 ViewportPosition = GameContext.Instance.cameraCtrler.gameCamera.WorldToViewportPoint(pos);
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
