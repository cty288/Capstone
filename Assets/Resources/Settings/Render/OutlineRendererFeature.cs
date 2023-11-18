using UnityEngine;
using UnityEngine.Rendering.Universal;

public class OutlineRendererFeature : ScriptableRendererFeature
{

    [SerializeField] private Material material;

    OutlineRenderPass m_ScriptablePass;
    
    
    public override void Create()
    {
        m_ScriptablePass = new OutlineRenderPass(material);
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


