using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

class BuffRenderPass : ScriptableRenderPass
{

    // Used to render from camera to post processings
	// back and forth, until we render the final image to
	// the camera
    public RenderTargetIdentifier source;
    
    RenderTargetIdentifier destinationA;
    RenderTargetIdentifier destinationB;
    RenderTargetIdentifier latestDest;

    readonly int temporaryRTIdA = Shader.PropertyToID("_TempRT");
    readonly int temporaryRTIdB = Shader.PropertyToID("_TempRTB");

    readonly int _color1ID = Shader.PropertyToID("_Color1");
    readonly int _color1ToggleID = Shader.PropertyToID("_Color1Toggle");
    readonly int _color2ID = Shader.PropertyToID("_Color2");
    readonly int _color2ToggleID = Shader.PropertyToID("_Color2Toggle");
    readonly int _colorHealID = Shader.PropertyToID("_ColorHeal");
    readonly int _healToggleID = Shader.PropertyToID("_HealToggle");

    private Material material;
    

    public BuffRenderPass(Material material)
    {
        // Set the render pass event
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        this.material = material;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ConfigureInput(ScriptableRenderPassInput.Normal);
        
        // Grab the camera target descriptor. We will use this when creating a temporary render texture.
        RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.depthBufferBits = 0;

        var renderer = renderingData.cameraData.renderer;
        source = renderer.cameraColorTarget;

        // Create a temporary render texture using the descriptor from above.
        cmd.GetTemporaryRT(temporaryRTIdA , descriptor, FilterMode.Bilinear);
        destinationA = new RenderTargetIdentifier(temporaryRTIdA);
        cmd.GetTemporaryRT(temporaryRTIdB , descriptor, FilterMode.Bilinear);
        destinationB = new RenderTargetIdentifier(temporaryRTIdB);
    }
    
    // The actual execution of the pass. This is where custom rendering occurs.
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
    	// Skipping post processing rendering inside the scene View
        if(renderingData.cameraData.isSceneViewCamera)
            return;
        
        if (material == null)
        {
            Debug.LogError("Custom Post Processing Materials instance is null");
            return;
        }
        
        CommandBuffer cmd = CommandBufferPool.Get("Custom Post Processing");
        cmd.Clear();

		// This holds all the current Volumes information
		// which we will need later
        var stack = VolumeManager.instance.stack;

        #region Local Methods

		// Swaps render destinations back and forth, so that
		// we can have multiple passes and similar with only a few textures
        void BlitTo(Material mat, int pass = 0)
        {
            var first = latestDest;
            var last = first == destinationA ? destinationB : destinationA;
            Blit(cmd, first, last, mat, pass);

            latestDest = last;
        }

        #endregion

		// Starts with the camera source
        latestDest = source;

        //---Custom effect here---
        var customEffect = stack.GetComponent<BuffEffect>();
        // Only process if the effect is active
        if (customEffect.IsActive())
        {
            // P.s. optimize by caching the property ID somewhere else
            //material.SetInt(Shader.PropertyToID("_Scale"), customEffect.scale.value);
            material.SetColor(_color1ID, customEffect.color1.value);
            material.SetFloat(_color1ToggleID, customEffect.color1Toggle.value);
            material.SetColor(_color2ID, customEffect.color2.value);
            material.SetFloat(_color2ToggleID, customEffect.color2Toggle.value);
            material.SetColor(_colorHealID, customEffect.colorHeal.value);
            material.SetFloat(_healToggleID, customEffect.healToggle.value);
            BlitTo(material);
        }
        
        // Add any other custom effect/component you want, in your preferred order
        // Custom effect 2, 3 , ...

		
		// DONE! Now that we have processed all our custom effects, applies the final result to camera
        Blit(cmd, latestDest, source);
        
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

	//Cleans the temporary RTs when we don't need them anymore
    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(temporaryRTIdA);
        cmd.ReleaseTemporaryRT(temporaryRTIdB);
    }
}
