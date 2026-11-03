using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// <summary>
/// Captures the actual scene with URP in Edit Mode. Does not enter Play Mode or save assets.
/// Run with -batchmode -executeMethod FirstFieldLevelDesignPreview.Capture, without -quit
/// or -nographics. The update loop exits the editor after writing its report.
/// </summary>
public static class FirstFieldLevelDesignPreview
{
    const string ScenePath = "Assets/2.Scenes/TutorialScene.unity";
    const string DressingPath = "Assets/LevelDesign/FirstField/FirstFieldDressing.prefab";
    const string OutputDirectory = "Logs/FirstFieldPreview";
    const int Width = 1280;
    const int Height = 720;

    static readonly List<Frame> Frames = new List<Frame>
    {
        new Frame("v02-camp-before", 70, -70, false, false),
        new Frame("v02-camp-day", 70, -70, true, false),
        new Frame("v02-camp-night", 70, -70, true, true),
        new Frame("v02-fork", -28, -58, true, false),
        new Frame("v02-salvage", -4, -64, true, false),
        new Frame("v02-housing", -4, -150, true, false),
        new Frame("v02-forest", -80, -110, true, false),
        new Frame("v02-scrapyard", 156, -160, true, false),
        new Frame("v02-workshop", 125, 20, true, false),
        new Frame("v02-outskirts", 224, -70, true, false),
        new Frame("v02-factory", 252, -52, true, false),
        new Frame("v02-overview", 80, -75, true, false)
    };

    static Camera previewCamera;
    static Light2D previewLight;
    static GameObject temporaryRoot;
    static GameObject dressing;
    static RenderTexture target;
    static CaptureReport report;
    static string originalSceneHash;
    static double startedAt;
    static int frameIndex;
    static int phase;
    static int waitFrames;

    public static void Capture()
    {
        if (!Application.isBatchMode)
            throw new InvalidOperationException("Run this capture utility in a separate Unity batch process to preserve the open editor session.");

        report = new CaptureReport();
        Directory.CreateDirectory(OutputDirectory);
        try
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                throw new InvalidOperationException("A graphics device is required. Do not pass -nographics.");
            if (GraphicsSettings.currentRenderPipeline == null)
                throw new InvalidOperationException("The project URP asset must be active for a Sprite-Lit preview.");

            originalSceneHash = HashFile(ScenePath);
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == "FirstFieldDressing")
                {
                    if (dressing != null)
                        throw new InvalidOperationException("More than one FirstFieldDressing root exists.");
                    dressing = root;
                }
                foreach (Camera camera in root.GetComponentsInChildren<Camera>(true))
                    camera.enabled = false;
                foreach (Canvas canvas in root.GetComponentsInChildren<Canvas>(true))
                    canvas.enabled = false;
                foreach (Light2D light in root.GetComponentsInChildren<Light2D>(true))
                    light.enabled = false;
            }

            if (dressing == null)
                throw new InvalidOperationException("FirstFieldDressing must already be attached to TutorialScene.");
            if (PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(dressing) != DressingPath)
                throw new InvalidOperationException("The scene dressing root does not reference " + DressingPath);

            FirstFieldLevelDesign.Validate(dressing);
            FirstFieldLevelDesign.AuditSceneRoutes();
            foreach (Frame frame in Frames)
            {
                if (string.IsNullOrEmpty(frame.landmarkPath))
                    continue;
                Transform landmark = dressing.transform.Find(frame.landmarkPath);
                if (landmark == null)
                    throw new InvalidOperationException("Missing landmark cluster: " + frame.landmarkPath);
                frame.x = landmark.position.x;
                frame.y = landmark.position.y;
            }

            report.dressingSpriteCount = dressing.GetComponentsInChildren<SpriteRenderer>(true).Length;
            if (report.dressingSpriteCount == 0)
                throw new InvalidOperationException("The dressing prefab contains no SpriteRenderers.");
            foreach (SpriteRenderer sprite in dressing.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (sprite.sprite == null || sprite.sharedMaterial == null)
                    throw new InvalidOperationException("Missing sprite or material: " + sprite.name);
            }

            temporaryRoot = new GameObject("FirstFieldPreview_Temporary") { hideFlags = HideFlags.HideAndDontSave };
            var cameraObject = new GameObject("PreviewCamera");
            cameraObject.transform.SetParent(temporaryRoot.transform);
            previewCamera = cameraObject.AddComponent<Camera>();
            previewCamera.enabled = false;
            previewCamera.orthographic = true;
            previewCamera.orthographicSize = 9;
            previewCamera.aspect = (float)Width / Height;
            previewCamera.nearClipPlane = 0.1f;
            previewCamera.farClipPlane = 100;
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = new Color(0.08f, 0.06f, 0.04f, 1);
            previewCamera.allowHDR = false;
            previewCamera.allowMSAA = false;
            previewCamera.cullingMask = ~(1 << LayerMask.NameToLayer("UI"));
            previewCamera.transparencySortMode = TransparencySortMode.CustomAxis;
            previewCamera.transparencySortAxis = Vector3.up;
            var cameraData = cameraObject.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing = false;
            cameraData.allowXRRendering = false;
            cameraData.SetRenderer(-1);

            var lightObject = new GameObject("PreviewGlobalLight");
            lightObject.transform.SetParent(temporaryRoot.transform);
            previewLight = lightObject.AddComponent<Light2D>();
            previewLight.lightType = Light2D.LightType.Global;
            previewLight.targetSortingLayers = new[] { SortingLayer.NameToID("Default") };
            previewLight.intensity = 1;

            target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32)
            {
                name = "FirstFieldPreviewTarget",
                antiAliasing = 1,
                filterMode = FilterMode.Point,
                hideFlags = HideFlags.HideAndDontSave
            };
            target.Create();
            previewCamera.targetTexture = target;
            startedAt = EditorApplication.timeSinceStartup;
            frameIndex = 0;
            phase = 0;
            EditorApplication.update += Tick;
            EditorApplication.QueuePlayerLoopUpdate();
        }
        catch (Exception exception)
        {
            Finish(exception);
        }
    }

    static void Tick()
    {
        try
        {
            if (EditorApplication.timeSinceStartup - startedAt > 240)
                throw new TimeoutException("First field capture did not finish within 240 seconds.");
            if (frameIndex >= Frames.Count)
            {
                Finish(null);
                return;
            }

            Frame frame = Frames[frameIndex];
            if (phase == 0)
            {
                dressing.SetActive(frame.showDressing);
                previewCamera.orthographicSize = frame.name == "v02-overview" ? 125 : 9;
                previewCamera.transform.SetPositionAndRotation(new Vector3(frame.x, frame.y, -10), Quaternion.identity);
                // Match isMorning.anim and isNight.anim instead of inventing brighter preview lighting.
                previewLight.color = frame.night ? new Color(0.5f, 0.5f, 0.6f, 1) : new Color(0.8f, 0.8f, 0.8f, 1);
                waitFrames = 3;
                phase = 1;
            }
            else if (waitFrames-- <= 0)
            {
                RenderFrame();
                if (phase == 1)
                {
                    // Render once before readback to warm up the pipeline and newly visible tile chunks.
                    phase = 2;
                    waitFrames = 1;
                }
                else
                {
                    SaveFrame(frame);
                    frameIndex++;
                    phase = 0;
                }
            }
            EditorApplication.QueuePlayerLoopUpdate();
        }
        catch (Exception exception)
        {
            Finish(exception);
        }
    }

    static void RenderFrame()
    {
        var request = new UniversalRenderPipeline.SingleCameraRequest { destination = target };
        if (RenderPipelineManager.currentPipeline == null)
            previewCamera.Render();
        if (!RenderPipeline.SupportsRenderRequest(previewCamera, request))
            throw new InvalidOperationException("The active render pipeline does not support the URP single-camera render request.");
        RenderPipeline.SubmitRenderRequest(previewCamera, request);
    }

    static void SaveFrame(Frame frame)
    {
        RenderTexture previous = RenderTexture.active;
        var image = new Texture2D(Width, Height, TextureFormat.RGB24, false);
        try
        {
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            image.Apply();
            Color32[] pixels = image.GetPixels32();
            var colors = new HashSet<int>();
            for (int i = 0; i < pixels.Length; i += 127)
            {
                Color32 pixel = pixels[i];
                colors.Add(pixel.r | (pixel.g << 8) | (pixel.b << 16));
            }
            if (colors.Count < 3)
                throw new InvalidOperationException("Capture is blank or uniform: " + frame.name);
            File.WriteAllBytes(Path.Combine(OutputDirectory, frame.name + ".png"), image.EncodeToPNG());
            report.frames.Add(frame);
            Debug.Log("FIRST_FIELD_PREVIEW: " + frame.name + ".png");
        }
        finally
        {
            RenderTexture.active = previous;
            Object.DestroyImmediate(image);
        }
    }

    static void Finish(Exception exception)
    {
        EditorApplication.update -= Tick;
        if (dressing != null)
            dressing.SetActive(true);
        if (temporaryRoot != null)
            Object.DestroyImmediate(temporaryRoot);
        if (target != null)
        {
            target.Release();
            Object.DestroyImmediate(target);
        }
        report.sceneFileUnchanged = originalSceneHash != null && originalSceneHash == HashFile(ScenePath);
        if (!report.sceneFileUnchanged && exception == null)
            exception = new InvalidOperationException("The scene file changed during the preview capture.");
        report.success = exception == null;
        report.error = exception == null ? "" : exception.ToString();
        File.WriteAllText(Path.Combine(OutputDirectory, "capture-report.json"), JsonUtility.ToJson(report, true));
        if (exception != null)
            Debug.LogException(exception);
        else
            Debug.Log("FIRST_FIELD_PREVIEW_COMPLETE: " + report.frames.Count + " Edit Mode captures; scene file unchanged.");
        EditorApplication.Exit(report.success ? 0 : 1);
    }

    static string HashFile(string path)
    {
        using (var stream = File.OpenRead(path))
        using (var sha = SHA256.Create())
            return BitConverter.ToString(sha.ComputeHash(stream));
    }

    [Serializable]
    sealed class CaptureReport
    {
        public bool success;
        public string scene = ScenePath;
        public string mode = "Edit Mode, actual scene render; no gameplay or collision test";
        public int width = Width;
        public int height = Height;
        public float orthographicSize = 9;
        public int dressingSpriteCount;
        public bool sceneFileUnchanged;
        public string error;
        public List<Frame> frames = new List<Frame>();
    }

    [Serializable]
    sealed class Frame
    {
        public string name;
        public float x;
        public float y;
        public bool showDressing;
        public bool night;
        public string landmarkPath;

        public Frame(string name, string landmarkPath) : this(name, 0, 0, true, false)
        {
            this.landmarkPath = landmarkPath;
        }

        public Frame(string name, float x, float y, bool showDressing, bool night)
        {
            this.name = name;
            this.x = x;
            this.y = y;
            this.showDressing = showDressing;
            this.night = night;
        }
    }
}
