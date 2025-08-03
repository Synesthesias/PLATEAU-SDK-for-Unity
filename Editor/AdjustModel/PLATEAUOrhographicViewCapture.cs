using PlasticGui.WorkspaceWindow.Locks;
using PLATEAU.CityInfo;
using PLATEAU.PolygonMesh;
using PLATEAU.RoadNetwork.Graph;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;

public class PLATEAUOrthographicViewCapture : EditorWindow
{
    private static List<PLATEAUInstancedCityModel> targetObjects = new();
    private GameObject triplanarTarget;
    private static float pixelsPerMeter = 1f;
    private static string savePath = "Assets/Screenshots/";
    private static float textureScale = 1.0f;
    private static float blendSharpness = 3.0f;
    private static float brightness = 1.0f;
    // �L���v�`�������e�N�X�`����ۑ�
    private static string lastFrontTexPath;
    private static string lastSideTexPath;
    private static string lastTopTexPath;
    private static CameraClearFlags cameraClearFlag = CameraClearFlags.Nothing;
    private static Color cameraClearColor = Color.black;
    //private static int layerIndex = 6;

    //private static Shader triplanarShader;
    enum Face
    {
        Front,
        Back,
        Left,
        Right,
        Top
    }


    class CaptureRequest
    {
        /// <summary>
        /// ��̖ʂɊւ��郊�N�G�X�g���
        /// </summary>
        public record FaceRequest
        {
            public string TexturePath { get; }

            public Vector3 Direction { get; }

            public Vector3 Up { get; }

            public Vector2Int ImageSize { get; }

            public Vector3 Coef { get; }

            public FaceRequest(Vector3 direction, Vector3 up, Vector2 imageSize)
            {
                Direction = direction;
                Up = up;
                ImageSize = imageSize.FloorToInt();
            }
        }

        /// <summary>
        /// ���b�V���R�[�h
        /// </summary>
        public string MeshCode { get; }

        /// <summary>
        /// �o�E���f�B���O�{�b�N�X
        /// </summary>
        public Bounds Bounds { get; }

        public Vector3 XCoef { get; }

        public Vector3 YCoef { get; }
        public Vector3 ZCoef { get; }

        public float PixelsPerMeter { get; }
        /// <summary>
        /// �e�ʂ��Ƃ̏��
        /// </summary>
        public IReadOnlyDictionary<Face, FaceRequest> Faces { get; }

        public CaptureRequest(string meshCode, Bounds bounds, float pixelsPerMeter)
        {
            MeshCode = meshCode;
            Bounds = bounds;
            PixelsPerMeter = pixelsPerMeter;

            var size = Bounds.size;
#if false
            XCoef = size.Zy().RevScaled().Axy(0f) * pixelsPerMeter;
            YCoef = size.Xz().RevScaled().Xay(0f) * pixelsPerMeter;
            ZCoef = size.Xy().RevScaled().Xya(0f) * pixelsPerMeter;
#endif
            XCoef = size.RevScaled();
            YCoef = size.RevScaled();
            ZCoef = size.RevScaled();
            Faces = new Dictionary<Face, FaceRequest>
            {
                [Face.Front] = new FaceRequest(Vector3.forward, Vector3.up, size.Xy()),
                [Face.Back] = new FaceRequest(Vector3.back, Vector3.up, size.Xy()),
                [Face.Left] = new FaceRequest(Vector3.left, Vector3.up, size.Zy()),
                [Face.Right] = new FaceRequest(Vector3.right, Vector3.up, size.Zy()),
                [Face.Top] = new FaceRequest(Vector3.up, Vector3.forward, size.Xz())
            };
        }
    }


    /// <summary>
    /// ���b�V���R�[�h���Ƃ̃��[�N�t�H���_
    /// </summary>
    /// <param name="meshCode"></param>
    /// <returns></returns>
    static string GetFolderPath(string meshCode)
    {
        return $"{savePath}/{meshCode}";
    }

    /// <summary>
    /// ���b�V���R�[�h/�ʂɂ��e�N�X�`���p�X
    /// </summary>
    /// <param name="meshCode"></param>
    /// <param name="face"></param>
    /// <returns></returns>
    static string GetTexturePath(string meshCode, Face face)
    {
        return $"{GetFolderPath(meshCode)}/{face.ToString()}.png";
    }


    /// <summary>
    /// ���b�V���R�[�h/�ʂɂ��Triplanar�}�e���A���p�X
    /// </summary>
    /// <param name="meshCode"></param>
    /// <param name="face"></param>
    /// <returns></returns>
    static string GetMaterialPath(string meshCode)
    {
        return $"{GetFolderPath(meshCode)}/{meshCode}Triplanar.mat";
    }

    [MenuItem("Tools/Orthographic View Capture")]
    public static void ShowWindow()
    {
        GetWindow<PLATEAUOrthographicViewCapture>("3�ʐ}�L���v�`�� & Triplanar�K�p");
    }

    /// <summary>
    /// PLATEAUInstancedCityModel�̒����̎q�̖��O����, �L���v�`���Ώۂ̃I�u�W�F�N�g���ǂ������擾����
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="meshCode"></param>
    /// <returns></returns>
    private static bool IsTargetBuilding(GameObject obj, out string meshCode)
    {
        meshCode = "";

        var names = obj.name.Split("_");
        if (names.Length < 2)
            return false;
        // meshCode_�^�C�v_XXXX_YY.gml�Ƃ����`��
        meshCode = names[0];
        var type = names[1];
        return type == "bldg";
    }

    class BuildingInfo
    {
        public string Name { get; }
        public Dictionary<float, GameObject> Lods { get; } = new Dictionary<float, GameObject>();

        public BuildingInfo(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// key   : MeshCode
    /// value : ���̃��b�V���R�[�h�̌������
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    private Dictionary<string, List<BuildingInfo>> GetBuildings(PLATEAUInstancedCityModel model)
    {
        var ret = new Dictionary<string, List<BuildingInfo>>();
        foreach (var tr in model.transform.GetChildren())
        {
            if (tr.gameObject.activeInHierarchy == false)
                continue;

            if (IsTargetBuilding(tr.gameObject, out var meshCode) == false)
            {
                continue;
            }

            ret[meshCode] = new List<BuildingInfo>();

            // key : gameObject��
            // value : { key : LOD�ԍ�, value : ����GameObject}
            Dictionary<string, Dictionary<float, GameObject>> objectLodTable = new();
            foreach (var child in tr.GetChildren())
            {
                var m = Regex.Match(child.name, @"LOD(\d+)");
                if (m.Success == false)
                {
                    continue;
                }

                var lod = int.Parse(m.Groups[1].Value);


                foreach (var c in child.GetChildren())
                {
                    var t = objectLodTable.GetValueOrCreate(c.name);
                    t[lod] = c.gameObject;
                }
            }


            foreach (var item in objectLodTable)
            {
                var b = new BuildingInfo(item.Key);
                foreach (var x in item.Value)
                {
                    b.Lods[x.Key] = x.Value;
                }
                ret[meshCode].Add(b);
            }
        }
        return ret;
    }

    private void SwitchLod1Visible(bool isLod1Visible)
    {
        foreach (var model in targetObjects)
        {
            var buildings = GetBuildings(model);
            foreach (var m in buildings.Values.SelectMany(x => x))
            {

                // LOD1��\������ꍇ
                if (isLod1Visible)
                {
                    var maxLod = m.Lods.Keys.Where(l => l <= 1).Max();
                    foreach (var x in m.Lods)
                    {
                        x.Value.SetActive(x.Key == maxLod);
                    }
                }
                // ��������Ȃ��ꍇ�͍ő��LOD��\������
                else
                {
                    var maxLod = m.Lods.Keys.Max();
                    foreach (var x in m.Lods)
                    {
                        x.Value.SetActive(x.Key == maxLod);
                    }
                }
            }
        }
    }

    private void Capture(Camera camera, PLATEAUInstancedCityModel model)
    {
        // ���ݕ\������Ă���, ��\���ɂ���Q�[���I�u�W�F�N�g
        var invisibleObjects = new HashSet<GameObject>();

        // �L���v�`�����I�������ɔ�\���ɖ߂��Q�[���I�u�W�F�N�g
        var afterInvisibleObjects = new HashSet<GameObject>();

        Vector3 boundMax = Vector3.one * float.MinValue;
        Vector3 boundMin = Vector3.one * float.MaxValue;


        Dictionary<string, GameObject> targetObjects = new Dictionary<string, GameObject>();

        SwitchLod1Visible(false);

        foreach (var tr in model.transform.GetChildren())
        {
            if (tr.gameObject.activeInHierarchy == false)
                continue;

            if (IsTargetBuilding(tr.gameObject, out var meshCode) == false)
            {
                // �Ώۂ̃Q�[���I�u�W�F�N�g�ȊO�͔�\����
                invisibleObjects.Add(tr.gameObject);
                continue;
            }

            // �����_�[�Ώۂ�������폜����
            targetObjects[meshCode] = tr.gameObject;

            //foreach (var child in tr.GetChildren())
            //{
            //    var m = Regex.Match(child.name, @"LOD(\d+)");
            //    if (m.Success == false)
            //    {
            //        continue;
            //    }

            //    var lod = int.Parse(m.Groups[1].Value);

            //    if (lod <= 1)
            //    {
            //        if (child.gameObject.activeInHierarchy)
            //        {
            //            invisibleObjects.Add(child.gameObject);
            //        }
            //    }
            //    else
            //    {
            //        if (child.gameObject.activeInHierarchy == false)
            //        {
            //            child.gameObject.SetActive(true);
            //            afterInvisibleObjects.Add(child.gameObject);
            //        }
            //    }
            //}
        }

        // ��U�S����\���ɂ���
        foreach (var obj in invisibleObjects)
            obj.SetActive(false);

        foreach (var obj in targetObjects.Values)
            obj.SetActive(false);

        foreach (var item in targetObjects)
        {
            // �\���ɖ߂�
            item.Value.SetActive(true);

            var bound = CalculateCombinedBounds(item.Value);
            CaptureOrthographicViews(item.Key, item.Value, camera);
        }

        foreach (var obj in invisibleObjects)
            obj.SetActive(true);

        foreach (var obj in afterInvisibleObjects)
            obj.SetActive(false);

        var buildings = GetBuildings(model);

        //foreach (var build in buildings)
        //{
        //    var matPath = GetMaterialPath(build.Key);
        //    var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        //    foreach (var b in build.Value)
        //    {
        //        var lod1 = b.Lods.GetValueOrDefault(1);
        //        if (lod1 == null)
        //            continue;
        //        var mesh = lod1.GetComponent<MeshRenderer>();
        //        if (mesh == null)
        //            continue;
        //        mesh.materials[0] = mat;
        //    }
        //}
    }

    private static void CaptureOrthographicViews(string meshCode, GameObject obj, Camera camera)
    {
        // �ۑ��f�B���N�g���̍쐬

        if (!Directory.Exists(GetFolderPath(meshCode)))
        {
            Directory.CreateDirectory(GetFolderPath(meshCode));
        }

        // �S�I�u�W�F�N�g�̓����o�E���f�B���O�{�b�N�X���v�Z
        Bounds combinedBounds = CalculateCombinedBounds(obj);

        var request = new CaptureRequest(meshCode, combinedBounds, pixelsPerMeter);

        // �e�N�X�`���̍쐬
        foreach (var f in request.Faces)
        {
            CaptureView(camera, request, f.Key);
        }

        // �}�e���A���̍쐬
        CreateTriplanarMaterial(request);


        // �A�Z�b�g�f�[�^�x�[�X���X�V
        AssetDatabase.Refresh();
    }

    private static string CaptureView(
        Camera camera
        , CaptureRequest req
        , Face face)
    {
        // �J�����̈ʒu�ƌ�����ݒ�
        var bounds = req.Bounds;
        var faceReq = req.Faces[face];

        var direction = faceReq.Direction;
        var up = faceReq.Up;
        var imageSize = faceReq.ImageSize;

        Vector3 center = bounds.center;
        float distance = Mathf.Abs(Vector3.Dot(bounds.size, direction));

        camera.transform.position = center + direction * (distance + 1f);
        camera.transform.LookAt(center, up);

        // ���𓊉e�̃T�C�Y��ݒ�
        camera.orthographicSize = imageSize.Min() * 0.5f;
        //camera.rect = cameraRect;
        // �j�A�N���b�v�ƃt�@�[�N���b�v��ݒ�
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = distance * 2f;

        // �����_�[�e�N�X�`�����쐬
        imageSize = (imageSize.ToVector2() * pixelsPerMeter).ToVector2Int();
        RenderTexture renderTexture = new RenderTexture(imageSize.x, imageSize.y, 24);
        camera.targetTexture = renderTexture;
        // �����_�����O���s
        camera.Render();

        // �e�N�X�`����ǂݎ��
        RenderTexture.active = renderTexture;
        Texture2D screenshot = new Texture2D(imageSize.x, imageSize.y, TextureFormat.RGBA32, false);
        screenshot.ReadPixels(new Rect(0, 0, imageSize.x, imageSize.y), 0, 0);
        screenshot.Apply();

        // �t�@�C���ɕۑ�
        byte[] data = screenshot.EncodeToPNG();
        string filePath = GetTexturePath(req.MeshCode, face);
        File.WriteAllBytes(filePath, data);

        // �N���[���A�b�v
        RenderTexture.active = null;
        camera.targetTexture = null;
        DestroyImmediate(renderTexture);
        DestroyImmediate(screenshot);

        Debug.Log($"{face}�ʐ}��ۑ����܂���: {filePath}");

        return filePath;
    }

    private void OnGUI()
    {
        GUILayout.Label("3�ʐ}�L���v�`�� & Triplanar�K�p�c�[��", EditorStyles.boldLabel);

        // �ΏۃI�u�W�F�N�g�̐ݒ�
        GUILayout.Label("�L���v�`���ΏۃI�u�W�F�N�g:", EditorStyles.boldLabel);

        if (GUILayout.Button("�I�𒆂̃I�u�W�F�N�g��ǉ�"))
        {
            foreach (GameObject obj in Selection.gameObjects)
            {
                var cityModel = obj.GetComponent<PLATEAUInstancedCityModel>();
                if (!cityModel)
                    continue;
                if (!targetObjects.Contains(cityModel))
                {
                    targetObjects.Add(cityModel);
                }
            }
        }

        using (var _ = new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Show LOD1"))
            {
                SwitchLod1Visible(true);
            }
            if (GUILayout.Button("Show LOD2"))
            {
                SwitchLod1Visible(false);
            }
        }


        // �I�u�W�F�N�g���X�g�̕\��
        for (int i = 0; i < targetObjects.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            targetObjects[i] = (PLATEAUInstancedCityModel)EditorGUILayout.ObjectField(targetObjects[i], typeof(PLATEAUInstancedCityModel), true);
            if (GUILayout.Button("�폜", GUILayout.Width(50)))
            {
                targetObjects.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("���X�g���N���A"))
        {
            targetObjects.Clear();
        }

        GUILayout.Space(10);

        // �L���v�`���ݒ�
        GUILayout.Label("�L���v�`���ݒ�:", EditorStyles.boldLabel);
        pixelsPerMeter = EditorGUILayout.FloatField("Pixels per Meter", pixelsPerMeter);
        savePath = EditorGUILayout.TextField("�ۑ��p�X", savePath);
        cameraClearFlag = (CameraClearFlags)EditorGUILayout.EnumPopup("Camera Clear Flag", cameraClearFlag);
        cameraClearColor = EditorGUILayout.ColorField("Camera Clear Color", cameraClearColor);
        //layerIndex = EditorGUILayout.IntField("Layer Index", layerIndex);
        //triplanarShader = (Shader)EditorGUILayout.ObjectField("TriplanarShader", triplanarShader, typeof(Shader), true);
        GUILayout.Space(10);

        // �L���v�`���{�^��
        if (GUILayout.Button("�L���v�`��"))
        {
            CaptureOrthographicViews();
        }

        GUILayout.Space(20);

        // Triplanar�K�p�ݒ�
        GUILayout.Label("Triplanar�K�p�ݒ�:", EditorStyles.boldLabel);
        triplanarTarget = (GameObject)EditorGUILayout.ObjectField("�K�p�ΏۃI�u�W�F�N�g", triplanarTarget, typeof(GameObject), true);

        textureScale = EditorGUILayout.FloatField("�e�N�X�`���X�P�[��", textureScale);
        blendSharpness = EditorGUILayout.Slider("�u�����h�V���[�v�l�X", blendSharpness, 1f, 10f);
        brightness = EditorGUILayout.Slider("���x", brightness, 0f, 2f);

        var boundingBox = CalculateCombinedBounds();
        EditorGUILayout.Vector3Field("�o�E���f�B���O�{�b�N�X[min]", boundingBox.min);
        EditorGUILayout.Vector3Field("�o�E���f�B���O�{�b�N�X[max]", boundingBox.max);
        EditorGUILayout.Vector3Field("�o�E���f�B���O�{�b�N�X[size]", boundingBox.size);
        EditorGUILayout.Vector3Field("�o�E���f�B���O�{�b�N�X[center]", boundingBox.center);

        var xy = (boundingBox.size.Xy() * pixelsPerMeter).FloorToInt();
        var zy = (boundingBox.size.Zy() * pixelsPerMeter).FloorToInt();
        var xz = (boundingBox.size.Xz() * pixelsPerMeter).FloorToInt();

        EditorGUILayout.Vector2Field("�T�C�Y[xy]", xy);
        EditorGUILayout.Vector2Field("�T�C�Y�t��[xy]", Vector2.one.RevScaled(xy.ToVector2()));
        EditorGUILayout.Vector2Field("�T�C�Y[zy]", zy);
        EditorGUILayout.Vector2Field("�T�C�Y�t��[zy]", Vector2.one.RevScaled(zy.ToVector2()));
        EditorGUILayout.Vector2Field("�T�C�Y[xz]", xz);
        EditorGUILayout.Vector2Field("�T�C�Y�t��[xz]", Vector2.one.RevScaled(xz.ToVector2()));

        GUI.enabled = triplanarTarget != null && !string.IsNullOrEmpty(lastFrontTexPath);
        if (GUILayout.Button("Triplanar���e��K�p"))
        {
            // ApplyTriplanarProjection();
        }
        GUI.enabled = true;

        if (triplanarTarget != null && string.IsNullOrEmpty(lastFrontTexPath))
        {
            EditorGUILayout.HelpBox("���3�ʐ}���L���v�`�����Ă�������", MessageType.Warning);
        }
    }

    private void CaptureOrthographicViews()
    {
        if (targetObjects.Count == 0)
        {
            EditorUtility.DisplayDialog("�G���[", "�ΏۃI�u�W�F�N�g���ݒ肳��Ă��܂���", "OK");
            return;
        }

        // �ۑ��f�B���N�g���̍쐬
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        // �S�I�u�W�F�N�g�̓����o�E���f�B���O�{�b�N�X���v�Z
        Bounds combinedBounds = CalculateCombinedBounds();

        // �ꎞ�I�ȃJ�������쐬
        GameObject cameraObj = new GameObject("TempOrthographicCamera");
        Camera camera = cameraObj.AddComponent<Camera>();
        camera.orthographic = true;
        camera.backgroundColor = cameraClearColor;
        camera.clearFlags = cameraClearFlag;
        //camera.cullingMask = 1 << layerIndex;

        try
        {
            foreach (var obj in targetObjects)
            {
                Capture(camera, obj);
            }
            // �A�Z�b�g�f�[�^�x�[�X���X�V
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("����", $"3�ʐ}�� {savePath} �ɕۑ����܂���", "OK");
        }
        finally
        {
            // �ꎞ�I�ȃJ�������폜
            DestroyImmediate(cameraObj);
        }
    }

    private Bounds CalculateCombinedBounds()
    {
        Bounds bounds = new Bounds();
        bool boundsInitialized = false;

        foreach (GameObject obj in targetObjects.Select(x => x.gameObject))
        {
            if (obj == null) continue;

            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (!boundsInitialized)
                {
                    bounds = renderer.bounds;
                    boundsInitialized = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }
        }

        return bounds;
    }

    private static Bounds CalculateCombinedBounds(GameObject obj)
    {
        Bounds bounds = new Bounds();
        bool boundsInitialized = false;
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (!boundsInitialized)
            {
                bounds = renderer.bounds;
                boundsInitialized = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }
        return bounds;
    }

    private static void CreateTriplanarMaterial(CaptureRequest req)
    {
        //if (triplanarTarget == null)
        //{
        //    EditorUtility.DisplayDialog("�G���[", "�K�p�ΏۃI�u�W�F�N�g���ݒ肳��Ă��܂���", "OK");
        //    return;
        //}

        //// Triplanar�V�F�[�_�[��ǂݍ���
        //if (triplanarShader == null)
        //{
        //    EditorUtility.DisplayDialog("�G���[", "TriplanarProjection�V�F�[�_�[��������܂���", "OK");
        //    return;
        //}

        // �}�e���A�����쐬

        var triplanarShader = Shader.Find("Shader Graphs/PLATEAULod1TriplanarShader");
        Material triplanarMaterial = new Material(triplanarShader);

        // �e�N�X�`����ǂݍ���

        triplanarMaterial.SetFloat("_Tile", 1f);
        triplanarMaterial.SetVector("_Offset", req.Bounds.min);
        triplanarMaterial.SetFloat("_PixelsPerMeter", req.PixelsPerMeter);
        foreach (var face in EnumEx.GetValues<Face>())
        {
            string filePath = GetTexturePath(req.MeshCode, face);
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
            triplanarMaterial.SetTexture($"_{face}Texture", tex);
        }

        triplanarMaterial.SetVector("_XCoef", req.XCoef);
        triplanarMaterial.SetVector("_YCoef", req.YCoef);
        triplanarMaterial.SetVector("_ZCoef", req.ZCoef);

        // �}�e���A����ۑ�
        string materialPath = GetMaterialPath(req.MeshCode);

        AssetDatabase.CreateAsset(triplanarMaterial, materialPath);

        //// �ΏۃI�u�W�F�N�g��Renderer�Ƀ}�e���A����K�p
        //Renderer[] renderers = triplanarTarget.GetComponentsInChildren<Renderer>();
        //foreach (Renderer renderer in renderers)
        //{
        //    renderer.material = triplanarMaterial;
        //}

        //AssetDatabase.SaveAssets();
        //AssetDatabase.Refresh();

        //EditorUtility.DisplayDialog("����", $"Triplanar���e��K�p���܂���\n�}�e���A��: {materialPath}", "OK");
    }
}
