using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ColourTween : ColourTween<Graphic>
{
    protected override List<Graphic> GetObjects()
    {
        return GetComponentsInChildren<Graphic>().ToList();
    }

    protected override Color GetColour(Graphic obj)
    {
        return obj.color;
    }

    protected override void SetColour(Graphic obj, Color colour)
    {
        obj.color = colour;
    }
}
