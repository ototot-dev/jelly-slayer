using UnityEngine;

public class PitchRandomizer : MonoBehaviour
{
    public AudioSource _audio;
    public float randomPercent = 10;

    public void Play() 
    {
        _audio.pitch *= 1 + Random.Range(-randomPercent / 100, randomPercent / 100);
    }
}
