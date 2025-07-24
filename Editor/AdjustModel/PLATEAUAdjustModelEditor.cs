using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class UVTransferByNearestVertex : EditorWindow
{
    private MeshFilter lod1Filter;   // UV とマテリアルを書き換えたい低解像度メッシュ
    private MeshFilter lod2Filter;   // 参照元（UV & テクスチャ）

    private string outputMeshPath = "Assets/LOD1_With_LOD2_UV.asset";
    private string outputMatPath = "Assets/LOD1_With_LOD2_Mat.mat";

    [MenuItem("Tools/LOD/Transfer UV + Texture (Nearest Vertex)")]
    private static void ShowWindow() => GetWindow<UVTransferByNearestVertex>("Transfer UV+Tex");

    private void OnGUI()
    {
        GUILayout.Label("LOD UV + Texture Transfer", EditorStyles.boldLabel);

        lod1Filter = (MeshFilter)EditorGUILayout.ObjectField("LOD1 MeshFilter (Target)",
            lod1Filter, typeof(MeshFilter), true);
        lod2Filter = (MeshFilter)EditorGUILayout.ObjectField("LOD2 MeshFilter (Source)",
            lod2Filter, typeof(MeshFilter), true);

        outputMeshPath = EditorGUILayout.TextField("Output Mesh Asset Path", outputMeshPath);
        outputMatPath = EditorGUILayout.TextField("Output Material Asset Path", outputMatPath);

        GUI.enabled = (lod1Filter && lod2Filter);
        if (GUILayout.Button("Transfer Now"))
        {
            Transfer();
        }

        if (GUILayout.Button("Copy material"))
        {
        }
        GUI.enabled = true;
    }

    // -------------------------------------------------------------

    private void Transfer()
    {
        if (!lod1Filter || !lod2Filter) return;

        Mesh lod1MeshOrig = lod1Filter.sharedMesh;
        Mesh lod2Mesh = lod2Filter.sharedMesh;
        if (!lod1MeshOrig || !lod2Mesh)
        {
            Debug.LogError("One of the MeshFilters has no mesh.");
            return;
        }

        // ---- 1. UV 転送 -----------------------------------------------------

        Vector3[] lod1WorldPos = GetWorldVerts(lod1MeshOrig, lod1Filter.transform);
        Vector3[] lod2WorldPos = GetWorldVerts(lod2Mesh, lod2Filter.transform);
        Vector2[] lod2UV = lod2Mesh.uv;

        SpatialHash<Vector2> hash = new SpatialHash<Vector2>(CellSize(lod2WorldPos));
        for (int i = 0; i < lod2WorldPos.Length; ++i) hash.Add(lod2WorldPos[i], lod2UV[i]);

        Vector2[] newUV = new Vector2[lod1WorldPos.Length];
        for (int i = 0; i < lod1WorldPos.Length; ++i)
        {
            if (!hash.TryGetNearest(lod1WorldPos[i], out newUV[i]))
                newUV[i] = Vector2.zero;          // フォールバック
        }

        Mesh newMesh = Instantiate(lod1MeshOrig);
        newMesh.name = lod1MeshOrig.name + "_WithLOD2UV";
        newMesh.uv = newUV;
        AssetDatabase.CreateAsset(newMesh, outputMeshPath);

        // ---- 2. マテリアル & テクスチャ複製 -------------------------------

        Renderer srcRend = lod2Filter.GetComponent<Renderer>();
        Renderer destRend = lod1Filter.GetComponent<Renderer>();

        if (srcRend && destRend && srcRend.materials.Length > 0)
        {
            destRend.materials = srcRend.materials.Select(m =>
             {
                 Material newMat = new Material(m);
                 newMat.mainTexture = m.mainTexture;
                 return newMat;
             }).ToArray();
        }
        //if (srcRend && destRend && srcRend.sharedMaterial)
        //{
        //    Material srcMat = srcRend.sharedMaterial;
        //    Material newMat = new Material(srcMat)   // 同じシェーダ & プロパティをコピー
        //    {
        //        name = srcMat.name + "_CopyForLOD1"
        //    };

        //    // ここでは MainTexture だけを想定（必要なら他の属性もコピー）
        //    newMat.mainTexture = srcMat.mainTexture;

        //    AssetDatabase.CreateAsset(newMat, outputMatPath);
        //    destRend.sharedMaterial = newMat;
        //}

        // ---- 3. LOD1 オブジェクトへ適用 ------------------------------------

        lod1Filter.sharedMesh = newMesh;
        EditorUtility.SetDirty(lod1Filter.gameObject);

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Transfer UV+Tex", "Finished!\nMesh: "
            + outputMeshPath + "\nMaterial: " + outputMatPath, "OK");
    }

    // ----------------  Helper -------------------------------------------------

    private static Vector3[] GetWorldVerts(Mesh m, Transform t)
    {
        Vector3[] v = m.vertices;
        Vector3[] wv = new Vector3[v.Length];
        Matrix4x4 M = t.localToWorldMatrix;
        for (int i = 0; i < v.Length; ++i) wv[i] = M.MultiplyPoint3x4(v[i]);
        return wv;
    }

    private static float CellSize(Vector3[] pts)
    {
        Bounds b = new Bounds(pts[0], Vector3.zero);
        foreach (var p in pts) b.Encapsulate(p);
        return Mathf.Max(b.size.x, b.size.y, b.size.z) / 10f + 1e-5f;
    }

    // 超簡易 Spatial Hash ------------------------------------------------------

    private class SpatialHash<T>
    {
        private readonly float cell;
        private readonly Dictionary<Vector3Int, List<(Vector3 pos, T val)>> tbl
            = new Dictionary<Vector3Int, List<(Vector3, T)>>();

        public SpatialHash(float cellSize) => cell = Mathf.Max(cellSize, 1e-5f);

        public void Add(Vector3 p, T v)
        {
            var key = Hash(p);
            if (!tbl.TryGetValue(key, out var list))
                tbl[key] = list = new List<(Vector3, T)>();
            list.Add((p, v));
        }

        public bool TryGetNearest(Vector3 p, out T v)
        {
            float best = float.MaxValue; v = default;
            var baseK = Hash(p);
            for (int x = -1; x <= 1; ++x)
                for (int y = -1; y <= 1; ++y)
                    for (int z = -1; z <= 1; ++z)
                    {
                        var k = new Vector3Int(baseK.x + x, baseK.y + y, baseK.z + z);
                        if (!tbl.TryGetValue(k, out var list)) continue;
                        foreach (var (pos, val) in list)
                        {
                            float d = (p - pos).sqrMagnitude;
                            if (d < best) { best = d; v = val; }
                        }
                    }
            return best < float.MaxValue;
        }

        private Vector3Int Hash(Vector3 p)
            => new Vector3Int(Mathf.FloorToInt(p.x / cell),
                              Mathf.FloorToInt(p.y / cell),
                              Mathf.FloorToInt(p.z / cell));
    }
}