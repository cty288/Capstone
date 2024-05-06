using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HueValue: MonoBehaviour
{
    public enum ColorType
    {
        Material,
        TrailMaterial,
        Light,
        ParticleColor
    }

    public ColorType type;
    public int colorNumber = 1;

    public float hue = 0;
    private Renderer _renderer;
    private ParticleSystemRenderer _particleSystemRenderer;
    private Light _light;
    private ParticleSystem _particleSystem;

    private void Start()
    {
        _renderer = transform.GetComponent<Renderer>();
        _particleSystemRenderer = transform.GetComponent<ParticleSystemRenderer>();
        _light = transform.GetComponent<Light>();
        _particleSystem = transform.GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if (type == ColorType.Material)
        {
            for (int i = 0; i < colorNumber; i++)
            {
                Color color = _renderer.material.GetColor("_Color"+(i+1));
                _renderer.material.SetColor("_Color"+(i+1), Hue(color, hue));
            }
        }

        if (type == ColorType.TrailMaterial)
        {
            for (int i = 0; i < colorNumber; i++)
            {
                Color color = _particleSystemRenderer.trailMaterial.GetColor("_Color" + (i + 1));
                _particleSystemRenderer.trailMaterial.SetColor("_Color" + (i + 1), Hue(color, hue));
            }
        }

        if (type == ColorType.Light)
        {
            Color color = _light.color;
            _light.color = Hue(color, hue);
        }
        
        if (type == ColorType.ParticleColor)
        {
            Color color = _particleSystem.main.startColor.color;

            ParticleSystem.MainModule m = _particleSystem.main;
            m.startColor = Hue(color, hue);
        }
    }

    Color Hue(Color main, float hue)
    {
        float h;
        float s;
        float v;

        float sh = hue;

        if (hue > 1)
        {
            sh = 1;
        }
        if (hue < 0)
        {
            sh = 0;
        }
        Color.RGBToHSV(main, out h, out s, out v);
        return Color.HSVToRGB(sh, s, v);
    }
}
