using System.Collections.Generic;
using MagicLeapHandTracking.Assets.Scripts.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;
using System.Linq;

[RequireComponent(typeof(HandTracking))]
public class HandTrackingWithKeyPointsController : MonoBehaviourSingleton<HandTrackingWithKeyPointsController>
{

    [SerializeField, Tooltip("Text to display gesture status to.")]
    private Text _statusText = null;

    [SerializeField]
    private GameObject keyPointPrefab;

    private Dictionary<string, GameObject> cacheKeyPoints;
    
    void Awake()
    {   
        if(_statusText == null)
        {
            Debug.LogError("Status and Mute State Text needs to be set");
            enabled = false;
            return;
        }

        cacheKeyPoints = new Dictionary<string, GameObject>();
    }

    void Update()
    {
        if (MLHands.IsStarted)
        {
            _statusText.text = $"Hand Key Points: {string.Join(",", cacheKeyPoints.Keys.Select(s => s).ToArray())}";

            AddAndUpdateHandKeyPoint(new FingerKeyPoint[] {
                // left hand key points
                new FingerKeyPoint { Name = $"HandLeft.{nameof(MLHands.Left.Pinky)}", KeyPoint = MLHands.Left.Pinky.KeyPoints[0]},
                new FingerKeyPoint { Name = $"HandLeft.{nameof(MLHands.Left.Ring)}", KeyPoint = MLHands.Left.Ring.KeyPoints[0]},
                new FingerKeyPoint { Name = $"HandLeft.{nameof(MLHands.Left.Middle)}", KeyPoint = MLHands.Left.Middle.KeyPoints[0]},
                new FingerKeyPoint { Name = $"HandLeft.{nameof(MLHands.Left.Index)}", KeyPoint = MLHands.Left.Index.KeyPoints[0]},
                new FingerKeyPoint { Name = $"HandLeft.{nameof(MLHands.Left.Thumb)}", KeyPoint = MLHands.Left.Thumb.KeyPoints[0]},
                
                // right hand key points
                new FingerKeyPoint { Name = $"HandRight.{nameof(MLHands.Right.Pinky)}", KeyPoint = MLHands.Right.Pinky.KeyPoints[0]},
                new FingerKeyPoint { Name = $"HandRight.{nameof(MLHands.Right.Ring)}", KeyPoint = MLHands.Right.Ring.KeyPoints[0]},
                new FingerKeyPoint { Name = $"HandRight.{nameof(MLHands.Right.Middle)}", KeyPoint = MLHands.Right.Middle.KeyPoints[0]},
                new FingerKeyPoint { Name = $"HandRight.{nameof(MLHands.Right.Index)}", KeyPoint = MLHands.Right.Index.KeyPoints[0]},
                new FingerKeyPoint { Name = $"HandRight.{nameof(MLHands.Right.Thumb)}", KeyPoint = MLHands.Right.Thumb.KeyPoints[0]}
            });
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

    public class FingerKeyPoint 
    {
        public string Name { get; set; }
        public MLKeyPoint KeyPoint { get; set; }
    }
}