using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("Custom/BuffEffect", typeof(UniversalRenderPipeline))]
public class BuffEffect : VolumeComponent, IPostProcessComponent
{
    public SSBuffEffectStack BuffEffectType = SSBuffEffectStack.Heal;

    public ColorParameter colorHeal = new ColorParameter(Color.green);
    public FloatParameter healToggle = new FloatParameter(0.0f);
    public ColorParameter color1 = new ColorParameter(Color.green);
    public FloatParameter color1Toggle = new FloatParameter(0.0f);
    public ColorParameter color2 = new ColorParameter(Color.green);
    public FloatParameter color2Toggle = new FloatParameter(0.0f);
    
    public bool IsActive()
    {
        return true;
    }

    public bool IsTileCompatible()
    {
        return true;
    }
}

public enum SSBuffEffectStack
{
    Heal,
    BuffType1,
}