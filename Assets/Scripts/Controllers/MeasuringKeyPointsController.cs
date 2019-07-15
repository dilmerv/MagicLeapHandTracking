using System.Collections.Generic;
using MagicLeapHandTracking.Assets.Scripts.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;
using System.Linq;

[RequireComponent(typeof(HandTracking))]
[RequireComponent(typeof(LineRenderer))]
public class MeasuringKeyPointsController : MonoBehaviourSingleton<MeasuringKeyPointsController>
{

    [SerializeField, Tooltip("Text to display gesture status to.")]
    private Text _statusText = null;

    [SerializeField]
    private GameObject keyPointPrefab;

    [SerializeField]
    private float measurementFactor = 39.37f;

    [SerializeField]
    private Text distanceText;

    private Dictionary<string, GameObject> cacheKeyPoints;

    private GameObject leftHandFinger, rightHandFinger;

    private LineRenderer measureLine;
    
    void Awake()
    {   
        if(_statusText == null || distanceText == null)
        {
            Debug.LogError("UI must all be bound through the inspector");
            enabled = false;
            return;
        }

        cacheKeyPoints = new Dictionary<string, GameObject>();
        measureLine = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (MLHands.IsStarted)
        {
            _statusText.text = $"Hand tracking has started...";
            
            AddAndUpdateHandKeyPoint(new FingerKeyPoint[] 
            { 
                new FingerKeyPoint { Name = $"HandLeft.{nameof(MLHands.Left.Index)}", KeyPoint = MLHands.Left.Index.KeyPoints[0]},
                new FingerKeyPoint { Name = $"HandRight.{nameof(MLHands.Right.Index)}", KeyPoint = MLHands.Right.Index.KeyPoints[0]},
            });

            if(cacheKeyPoints.Count > 0)
            {
                leftHandFinger = cacheKeyPoints.FirstOrDefault().Value;
                rightHandFinger = cacheKeyPoints.LastOrDefault().Value;

                // update source and target line
                measureLine.SetPosition(0, leftHandFinger.transform.position);
                measureLine.SetPosition(1, rightHandFinger.transform.position);

                distanceText.text = $"DISTANCE: {(Vector3.Distance(leftHandFinger.transform.position, rightHandFinger.transform.position) * measurementFactor).ToString("F2")} in";
            }

            _statusText.text = $"Hand Key Points: {string.Join(",", cacheKeyPoints.Keys.Select(s => s).ToArray())}";
        }
        else 
        {
            _statusText.text = $"Hand tracking has not started...";
        }
    }

    void AddAndUpdateHandKeyPoint(FingerKeyPoint[] fingers)
    {
        foreach(FingerKeyPoint finger in fingers)
        {
            if(!cacheKeyPoints.TryGetValue(finger.Name, out GameObject fingerKeyPoint))
            {
                fingerKeyPoint = Instantiate(keyPointPrefab, finger.KeyPoint.Position, Quaternion.identity);
                cacheKeyPoints.Add(finger.Name, fingerKeyPoint);
            }
            else 
            {
                fingerKeyPoint.transform.position = finger.KeyPoint.Position;
            }
        }
    }
}