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

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildPrepocesor : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildReport report)
    {
        if(report.summary.platform == BuildTarget.Lumin)
        {
            if(PlayerSettings.colorSpace != ColorSpace.Linear)
            {
                Debug.Log("BuildPreprocesor changing PlayerSettings.colorSpace to ColorSpace.Linear");
                PlayerSettings.colorSpace = ColorSpace.Linear;
            }

            if(PlayerSettings.stereoRenderingPath != StereoRenderingPath.Instancing)
            {
                Debug.Log("BuildPreprocesor changing PlayerSettings.stereoRenderingPath to StereoRenderingPath.Instancing");
                PlayerSettings.stereoRenderingPath = StereoRenderingPath.Instancing;
            }
        }
    }
}
