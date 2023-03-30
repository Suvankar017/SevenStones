using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditorInternal;
using commanastationwww.eternaltemple;
using UnityEditor;

public class IDCWTFISTHIS : MonoBehaviour
{

    public enum ModifyMode
    {
        Create,
        Replace,
        RemoveFirstBrackets,
        SwapInScene,
        LoadNewPrefabs
    }

    public struct PrefabInfo
    {
        public Transform Prefab;
        public List<Mesh> Meshes;

        public PrefabInfo(Transform prefab)
        {
            Prefab = prefab;
            Meshes = new List<Mesh>();
        }

        public bool Contains(Mesh mesh)
        {
            return Meshes.Contains(mesh);
        }
    }

    public ModifyMode Mode = ModifyMode.Create;
    public GameObject Obj = null;
    public bool SelectionMode = false;
    public string SavePath = "Assets/";

    public Transform Prefab;
    public Transform[] Prefabs;
    public Transform[] Meshes;
    public Transform[] NewPrefabs;
    private List<Mesh> PrefabMeshes = new List<Mesh>();
    private List<PrefabInfo> PrefabInfos = new List<PrefabInfo>();
    public GameObject[] SelectedObjs;

    public void LoadNewPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:prefab", new string[] { "Assets/Arts" });

        List<Transform> newPrefabsList = new List<Transform>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            newPrefabsList.Add(go.transform);
        }

        NewPrefabs = newPrefabsList.ToArray();
    }

    public void SwapInScene()
    {
        GameObject oldGO = new GameObject("Old Prefabs");
        oldGO.transform.localPosition = Vector3.zero;
        oldGO.transform.localEulerAngles = Vector3.zero;

        GameObject newGO = new GameObject("New Prefabs");
        newGO.transform.localPosition = Vector3.zero;
        newGO.transform.localEulerAngles = Vector3.zero;

        foreach (Transform oldPrefab in Prefabs)
        {
            foreach (Transform newPrefab in NewPrefabs)
            {
                if (oldPrefab.name == newPrefab.name)
                {
                    Transform t = PrefabUtility.InstantiatePrefab(newPrefab) as Transform;
                    t.name = newPrefab.name;
                    t.localPosition = oldPrefab.transform.localPosition;
                    t.localEulerAngles = oldPrefab.localEulerAngles;
                    t.SetParent(newGO.transform);
                    oldPrefab.SetParent(oldGO.transform);
                    oldPrefab.gameObject.SetActive(false);
                    break;
                }
            }
        }
    }

    public void RemoveFirstBracket()
    {
        if (SelectionMode)
        {
            foreach (GameObject selectedObj in SelectedObjs)
            {
                if (!PrefabUtility.IsPartOfAnyPrefab(selectedObj))
                    continue;

                Transform parent = selectedObj.transform;
                string[] splitedNames = selectedObj.name.Split(' ');
                selectedObj.name = splitedNames[0];
                /*for (int i = 0; i < parent.childCount; i++)
                {
                    Transform child = parent.GetChild(i);
                    string[] splitedNames = child.name.Split(' ');
                    child.name = splitedNames[0];
                }*/
            }
        }
        else
        {
            foreach (Transform prefab in Prefabs)
            {
                Transform parent = prefab.transform;
                string[] splitedNames = prefab.name.Split(' ');
                prefab.name = splitedNames[0];
                /*for (int i = 0; i < parent.childCount; i++)
                {
                    Transform child = parent.GetChild(i);
                    string[] splitedNames = child.name.Split(' ');
                    child.name = splitedNames[0];
                }*/
            }
        }
    }

    public void Replacer(Action onDone)
    {
        PrefabInfos.Clear();

        foreach (Transform t in NewPrefabs)
        {
            PrefabInfo info = new PrefabInfo(t);
            TraverseTransform(t, info.Meshes);
            PrefabInfos.Add(info);
        }

        if (SelectionMode)
        {
            foreach (GameObject g in SelectedObjs)
            {
                g.transform.localPosition = Vector3.zero;
                g.transform.localEulerAngles = Vector3.zero;

                GameObject newGO = new GameObject(g.name);
                newGO.transform.localPosition = Vector3.zero;
                newGO.transform.localEulerAngles = Vector3.zero;

                for (int i = 0; i < g.transform.childCount; i++)
                {
                    ReplacerInternal(g.transform.GetChild(i), newGO.transform);
                }

                Obj = newGO;
                onDone();
            }
        }
        else
        {
            foreach (Transform prefab in Prefabs)
            {
                ReplacerInternal(prefab, null);
            }
        }
    }

    private void ReplacerInternal(Transform prefab, Transform parent)
    {
        string[] names = prefab.name.Split('_');
        if (names[0] == "Walls")
        {
            foreach (PrefabInfo info in PrefabInfos)
            {
                if (info.Prefab.name != prefab.name)
                    continue;

                GameObject go = PrefabUtility.InstantiatePrefab(info.Prefab.gameObject) as GameObject;
                go.transform.localPosition = prefab.position;
                go.transform.localEulerAngles = prefab.eulerAngles;
                go.transform.SetParent(parent);
                break;
            }
            return;
        }

        if (prefab.TryGetComponent(out MeshFilter filter))
        {
            foreach (PrefabInfo info in PrefabInfos)
            {
                if (!info.Contains(filter.sharedMesh))
                    continue;

                GameObject go = PrefabUtility.InstantiatePrefab(info.Prefab.gameObject) as GameObject;
                go.transform.localPosition = prefab.position;
                go.transform.localEulerAngles = prefab.eulerAngles;
                go.transform.SetParent(parent);
                break;
            }
        }
        else
        {
            if (prefab.GetChild(0).TryGetComponent(out filter))
            {
                foreach (PrefabInfo info in PrefabInfos)
                {
                    if (!info.Contains(filter.sharedMesh))
                        continue;

                    GameObject go = PrefabUtility.InstantiatePrefab(info.Prefab.gameObject) as GameObject;
                    go.transform.localPosition = prefab.position;
                    go.transform.localEulerAngles = prefab.eulerAngles;
                    go.transform.SetParent(parent);
                    break;
                }
            }
        }
    }

    public void Hello(Action onDone)
    {
        Obj = null;

        Ray ray = new Ray(new Vector3(0.0f, 1000.0f, 0.0f), Vector3.down);
        Vector3 hitPoint = Vector3.zero;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            hitPoint = hit.point;
        }

        if (SelectionMode)
        {
            PrefabMeshes.Clear();
            HelloInternal(Prefab, hitPoint);
            onDone();
        }
        else
        {
            if (Prefabs != null)
            {
                foreach (Transform p in Prefabs)
                {
                    Obj = null;
                    PrefabMeshes.Clear();
                    HelloInternal(p, hitPoint);
                    onDone();
                }
            }
        }
        
        Debug.Log("Computation Successful!!!");
    }

    private void HelloInternal(Transform prefab, Vector3 hitPoint)
    {
        TraverseTransform(prefab, PrefabMeshes);

        foreach (Transform mesh in Meshes)
        {
            List<Mesh> currentFBXMeshes = new List<Mesh>();
            TraverseTransform(mesh, currentFBXMeshes);

            if (PrefabMeshes.Exists((m) => currentFBXMeshes.Contains(m)))
            {
                bool hasHideGroupComponent = false;

                Transform t = Instantiate(mesh, hitPoint, Quaternion.identity);
                t.name = mesh.name;

                Component[] components = prefab.GetComponents<Component>();
                foreach (Component c in components)
                {
                    if (c is Transform) continue;
                    if (c is MeshFilter || c is MeshRenderer) continue;

                    if (ComponentUtility.CopyComponent(c))
                        ComponentUtility.PasteComponentAsNew(t.gameObject);

                    if (c is Collider)
                        (c as Collider).material = null;

                    if (c is HideGroup)
                        hasHideGroupComponent = true;
                }

                if (hasHideGroupComponent)
                {
                    TraverseTransform(t, null, (t, p) =>
                    {
                        if (p)
                        {
                            t.gameObject.AddComponent<HideablePart>();
                        }
                    });
                }

                Obj = t.gameObject;
                Obj.isStatic = true;
                break;
            }
        }
    }

    private void TraverseTransform(Transform t, List<Mesh> meshes)
    {
        MeshFilter mf = t.GetComponent<MeshFilter>();
        if (mf != null)
            if (!meshes.Contains(mf.sharedMesh))
                meshes.Add(mf.sharedMesh);

        for (int i = 0; i < t.childCount; i++)
        {
            TraverseTransform(t.GetChild(i), meshes);
        }
    }

    private void TraverseTransform(Transform t, Transform parent, Action<Transform, Transform> onEvaluate)
    {
        onEvaluate(t, parent);

        for (int i = 0; i < t.childCount; i++)
        {
            TraverseTransform(t.GetChild(i), t, onEvaluate);
        }
    }

    private Component CopyComponent(Component original, GameObject destination)
    {
        Type type = original.GetType();
        Component copy = destination.AddComponent(type);

        // Copy fields
        Debug.Log(type.Name);
        PropertyInfo[] properties = type.GetProperties();
        foreach (PropertyInfo property in properties)
        {
            if (property.CanWrite)
            {
                try
                {
                    Debug.Log(property.Name + " : " + property.GetValue(original));
                    property.SetValue(copy, property.GetValue(original));
                }
                catch
                {

                }
            }
        }

        FieldInfo[] fields = type.GetFields();
        foreach (FieldInfo field in fields)
        {
            Debug.Log(field.Name + " : " + field.GetValue(original));
            field.SetValue(copy, field.GetValue(original));
        }

        return copy;
    }

}
