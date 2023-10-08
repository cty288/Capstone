using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("Custom/SandstormFog", typeof(UniversalRenderPipeline))]
public class SandstormEffect : VolumeComponent, IPostProcessComponent
{
    //public IntParameter scale = new IntParameter(1);
    public IntParameter posterization = new IntParameter(40);
    public FloatParameter maxGradientDistance = new FloatParameter(400.0f);
    public FloatParameter sandstormDepthDistance = new FloatParameter(100.0f);
    public Vector4Parameter noiseStrengths = new Vector4Parameter(new Vector4(0.1f, 0.1f, 0.01f, 0.8f));
    [Range(0, 1)]public FloatParameter sandstormAlpha = new FloatParameter(1);

    public bool IsActive()
    {
        return true;
    }

    public bool IsTileCompatible()
    {
        return true;
    }
}
