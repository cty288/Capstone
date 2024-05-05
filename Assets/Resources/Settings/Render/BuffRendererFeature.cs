using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class BuffRendererFeature : ScriptableRendererFeature
{

    [SerializeField] private Material materialHeal;
    [SerializeField] private Material materialBuff;

    public Material MaterialHeal {
        get => materialHeal;
        set => materialHeal = value;
    }

    public Material MaterialBuff
    {
        get => materialBuff;
        set => materialBuff = value;
    }

    

    private BuffRenderPass heal_ScriptablePass;
    private BuffRenderPass buff_ScriptablePass;
    
    
    public override void Create()
    {
        heal_ScriptablePass = new BuffRenderPass(materialHeal);
        heal_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        buff_ScriptablePass = new BuffRenderPass(materialBuff);
        buff_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        heal_ScriptablePass.source = renderer.cameraColorTarget;
        renderer.EnqueuePass(heal_ScriptablePass);
        buff_ScriptablePass.source = renderer.cameraColorTarget;
        renderer.EnqueuePass(buff_ScriptablePass);
    }
}


