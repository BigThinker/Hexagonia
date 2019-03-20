using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wiggle : MonoBehaviour
{
    private float frequency = 3;
    private float amplitude = 2;

    void Update()
    {
        Vector3 euler = transform.rotation.eulerAngles;
        euler.x = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.rotation = Quaternion.Euler(euler);
    }
}
