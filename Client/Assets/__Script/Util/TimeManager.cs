using UnityEngine;
using UniRx;
using System;

public class TimeManager : MonoSingleton<TimeManager>
{
    IDisposable _timer;

    public void SlomoTime(MonoBehaviour obj, float slomoRate = 0.5f, float slomoTime = 0.3f)
    {
        Time.timeScale = slomoRate;
        Observable.Timer(TimeSpan.FromSeconds(slomoTime))
            .DoOnCompleted(() =>
            {
                Time.timeScale = 1f;
            })
            .Subscribe().AddTo(obj);
    }
}
