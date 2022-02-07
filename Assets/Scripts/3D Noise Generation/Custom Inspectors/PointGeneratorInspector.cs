using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NoiseGenerator))]
public class PointGeneratorInspector : Editor
{
    //Functions for undo / redo
    private NoiseGenerator _noiseGenerator;

    private void OnEnable()
    {
        _noiseGenerator = target as NoiseGenerator;
        Undo.undoRedoPerformed += RefreshGenerator;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= RefreshGenerator;
    }

    private void RefreshGenerator()
    {
        if (Application.isPlaying)
        {
            _noiseGenerator.GeneratePoints();
        }
    }


    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck() && Application.isPlaying)
        {
            RefreshGenerator();
        }
    }
}
