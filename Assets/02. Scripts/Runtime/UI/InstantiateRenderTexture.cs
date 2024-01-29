using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using UnityEngine;

public class InstantiateRenderTexture : AbstractMikroController<MainGame>
{
    [SerializeField] private Renderer renderer;

    private RenderTexture _renderTexture;
    // Start is called before the first frame update
    void Start()
    {
        var cam = GetComponent<Camera>();
        _renderTexture = new RenderTexture(cam.targetTexture);
        cam.targetTexture = _renderTexture;
        renderer.material.SetTexture("_BaseMap", _renderTexture);
    }
}
