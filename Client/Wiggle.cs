using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wiggle : MonoBehaviour
{
    void Start()
    {
        
    }
    
    void Update()
    {
        Vector3 euler = transform.rotation.eulerAngles;
        euler.x = Mathf.Sin(Time.time * 3) * 2;
        transform.rotation = Quaternion.Euler(euler);
    }
}
