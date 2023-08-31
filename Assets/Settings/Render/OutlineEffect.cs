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
    public FloatParameter depthThreshold = new FloatParameter(2.56f);
    [Range(0, 1)] public FloatParameter normalThreshold = new FloatParameter(0.38f);
    [Range(0, 1)] public FloatParameter depthNormalThreshold = new FloatParameter(0.77f);
    public FloatParameter depthNormalThresholdScale = new FloatParameter(7.42f);
    [Range(0, 1)] public FloatParameter lineAlpha = new FloatParameter(0f);

    public bool IsActive()
    {
        return true;
    }

    public bool IsTileCompatible()
    {
        return true;
    }
}
