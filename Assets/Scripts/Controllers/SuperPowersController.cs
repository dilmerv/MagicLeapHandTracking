using System.Collections;
using System.Collections.Generic;
using MagicLeap;
using MagicLeapHandTracking.Assets.Scripts.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;

public class SuperPowersController : MonoBehaviourSingleton<SuperPowersController>
{
        [SerializeField, Tooltip("Text to display gesture status to.")]
        private Text _statusText = null;

        [SerializeField, Tooltip("Text to display the total score.")]
        private Text _scoreText = null;

        [SerializeField]
        private float KeyPoseConfidenceValue = 0.6f;

        [SerializeField]
        private GameObject leftHandSphere, rightHandSphere;

        [SerializeField, Tooltip("Prefab to use when spawning an object.")]
        private GameObject spawnPrefab;

        [SerializeField, Tooltip("Force to use when spawning.")]
        private float spawnForce = 10.0f;

        [SerializeField, Tooltip("How long to wait in seconds until destroying spawned game object.")]
        private float destroySpawnAfterSeconds = 10.0f;

        [SerializeField, Tooltip("How long to wait in until next spawned game object is allowed.")]    
        private float delayNextSpawnBy = 1.0f;

        // Timer to track spawn delays
        private float spawnTimer = 0;

        private bool canSpawn = false;

        private Transform superPowersRaycastVisualizerTransform = null;
        void Awake()
        {   
            if(_statusText == null)
            {
                Debug.LogError("Status and Mute State Text needs to be set");
                enabled = false;
                return;
            }

            superPowersRaycastVisualizerTransform = SuperPowersRaycastVisualizer.Instance.transform;
        }

        void Update()
        {
            #if UNITY_EDITOR
                if(Input.GetKeyDown(KeyCode.A))
                {
                    Debug.Log("Spawning new game object");
                    Spawn(Vector3.zero);
                }
            #endif

            if (MLHands.IsStarted)
            {
                _statusText.text = string.Format(
                    "Hand Gestures\nLeft: {0}, {2}% confidence\nRight: {1}, {3}% confidence, LHC {4}, RHC {5}",
                    MLHands.Left.KeyPose.ToString(),
                    MLHands.Right.KeyPose.ToString(),
                    (MLHands.Left.KeyPoseConfidence * 100.0f).ToString("n0"),
                    (MLHands.Right.KeyPoseConfidence * 100.0f).ToString("n0"),
                    MLHands.Left.KeyPoseConfidence.ToString(),
                    MLHands.Right.KeyPoseConfidence.ToString());
                
                _scoreText.text = $"{ScoreTracker.TotalPoints.ToString("n0")}";

                leftHandSphere.transform.position = MLHands.Left.Center;
                rightHandSphere.transform.position = MLHands.Right.Center;
                
                if(MLHands.Left.KeyPose.IsOpenHand() && MLHands.Left.KeyPoseConfidence >= KeyPoseConfidenceValue)
                    Spawn(MLHands.Left.Center);
                if(MLHands.Right.KeyPose.IsOpenHand() && MLHands.Right.KeyPoseConfidence >= KeyPoseConfidenceValue)
                    Spawn(MLHands.Right.Center);
            }

            // spawnTimer tracking
            if(spawnTimer >= delayNextSpawnBy)
                canSpawn = true;
            else
                spawnTimer += Time.deltaTime;
        }

        void Spawn(Vector3 position)
        {
            if(!canSpawn)
                return;
                
            canSpawn = false;

            AudioController.Instance.PlayPitch();
            
            GameObject spawnObject = Instantiate(spawnPrefab, position, superPowersRaycastVisualizerTransform.rotation);
            Rigidbody spawnObjectRigidBody = spawnObject.GetComponent<Rigidbody>();
            spawnObjectRigidBody.AddForce((-superPowersRaycastVisualizerTransform.forward) * spawnForce);
            Destroy(spawnObject, destroySpawnAfterSeconds);
            
            spawnTimer = 0;
        }
}
