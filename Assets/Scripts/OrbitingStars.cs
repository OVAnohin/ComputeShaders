using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitingStars : MonoBehaviour
{
    [SerializeField] private int _starCount;
    [SerializeField] private ComputeShader _shader;
    [SerializeField] private GameObject _starPrefab;

    private int _kernel;
    private uint _threadGroupSizeX;
    private int _groupSizeX;

    private Transform[] _stars;
    private ComputeBuffer _resultBuffer;
    private Vector3[] _output;

    private void Start()
    {
        _kernel = _shader.FindKernel("OrbitingStars");
        _shader.GetKernelThreadGroupSizes(_kernel, out _threadGroupSizeX, out _, out _);
        _groupSizeX = (int)((_starCount + _threadGroupSizeX - 1) / _threadGroupSizeX);
        Debug.Log(_groupSizeX);

        _resultBuffer = new ComputeBuffer(_starCount, sizeof(float) * 3);
        _shader.SetBuffer(_kernel, "Result", _resultBuffer);
        _output = new Vector3[_starCount];

        _stars = new Transform[_starCount];
        for (int i = 0; i < _starCount; i++)
        {
            _stars[i] = Instantiate(_starPrefab, transform).transform;
        }
    }

    private void Update()
    {
        _shader.SetFloat("Time", Time.time);
        _shader.Dispatch(_kernel, _groupSizeX, 1, 1);

        _resultBuffer.GetData(_output);
        for (int i = 0; i < _stars.Length; i++)
        {
            _stars[i].localPosition = _output[i];
        }
    }

    private void OnDestroy()
    {
        _resultBuffer.Dispose();
    }
}
