using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BlurHighlight : BaseCompletePP
{
    [Range(0, 50)]
    [SerializeField] private int _blurRadius = 20;

    [Range(0.0f, 100.0f)]
    [SerializeField] private float _radius = 10;

    [Range(0.0f, 100.0f)]
    [SerializeField] private float _softenEdge = 30;

    [Range(0.0f, 1.0f)]
    [SerializeField] private float _shade = 0.5f;
    [SerializeField] private Transform _target;

    private Vector4 _center;

    protected override void Init()
    {
        _center = new Vector4();
        base.Init();
    }

    protected override void CreateTextures()
    {
        base.CreateTextures();
    }

    private void OnValidate()
    {
        if(!IsIneted)
            Init();
           
        SetProperties();
    }

    protected void SetProperties()
    {
        float radius = (_radius / 100.0f) * TextureSize.y;
        Shader.SetFloat("radius", radius);
        Shader.SetFloat("edgeWidth", radius * _softenEdge / 100.0f);
        Shader.SetFloat("shade", _shade);
    }

    protected override void DispatchWithSource(ref RenderTexture source, ref RenderTexture destination)
    {
        if (!IsIneted) return;

        Graphics.Blit(source, RenderedSourceTexture);

        Shader.Dispatch(KernelID, GroupSize.x, GroupSize.y, 1);

        Graphics.Blit(OutputTexture, destination);
    }

    protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (Shader == null)
        {
            Graphics.Blit(source, destination);
        }
        else
        {
            if (_target && Camera)
            {
                Vector3 position = Camera.WorldToScreenPoint(_target.position);
                _center.x = position.x;
                _center.y = position.y;
                Shader.SetVector("center", _center);
            }
            bool resChange = false;
            CheckResolution(out resChange);
            if (resChange) SetProperties();
            DispatchWithSource(ref source, ref destination);
        }
    }

}
