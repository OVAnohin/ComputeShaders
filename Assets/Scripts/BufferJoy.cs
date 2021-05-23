using UnityEngine;
using System.Collections;

public class BufferJoy : MonoBehaviour
{
    [SerializeField] private ComputeShader _shader;
    [SerializeField] private int _texResolution = 1024;
    [SerializeField] private Color _clearColor = new Color();
    [SerializeField] private Color _circleColor = new Color();

    private Renderer _renderer;
    private RenderTexture _outputTexture;

    private int _circlesHandle;
    private int _clearHandle;
    private int _count = 10;
    private Circle[] _circleData;
    private ComputeBuffer _buffer;

    private void Start()
    {
        _outputTexture = new RenderTexture(_texResolution, _texResolution, 0);
        _outputTexture.enableRandomWrite = true;
        _outputTexture.Create();

        _renderer = GetComponent<Renderer>();
        _renderer.enabled = true;

        InitData();

        InitShader();
    }

    private void Update()
    {
        DispatchKernels(_count);
    }

    private void InitData()
    {
        _circlesHandle = _shader.FindKernel("Circles");

        uint threadGroupSizeX;
        _shader.GetKernelThreadGroupSizes(_circlesHandle, out threadGroupSizeX, out _, out _);
        int total = (int)threadGroupSizeX * _count;
        _circleData = new Circle[total];

        float speed = 100;
        float halfSpeed = speed * 0.5f;
        float minRadius = 10.0f;
        float maxRadius = 30.0f;
        float radiusRange = maxRadius - minRadius;

        for (int i = 0; i < total; i++)
        {
            Circle circle = _circleData[i];
            circle.origin.x = Random.value * _texResolution;
            circle.origin.y = Random.value * _texResolution;
            circle.velocity.x = (Random.value * speed) - halfSpeed;
            circle.velocity.y = (Random.value * speed) - halfSpeed;
            circle.radius = Random.value * radiusRange + minRadius;
            _circleData[i] = circle;
        }
    }

    private void InitShader()
    {
        _clearHandle = _shader.FindKernel("Clear");

        _shader.SetVector("clearColor", _clearColor);
        _shader.SetVector("circleColor", _circleColor);
        _shader.SetInt("texResolution", _texResolution);

        _shader.SetTexture(_clearHandle, "Result", _outputTexture);
        _shader.SetTexture(_circlesHandle, "Result", _outputTexture);

        int stride = (2 + 2 + 1) * sizeof(float);
        _buffer = new ComputeBuffer(_circleData.Length, stride);
        _buffer.SetData(_circleData);
        _shader.SetBuffer(_circlesHandle, "circlesBuffer", _buffer);

        _renderer.material.SetTexture("_MainTex", _outputTexture);
    }

    private void DispatchKernels(int count)
    {
        _shader.Dispatch(_clearHandle, _texResolution / 8, _texResolution / 8, 1);
        _shader.SetFloat("time", Time.time);
        _shader.Dispatch(_circlesHandle, count, 1, 1);
    }

    private struct Circle
    {
        public Vector2 origin;
        public Vector2 velocity;
        public float radius;
    }
}