using MagicLeapHandTracking.Assets.Scripts.Core;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class SuperPowersRaycastVisualizer : MonoBehaviourSingleton<SuperPowersRaycastVisualizer>
{
    [SerializeField, Tooltip("The reference to the class to handle results from.")]
    private BaseRaycast _raycast = null;

    [SerializeField, Tooltip("The default distance for the cursor when a hit is not detected.")]
    private float _defaultDistance = 9.0f;

    private bool _hit = false;

    private Renderer _render = null;

    void Awake()
    {
        if (_raycast == null)
        {
            Debug.LogError("Error: RaycastVisualizer._raycast is not set, disabling script.");
            enabled = false;
            return;
        }

        _render = GetComponent<Renderer>();
        if (_render == null)
        {
            Debug.LogError("Error: RaycastVisualizer._render is not set, disabling script.");
            enabled = false;
            return;
        }
    }
    
    public void OnRaycastHit(MLWorldRays.MLWorldRaycastResultState state, RaycastHit result, float confidence)
    {
        transform.position = (_raycast.RayOrigin + (_raycast.RayDirection * _defaultDistance));
        transform.LookAt(_raycast.RayOrigin);
        transform.localScale = Vector3.one;
        _render.material.color = Color.red;
    }
}