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

using UnityEngine;
using MagicLeap.UI;

/// <summary>
/// This is an example class that demonstrates how to visualize the input selection and cursor locations from the MLInputModule.
/// </summary>
public class InputExample : MonoBehaviour
{
    #region Private Variables
    [SerializeField, Tooltip("The MLInputModule used in the scene.")]
    private MLInputModule _inputModule = null;

    [SerializeField, Tooltip("The material that should be applied to MLInputModule line segments.")]
    private Material _beamMaterial = null;

    [SerializeField, Tooltip("The prefab that will represent a visual cursor.")]
    private GameObject _cursorPrefab = null;

    private const int MAX_CONTROLLERS = 1;
    private LineRenderer[] _beams = new LineRenderer[MAX_CONTROLLERS];
    private GameObject _cursorHead = null;
    private GameObject[] _cursorControls = new GameObject[MAX_CONTROLLERS];
    #endregion

    #region Unity Methods
    void Awake()
    {
        // Create Beams
        for (int i = 0; i < MAX_CONTROLLERS; i++)
        {
            GameObject laserObject = new GameObject();
            laserObject.name = "Laser: " + i;

            _beams[i] = laserObject.AddComponent<LineRenderer>();
            _beams[i].material = _beamMaterial;
            _beams[i].startWidth = 0.01f;
            _beams[i].endWidth = 0.01f;
        }

        // Instantiate Cursors GameObjects
        _cursorHead = Instantiate(_cursorPrefab);
        _cursorHead.name = "Gaze Cursor";

        for (int i = 0; i < MAX_CONTROLLERS; i++)
        {
            _cursorControls[i] = Instantiate(_cursorPrefab);
            _cursorControls[i].name = "Control: " + i;
        }
    }

    void Update()
    {
        // Update Gaze Cursor
        _cursorHead.SetActive(_inputModule.GazeLineSegment.End.HasValue);
        if (_inputModule.GazeLineSegment.End.HasValue)
        {
            _cursorHead.transform.position = _inputModule.GazeLineSegment.End.Value;
            _cursorHead.transform.localRotation = Quaternion.LookRotation(_inputModule.GazeLineSegment.Normal, _cursorHead.transform.up);
        }

        // Update Control(s) Cursor and Beam
        for (int i = 0; i < MAX_CONTROLLERS; i++)
        {
            _beams[i].gameObject.SetActive(_inputModule.ControlsLineSegment[i].End.HasValue);
            _cursorControls[i].SetActive(_inputModule.ControlsLineSegment[i].End.HasValue);

            if (_inputModule.ControlsLineSegment[i].End.HasValue)
            {
                _beams[i].SetPosition(0, _inputModule.ControlsLineSegment[i].Start);
                _beams[i].SetPosition(1, _inputModule.ControlsLineSegment[i].End.Value);

                _cursorControls[i].transform.position = _inputModule.ControlsLineSegment[i].End.Value;
                if (_inputModule.ControlsLineSegment[i].Normal != Vector3.zero)
                {
                    _cursorControls[i].transform.localRotation = Quaternion.LookRotation(_inputModule.ControlsLineSegment[i].Normal, _cursorControls[i].transform.up);
                }
            }
        }
    }
    #endregion
}
