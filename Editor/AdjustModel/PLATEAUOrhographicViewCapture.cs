using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class PLATEAUOrthographicViewCapture : EditorWindow
{
    private List<GameObject> targetObjects = new List<GameObject>();
    private GameObject triplanarTarget;
    private int imageWidth = 1024;
    private int imageHeight = 1024;
    private string savePath = "Assets/Screenshots/";
    private float textureScale = 1.0f;
    private float blendSharpness = 3.0f;
    private float brightness = 1.0f;

    // �L���v�`�������e�N�X�`����ۑ�
    private string lastFrontTexPath;
    private string lastSideTexPath;
    private string lastTopTexPath;

    [MenuItem("Tools/Orthographic View Capture")]
    public static void ShowWindow()
    {
        GetWindow<PLATEAUOrthographicViewCapture>("3�ʐ}�L���v�`�� & Triplanar�K�p");
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
                if (!targetObjects.Contains(obj))
                {
                    targetObjects.Add(obj);
                }
            }
        }

        // �I�u�W�F�N�g���X�g�̕\��
        for (int i = 0; i < targetObjects.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            targetObjects[i] = (GameObject)EditorGUILayout.ObjectField(targetObjects[i], typeof(GameObject), true);
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
        imageWidth = EditorGUILayout.IntField("�摜��", imageWidth);
        imageHeight = EditorGUILayout.IntField("�摜����", imageHeight);
        savePath = EditorGUILayout.TextField("�ۑ��p�X", savePath);

        GUILayout.Space(10);

        // �L���v�`���{�^��
        if (GUILayout.Button("3�ʐ}���L���v�`��"))
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

        GUI.enabled = triplanarTarget != null && !string.IsNullOrEmpty(lastFrontTexPath);
        if (GUILayout.Button("Triplanar���e��K�p"))
        {
            ApplyTriplanarProjection();
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
        camera.backgroundColor = Color.white;
        camera.clearFlags = CameraClearFlags.SolidColor;

        try
        {
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // ���ʐ} (Front View - Z������)
            lastFrontTexPath = CaptureView(camera, combinedBounds, Vector3.forward, Vector3.up, "Front", timestamp);

            // ���ʐ} (Side View - X������)
            lastSideTexPath = CaptureView(camera, combinedBounds, Vector3.right, Vector3.up, "Side", timestamp);

            // ��ʐ} (Top View - Y������)
            lastTopTexPath = CaptureView(camera, combinedBounds, Vector3.up, Vector3.forward, "Top", timestamp);

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

        foreach (GameObject obj in targetObjects)
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

    private string CaptureView(Camera camera, Bounds bounds, Vector3 direction, Vector3 up, string viewName, string timestamp)
    {
        // �J�����̈ʒu�ƌ�����ݒ�
        Vector3 center = bounds.center;
        float distance = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) * 2f;

        camera.transform.position = center + direction * distance;
        camera.transform.LookAt(center, up);

        // ���𓊉e�̃T�C�Y��ݒ�
        float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        camera.orthographicSize = maxSize * 0.6f;

        // �j�A�N���b�v�ƃt�@�[�N���b�v��ݒ�
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = distance * 2f;

        // �����_�[�e�N�X�`�����쐬
        RenderTexture renderTexture = new RenderTexture(imageWidth, imageHeight, 24);
        camera.targetTexture = renderTexture;

        // �����_�����O���s
        camera.Render();

        // �e�N�X�`����ǂݎ��
        RenderTexture.active = renderTexture;
        Texture2D screenshot = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
        screenshot.Apply();

        // �t�@�C���ɕۑ�
        byte[] data = screenshot.EncodeToPNG();
        string fileName = $"OrthographicView_{viewName}_{timestamp}.png";
        string filePath = Path.Combine(savePath, fileName);
        File.WriteAllBytes(filePath, data);

        // �N���[���A�b�v
        RenderTexture.active = null;
        camera.targetTexture = null;
        DestroyImmediate(renderTexture);
        DestroyImmediate(screenshot);

        Debug.Log($"{viewName}�ʐ}��ۑ����܂���: {filePath}");

        return filePath;
    }

    private void ApplyTriplanarProjection()
    {
        if (triplanarTarget == null)
        {
            EditorUtility.DisplayDialog("�G���[", "�K�p�ΏۃI�u�W�F�N�g���ݒ肳��Ă��܂���", "OK");
            return;
        }

        // Triplanar�V�F�[�_�[��ǂݍ���
        Shader triplanarShader = Shader.Find("Custom/TriplanarProjection");
        if (triplanarShader == null)
        {
            EditorUtility.DisplayDialog("�G���[", "TriplanarProjection�V�F�[�_�[��������܂���", "OK");
            return;
        }

        // �}�e���A�����쐬
        Material triplanarMaterial = new Material(triplanarShader);

        // �e�N�X�`����ǂݍ���
        Texture2D frontTex = AssetDatabase.LoadAssetAtPath<Texture2D>(lastFrontTexPath);
        Texture2D sideTex = AssetDatabase.LoadAssetAtPath<Texture2D>(lastSideTexPath);
        Texture2D topTex = AssetDatabase.LoadAssetAtPath<Texture2D>(lastTopTexPath);

        if (frontTex == null || sideTex == null || topTex == null)
        {
            EditorUtility.DisplayDialog("�G���[", "�e�N�X�`���̓ǂݍ��݂Ɏ��s���܂���", "OK");
            return;
        }

        // �}�e���A���Ƀe�N�X�`����ݒ�
        triplanarMaterial.SetTexture("_FrontTex", frontTex);
        triplanarMaterial.SetTexture("_SideTex", sideTex);
        triplanarMaterial.SetTexture("_TopTex", topTex);
        triplanarMaterial.SetFloat("_Scale", textureScale);
        triplanarMaterial.SetFloat("_BlendSharpness", blendSharpness);
        triplanarMaterial.SetFloat("_Brightness", brightness);

        // �}�e���A����ۑ�
        string materialPath = Path.Combine(savePath, $"TriplanarMaterial_{System.DateTime.Now:yyyyMMdd_HHmmss}.mat");
        AssetDatabase.CreateAsset(triplanarMaterial, materialPath);

        // �ΏۃI�u�W�F�N�g��Renderer�Ƀ}�e���A����K�p
        Renderer[] renderers = triplanarTarget.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material = triplanarMaterial;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("����", $"Triplanar���e��K�p���܂���\n�}�e���A��: {materialPath}", "OK");
    }
}
