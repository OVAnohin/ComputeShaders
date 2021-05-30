using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class BaseCompletePP : MonoBehaviour
{
    [SerializeField] private ComputeShader _shader = null;
    [SerializeField] private string _kernelName = "CSMain";

    private Vector2Int _textureSize = new Vector2Int(0, 0);
    private Vector2Int _groupSize = new Vector2Int();
    private Camera _camera;

    private RenderTexture _outputTexture = null;
    private RenderTexture _renderedSourceTexture = null;

    private int _kernelID = -1;
    public bool IsIneted { get; private set; }
    public Vector2Int TextureSize => _textureSize;
    public Vector2Int GroupSize => _groupSize;
    public ComputeShader Shader => _shader;
    public RenderTexture OutputTexture => _outputTexture;
    public RenderTexture RenderedSourceTexture => _renderedSourceTexture;
    public int KernelID => _kernelID;
    public Camera Camera => _camera;

    protected virtual void OnEnable()
    {
        IsIneted = false;
        Init();
        CreateTextures();
    }

    protected virtual void OnDisable()
    {
        ClearTextures();
        IsIneted = false;
    }

    protected virtual void OnDestroy()
    {
        ClearTextures();
        IsIneted = false;
    }

    protected virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!IsIneted || _shader == null)
        {
            Graphics.Blit(source, destination);
        }
        else
        {
            CheckResolution(out _);
            DispatchWithSource(ref source, ref destination);
        }
    }

    protected virtual void Init()
    {
        if (!SystemInfo.supportsComputeShaders)
        {
            Debug.LogError("It seems your target Hardware does not support Compute Shaders.");
            return;
        }

        if (!_shader)
        {
            Debug.LogError("No shader");
            return;
        }

        _kernelID = _shader.FindKernel(_kernelName);

        _camera = GetComponent<Camera>();

        if (!_camera)
        {
            Debug.LogError("Object has no Camera");
            return;
        }

        CreateTextures();

        IsIneted = true;
    }

    protected void ClearTexture(ref RenderTexture textureToClear)
    {
        if (null != textureToClear)
        {
            textureToClear.Release();
            textureToClear = null;
        }
    }

    protected virtual void ClearTextures()
    {
        ClearTexture(ref _outputTexture);
        ClearTexture(ref _renderedSourceTexture);
    }

    protected void CreateTexture(ref RenderTexture textureToMake, int divide = 1)
    {
        textureToMake = new RenderTexture(_textureSize.x / divide, _textureSize.y / divide, 0);
        textureToMake.enableRandomWrite = true;
        textureToMake.Create();
    }


    protected virtual void CreateTextures()
    {
        _textureSize.x = _camera.pixelWidth;
        _textureSize.y = _camera.pixelHeight;

        if (_shader)
        {
            uint x, y;
            _shader.GetKernelThreadGroupSizes(_kernelID, out x, out y, out _);
            _groupSize.x = Mathf.CeilToInt((float)_textureSize.x / (float)x);
            _groupSize.y = Mathf.CeilToInt((float)_textureSize.y / (float)y);
        }

        CreateTexture(ref _outputTexture);
        CreateTexture(ref _renderedSourceTexture);

        _shader.SetTexture(_kernelID, "source", _renderedSourceTexture);
        _shader.SetTexture(_kernelID, "output", _outputTexture);
    }

    protected virtual void DispatchWithSource(ref RenderTexture source, ref RenderTexture destination)
    {
        Graphics.Blit(source, _renderedSourceTexture);

        _shader.Dispatch(_kernelID, _groupSize.x, _groupSize.y, 1);

        Graphics.Blit(_outputTexture, destination);
    }

    protected void CheckResolution(out bool resolutionChange)
    {
        resolutionChange = false;

        if (_textureSize.x != _camera.pixelWidth || _textureSize.y != _camera.pixelHeight)
        {
            resolutionChange = true;
            CreateTextures();
        }
    }
}

