using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floater : MonoBehaviour
{ 
    [SerializeField]
    private float degreesPerSecond = 15.0f;
    
    [SerializeField]
    private float amplitude = 0.5f;

    [SerializeField]
    private float frequency = 1f;

    [SerializeField]
    private Vector3 rotation = new Vector3 ();
    
    Vector3 posOffset = new Vector3 ();
    Vector3 tempPos = new Vector3 ();
 
    void Start () => posOffset = transform.position;
     
    void Update () 
    {
        transform.Rotate(rotation * Time.deltaTime * degreesPerSecond, Space.World);
        tempPos = posOffset;
        tempPos.y += Mathf.Sin (Time.fixedTime * Mathf.PI * frequency) * amplitude;
        transform.position = tempPos;
    }
}