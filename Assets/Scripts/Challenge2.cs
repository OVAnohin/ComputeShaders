using UnityEngine;
using System.Collections;

public class Challenge2 : MonoBehaviour
{
    [SerializeField] private ComputeShader _shader;
    [SerializeField] private int _texResolution = 1024;
    [SerializeField] private Color fillColor = new Color(1.0f, 1.0f, 0.0f, 1.0f);
    [SerializeField] private Color clearColor = new Color(0, 0, 0.3f, 1.0f);

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

        Vector2 idxy = new Vector2(1, 1);
        //float2 pos = float2((((float2)id.xy) / (float)texResolution) - 0.5);
        float idx = idxy.x / (float)_texResolution - 0.5f;
        float idy = idxy.y / (float)_texResolution - 0.5f;
        Debug.Log(idx + " " + idy);

        int halfResolution = _texResolution >> 1;
        //float2 pt = ((float)(id.x - halfResolution), (float)(id.y - halfResolution));
        float ptx = idxy.x - halfResolution;
        float pty = idxy.y - halfResolution;
        Debug.Log(ptx + " " + pty);
        Debug.Log((float)_texResolution - 0.5f);
        Debug.Log(idxy.x / (float)_texResolution);
    }

    private void Update()
    {
        //DispatchShader(_texResolution / 8, _texResolution / 8);
    }

    private void InitShader()
    {
        _kernel = _shader.FindKernel("CSMain");

        _shader.SetVector("fillColor", fillColor);
        _shader.SetVector("clearColor", clearColor);

        _shader.SetInt("texResolution", _texResolution);
        _shader.SetTexture(_kernel, "Result", _outputTexture);

        _renderer.material.SetTexture("_MainTex", _outputTexture);
    }

    private void DispatchShader(int x, int y)
    {
        _shader.SetFloat("time", Time.time);
        _shader.Dispatch(_kernel, x, y, 1);
    }
}

