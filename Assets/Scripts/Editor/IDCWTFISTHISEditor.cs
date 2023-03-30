using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(IDCWTFISTHIS))]
public class IDCWTFISTHISEditor : Editor
{
    private IDCWTFISTHIS _targetScript = null;

    private void OnEnable()
    {
        _targetScript = target as IDCWTFISTHIS;
    }

    public override void OnInspectorGUI()
    {
        if (Selection.activeGameObject)
            _targetScript.Prefab = Selection.activeGameObject.transform;
        if (Selection.gameObjects != null)
            _targetScript.SelectedObjs = Selection.gameObjects;

        base.OnInspectorGUI();

        if (GUILayout.Button("Generate"))
        {
            ClearConsole();
            
            switch (_targetScript.Mode)
            {
                case IDCWTFISTHIS.ModifyMode.Create:
                    _targetScript.Hello(OnDone);
                    break;

                case IDCWTFISTHIS.ModifyMode.Replace:
                    _targetScript.Replacer(OnDone);
                    break;

                case IDCWTFISTHIS.ModifyMode.RemoveFirstBrackets:
                    _targetScript.RemoveFirstBracket();
                    break;

                case IDCWTFISTHIS.ModifyMode.SwapInScene:
                    _targetScript.SwapInScene();
                    break;

                case IDCWTFISTHIS.ModifyMode.LoadNewPrefabs:
                    _targetScript.LoadNewPrefabs();
                    break;
            }
        }

        if (GUILayout.Button("Instantiate"))
        {
            PrefabUtility.InstantiatePrefab(_targetScript.Prefab);
        }
    }

    private void OnDone()
    {
        if (_targetScript.Obj == null)
            return;

        Vector3 prev = _targetScript.Obj.transform.position;
        _targetScript.Obj.transform.position = Vector3.zero;

        DirectoryInfo info = new DirectoryInfo(_targetScript.SavePath);
        info.Create();

        PrefabUtility.SaveAsPrefabAssetAndConnect(_targetScript.Obj, _targetScript.SavePath + _targetScript.Obj.name + ".prefab", InteractionMode.UserAction);
        
        _targetScript.Obj.transform.position = prev;
        DestroyImmediate(_targetScript.Obj);
        _targetScript.Obj = null;
    }

    public void ClearConsole()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(Editor));
        Type type = assembly.GetType("UnityEditor.LogEntries");
        MethodInfo method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }
}
