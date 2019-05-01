// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using System.Collections;

namespace UnityEngine.XR.MagicLeap
{
    /// <summary>
    /// This is an obsolete component and will be removed in a future release. Please use MLPersistentBehavior.
    /// </summary>
    [Obsolete("This component is obsolete and is superseded with MLPersistentBehavior.", true)]
    public class MLPersistentPoint : MonoBehaviour
    {
        #region Public Variables
        /// <summary>
        /// Every persistent point in your project must have a unique Id
        /// </summary>
        [Tooltip("Unique id for this persistent point. If not provided the name of the GameObject would be used")]
        public string UniqueId;

        /// <summary>
        /// Retry timeout.
        /// <summary/>
        public float TimeOutInSeconds = 5.0f;

        /// <summary>
        /// Delay to retry when failing to restore
        /// </summary>
        public float RetryDelayInSeconds = 2.0f;

        /// <summary>
        /// Number of times to retry when failing to restore
        /// </summary>
        public int NumRetriesForRestore = 3;
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets the binding.
        /// </summary>
        /// <value>The binding.</value>
        public MLContentBinding Binding { get; private set; }
        #endregion

        #region Public Events
        /// <summary>
        /// This event is raised, with true, when the content is restored or saved, on create, successfully.
        /// This is raised, with false, when the content failed to restore or save, on create.
        /// This event is only raised once. Event Listeners should be bound before Start().
        /// </summary>
        public event System.Action<bool> OnComplete;

        /// <summary>
        /// This event happens when there are errors. The parameter indicates the specific error
        /// that happened at the point of failure.
        /// </summary>
        public event System.Action<MLResult> OnError;
        #endregion

        #region Private Variables
        bool _done = false;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Start up
        /// Note: This requires the privilege to be granted prior to Start()
        /// </summary>
        void Start()
        {
            MLResult result = MLPersistentStore.Start();
            if (!result.IsOk)
            {
                Debug.LogErrorFormat("MLPersistentPoint failed starting MLPersistentStore, disabling script. Reason: {0}", result);
                enabled = false;
                return;
            }

            result = MLPersistentCoordinateFrames.Start();
            if (!result.IsOk)
            {
                MLPersistentStore.Stop();
                Debug.LogErrorFormat("MLPersistentPoint failed starting MLPersistentCoordinateFrames, disabling script. Reason: {0}", result);
                enabled = false;
                return;
            }

            if (string.IsNullOrEmpty(UniqueId))
            {
                Debug.LogWarning("Unique Id is empty will try to use game object's name. It's good to provide a unique id for virtual objects to avoid weird behavior.");
                if (string.IsNullOrEmpty(gameObject.name))
                {
                    SetError(new MLResult(MLResultCode.UnspecifiedFailure, "Either UniqueId or name should be non empty. Disabling component"));
                    enabled = false;
                    return;
                }
                UniqueId = gameObject.name;
            }
            else
            {
                gameObject.name = UniqueId;
            }

            if (MLPersistentCoordinateFrames.IsReady)
            {
                RestoreBinding(gameObject.name);
            }
            else
            {
                MLPersistentCoordinateFrames.OnReady += HandleReady;
            }
        }

        /// <summary>
        /// Clean Up
        /// </summary>
        private void OnDestroy()
        {
            if (MLPersistentStore.IsStarted)
            {
                MLPersistentStore.Stop();
            }
            if (MLPersistentCoordinateFrames.IsStarted)
            {
                MLPersistentCoordinateFrames.Stop();
                MLPersistentCoordinateFrames.OnReady -= HandleReady;
            }
        }
        #endregion // Unity Methods

        #region Private Methods
        /// <summary>
        /// Utility function that shows the error and also raises the OnErrorEvent
        /// </summary>
        /// <param name="result">result to be shown.</param>
        void SetError(MLResult result)
        {
            Debug.LogError(result);
            if (OnError != null)
            {
                OnError(result);
            }
        }

        /// <summary>
        /// Tries to restore the binding or find closest PCF.
        /// </summary>
        void RestoreBinding(string objId)
        {
            if (MLPersistentStore.Contains(objId))
            {
                MLContentBinding binding;

                MLResult result = MLPersistentStore.Load(objId, out binding);
                if (!result.IsOk)
                {
                    SetError(new MLResult(result.Code,
                        string.Format("Error: MLPersistentPoint failed to load binding. Reason: {0}", result)));
                    SetComplete(false);
                }
                else
                {
                    Binding = binding;
                    Binding.GameObject = this.gameObject;
                    MLContentBinder.Restore(Binding, HandleBindingRestore);
                }
            }
            else
            {
                StartCoroutine(BindToClosestPCF());
            }
        }

        /// <summary>
        /// Finds the closest pcf for this persistent point.
        /// </summary>
        IEnumerator BindToClosestPCF()
        {
            float timeoutInSeconds = TimeOutInSeconds;
            while (timeoutInSeconds > 0.0f)
            {
                yield return StartCoroutine(TryBindingToClosestPCF());
                if (_done)
                {
                    yield break;
                }
                yield return new WaitForSeconds(1.0f);
                timeoutInSeconds -= 1.0f;
            }

            SetError(new MLResult(MLResultCode.Timeout, "Error: MLPersistentPoint failed to bind to closest PCF. Reason: Timeout"));
            SetComplete(false);
        }

        /// <summary>
        /// Creates a binding to the closest PCF
        /// </summary>
        /// <returns>Must be executed as a Coroutine</returns>
        IEnumerator TryBindingToClosestPCF()
        {
            _done = false;

            MLResult returnResult = MLPersistentCoordinateFrames.FindClosestPCF(gameObject.transform.position, (result, returnPCF) =>
            {
                if (result.IsOk && returnPCF.CurrentResult == MLResultCode.Ok)
                {
                    Debug.Log("Binding to closest found PCF: " + returnPCF.CFUID);
                    Binding = MLContentBinder.BindToPCF(gameObject.name, gameObject, returnPCF);
                    MLPersistentStore.Save(Binding);
                    SetComplete(true);
                    _done = true;
                }
                else
                {
                    Debug.LogErrorFormat("Error: MLPersistentPoint failed to find closest PCF. Reason: {0}", result);
                    SetComplete(false);
                    _done = true;
                }
            });

            if (!returnResult.IsOk)
            {
                // Technically, if we reach this point, the system had a problem
                Debug.LogErrorFormat("Error: MLPersistentPoint failed to attempt to find closest PCF. Reason: {0}", returnResult);
                SetComplete(false);
                _done = true;
            }

            while (!_done)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Triggers OnComplete event with success or failure
        /// </summary>
        /// <param name="success">True if restored or saved successfully, false otherwise</param>
        void SetComplete(bool success)
        {
            if (OnComplete != null)
            {
                OnComplete(success);
            }
        }

        /// <summary>
        /// Try to restore after a delay
        /// </summary>
        /// <returns>IEnumerator for delay</returns>
        IEnumerator TryRestore()
        {
            yield return new WaitForSeconds(RetryDelayInSeconds);
            MLContentBinder.Restore(Binding, HandleBindingRestore);
        }
        #endregion // Private Methods

        #region Event Handlers
        /// <summary>
        /// Handler for binding restore
        /// </summary>
        /// <param name="contentBinding">Content binding.</param>
        /// <param name="resultCode">Result code.</param>
        void HandleBindingRestore(MLContentBinding contentBinding, MLResult result)
        {
            if (!result.IsOk)
            {
                if (NumRetriesForRestore > 0)
                {
                    NumRetriesForRestore--;
                    Debug.LogWarningFormat("Failed to restore: {0} - {1}. Retries left: {2}. Result Code: {3}",
                        gameObject.name, contentBinding.PCF.CFUID, NumRetriesForRestore, result);
                    StartCoroutine(TryRestore());
                }
                else
                {
                    Debug.LogErrorFormat("Failed to restore : {0} - {1}. Deleting Binding. Result code: {2}",
                        gameObject.name, contentBinding.PCF.CFUID, result);
                    MLPersistentStore.DeleteBinding(contentBinding);
                    SetComplete(false);
                }
            }
            else
            {
                SetComplete(true);
            }
        }

        /// <summary>
        /// Handler when MLPersistentCoordinateFrames becomes ready
        /// </summary>
        private void HandleReady()
        {
            MLPersistentCoordinateFrames.OnReady -= HandleReady;
            RestoreBinding(gameObject.name);
        }
        #endregion // Event Handlers

        #region Public Methods
        /// <summary>
        /// Saves the binding if transform has changed
        /// </summary>
        public void UpdateBinding()
        {
            if (transform.hasChanged)
            {
                // Note: this does not change the PCF bound to
                Binding.Update();
                MLPersistentStore.Save(Binding);
                transform.hasChanged = false;
            }
        }

        /// <summary>
        /// Destroys the binding
        /// Note: Game Object is still alive. It is the responsibility
        /// of the caller to deal with the Game Object
        /// </summary>
        public void DestroyBinding()
        {
            MLPersistentStore.DeleteBinding(Binding);
        }
        #endregion
    }
}
