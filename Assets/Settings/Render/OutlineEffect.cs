using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("Custom/OutlineEffect", typeof(UniversalRenderPipeline))]
public class OutlineEffect : VolumeComponent, IPostProcessComponent
{
    public IntParameter scale = new IntParameter(1);
    public FloatParameter depthThreshold = new FloatParameter(0.2f);
    [Range(0, 1)] public FloatParameter normalThreshold = new FloatParameter(0.4f);
    [Range(0, 1)] public FloatParameter depthNormalThreshold = new FloatParameter(0.5f);
    public FloatParameter depthNormalThresholdScale = new FloatParameter(7);
    [Range(0, 1)] public FloatParameter lineAlpha = new FloatParameter(1f);

    public bool IsActive()
    {
        return true;
    }

    public bool IsTileCompatible()
    {
        return true;
    }
}
