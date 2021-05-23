using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class AssignTexture : MonoBehaviour
{
    [SerializeField] private ComputeShader _shader;
    [SerializeField] private int _texResolution;
    [SerializeField] private string _kernelName;

    private Renderer _renderer;
    private RenderTexture _outputTexture;
    private int _kernel;

    private void Start()
    {
        _outputTexture = new RenderTexture(_texResolution, _texResolution, 0);
        _outputTexture.enableRandomWrite = true;
        _outputTexture.Create();

        _renderer = GetComponent<Renderer>();
        _renderer.enabled = true;

        InitShader();
    }

    private void InitShader()
    {
        _kernel = _shader.FindKernel(_kernelName);
        _shader.SetInt("TexResolution", _texResolution);
        _shader.SetTexture(_kernel, "Result", _outputTexture);
        _renderer.material.SetTexture("_MainTex", _outputTexture);

        DispathShader(32, 32);
    }

    private void DispathShader(int x, int y)
    {
        _shader.Dispatch(_kernel, x, y, 1);
    }
}
