using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LearnStars : MonoBehaviour
{
    [SerializeField] private int _starCount;
    [SerializeField] private ComputeShader _shader;

    private int _kernel;
    private uint _threadGroupSizeX;
    private int _groupSizeX;

    private int[] _stars;
    private ComputeBuffer _resultBuffer;
    private int[] _output;

    private void Start()
    {
        _kernel = _shader.FindKernel("Learn");
        _shader.GetKernelThreadGroupSizes(_kernel, out _threadGroupSizeX, out _, out _);
        _groupSizeX = (int)((_starCount + _threadGroupSizeX - 1) / _threadGroupSizeX);
        Debug.Log(_groupSizeX);

        _resultBuffer = new ComputeBuffer(_starCount, sizeof(int) * 1);
        _shader.SetBuffer(_kernel, "Result", _resultBuffer);
        _output = new int[_starCount];

        _stars = new int[_starCount];
    }

    private void Update()
    {
        _shader.Dispatch(_kernel, _groupSizeX, 1, 1);

        _resultBuffer.GetData(_output);
        for (int i = 0; i < _stars.Length; i++)
        {
            Debug.Log(_output[i]);
        }
    }

    private void OnDestroy()
    {
        _resultBuffer.Dispose();
    }
}
