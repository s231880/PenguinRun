using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class AudioFadeTween : FadeTween<AudioSource>
{
    protected override List<AudioSource> GetObjects()
    {
        return GetComponentsInChildren<AudioSource>().ToList();
    }

    protected override float GetAlpha(AudioSource obj)
    {
        return obj.volume;
    }

    protected override void SetAlpha(AudioSource obj, float alpha)
    {
        obj.volume = alpha;
    }
}
