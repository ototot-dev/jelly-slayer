using Game;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UIGameOverlay : MonoBehaviour
{
    public Slider _sliderHP;
    public Slider _sliderStamina;

    SlayerBrain _heroBrain = null;


    // Start is called before the first frame update
    void Start()
    {
        _heroBrain = FindFirstObjectByType<SlayerBrain>().GetComponent<SlayerBrain>();

        _heroBrain.BB.stat.stamina.Subscribe(v =>
        {
            var rate = _heroBrain.BB.stat.stamina.Value / _heroBrain.BB.stat.maxStamina.Value;
            _sliderStamina.value = rate;

        }).AddTo(_heroBrain);

        _heroBrain.PawnHP.heartPoint.Subscribe(v =>
        {
            _sliderHP.value = v;

        }).AddTo(_heroBrain);


    }
}
