using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class BasePP : MonoBehaviour
{
    [SerializeField] private ComputeShader _shader;
    [SerializeField] private string _kernelName;
    [SerializeField] private Transform _trackedObject;

    [Range(0.0f, 100.0f)]
    [SerializeField] private float _radius = 10;
    
    [Range(0.0f, 100.0f)]
    [SerializeField] private float _softenEdge;
    
    [Range(0.0f, 1.0f)]
    [SerializeField] private float _shade;
    

    private Vector4 _center;
    private Vector2Int _textureSize = new Vector2Int(0, 0);
    private Vector2Int _groupSize = new Vector2Int();
    private Camera _camera;

    private RenderTexture _outputTexture = null;
    private RenderTexture _renderedSourceTexture = null;

    private int _kernel = -1;
    private bool _isInit = false;

    private void OnEnable()
    {
        Init();
        CreateTextures();
    }

    private void OnDisable()
    {
        ClearTextures();
        _isInit = false;
    }

    private void OnDestroy()
    {
        ClearTextures();
        _isInit = false;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_trackedObject && _camera)
        {
            Vector3 position = _camera.WorldToScreenPoint(_trackedObject.position);
            _center.x = position.x;
            _center.y = position.y;
            _shader.SetVector("center", _center);
        }
        
        bool resolutionChange = false;
        CheckResolution(out resolutionChange);

        if (resolutionChange) 
            SetProperties();

        DispatchWithSource(ref source, ref destination);
    }

    private void OnValidate()
    {
        if (!_isInit)
            Init();

        SetProperties();
    }

    private void SetProperties()
    {
        float radius = (_radius / 100.0f) * _textureSize.y;
        _shader.SetFloat("radius", radius);
        _shader.SetFloat("edgeWidth", radius * _softenEdge / 100.0f);
        _shader.SetFloat("shade", _shade);
    }

    private void Init()
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

        _center = new Vector4();
        _kernel = _shader.FindKernel(_kernelName);
        _camera = GetComponent<Camera>();

        if (!_camera)
        {
            Debug.LogError("Object has no Camera");
            return;
        }

        CreateTextures();

        _isInit = true;
    }

    private void CreateTexture(ref RenderTexture textureToMake, int divide = 1)
    {
        textureToMake = new RenderTexture(_textureSize.x / divide, _textureSize.y / divide, 0);
        textureToMake.enableRandomWrite = true;
        textureToMake.Create();
    }

    private void CreateTextures()
    {
        _textureSize.x = _camera.pixelWidth;
        _textureSize.y = _camera.pixelHeight;

        uint x, y;
        _shader.GetKernelThreadGroupSizes(_kernel, out x, out y, out _);
        _groupSize.x = Mathf.CeilToInt((float)_textureSize.x / (float)x);
        _groupSize.y = Mathf.CeilToInt((float)_textureSize.y / (float)y);

        CreateTexture(ref _renderedSourceTexture);
        CreateTexture(ref _outputTexture);

        _shader.SetTexture(_kernel, "RenderedSourceTexture", _renderedSourceTexture);
        _shader.SetTexture(_kernel, "outputTexture", _outputTexture);
    }

    private void ClearTexture(ref RenderTexture textureToClear)
    {
        if (null != textureToClear)
        {
            textureToClear.Release();
            textureToClear = null;
        }
    }

    private void ClearTextures()
    {
        ClearTexture(ref _outputTexture);
        ClearTexture(ref _renderedSourceTexture);
    }

    private void DispatchWithSource(ref RenderTexture source, ref RenderTexture destination)
    {
        Graphics.Blit(source, _renderedSourceTexture);

        _shader.Dispatch(_kernel, _groupSize.x, _groupSize.y, 1);

        Graphics.Blit(_outputTexture, destination);
    }

    private void CheckResolution(out bool isResolutionChanged)
    {
        isResolutionChanged = false;

        if (_textureSize.x != _camera.pixelWidth || _textureSize.y != _camera.pixelHeight)
        {
            isResolutionChanged = true;
            CreateTextures();
        }
    }
}
