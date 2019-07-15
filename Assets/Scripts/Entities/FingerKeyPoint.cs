using UnityEngine;
using UnityEngine.XR.MagicLeap;

[System.Serializable]
public class FingerKeyPoint 
{
    [SerializeField]
    public string Name;
    public MLKeyPoint KeyPoint;
}