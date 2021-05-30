using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 _offset;
    [SerializeField] private Transform _target;

    private void Start()
    {
        transform.position = _target.position + _offset;
    }

    private void Update()
    {
        transform.position = new Vector3(_target.position.x + _offset.x, _offset.y, _offset.z);
        transform.LookAt(_target);
    }
}
