using System.Collections.Generic;
using UnityEngine;

namespace DripKings.MVP
{
    /// <summary>
    /// Self-contained, offline 3D showroom preview. It never modifies the gameplay vehicle.
    /// The preview camera only renders layer 29 into a private RenderTexture.
    /// </summary>
    public sealed class GarageVehiclePreview : MonoBehaviour
    {
        private const int PreviewLayer = 29;
        private readonly List<Material> materials = new List<Material>();
        private GameObject previewWorld;
        private GameObject carPivot;
        private Camera previewCamera;
        private RenderTexture renderTexture;
        private Material paintMaterial;
        private int appliedPaint = -1;

        public RenderTexture Texture => renderTexture;

        private void Awake()
        {
            renderTexture = new RenderTexture(640, 336, 16, RenderTextureFormat.ARGB32)
            {
                name = "DripKings_GaragePreview",
                antiAliasing = 1
            };
            renderTexture.Create();

            previewWorld = new GameObject("Garage Showroom (Local Preview Only)");
            previewWorld.transform.SetParent(transform, false);
            previewWorld.transform.position = new Vector3(0f, -5000f, 0f);
            previewWorld.layer = PreviewLayer;

            Material floorMaterial = NewMaterial(new Color32(30, 38, 54, 255));
            Material glassMaterial = NewMaterial(new Color32(24, 35, 53, 255));
            Material rubberMaterial = NewMaterial(new Color32(16, 19, 24, 255));
            Material metalMaterial = NewMaterial(new Color32(144, 153, 164, 255));
            Material lightMaterial = NewMaterial(new Color32(245, 229, 178, 255));
            Material rearLightMaterial = NewMaterial(new Color32(194, 44, 48, 255));
            paintMaterial = NewMaterial(LocalGarage.SavedPaintColor);

            AddPart("Platform", previewWorld.transform,
                new Vector3(0f, -0.38f, 0f), new Vector3(5.8f, 0.16f, 6.8f), floorMaterial);

            carPivot = new GameObject("Rotating Car");
            carPivot.layer = PreviewLayer;
            carPivot.transform.SetParent(previewWorld.transform, false);

            AddPart("Car Body", carPivot.transform,
                new Vector3(0f, 0.36f, 0f), new Vector3(2f, 0.58f, 3.7f), paintMaterial);
            AddPart("Cabin", carPivot.transform,
                new Vector3(0f, 0.87f, -0.26f), new Vector3(1.63f, 0.62f, 1.85f), paintMaterial);
            AddPart("Windshield", carPivot.transform,
                new Vector3(0f, 0.91f, 0.69f), new Vector3(1.39f, 0.42f, 0.08f), glassMaterial);
            AddPart("Rear Window", carPivot.transform,
                new Vector3(0f, 0.91f, -1.20f), new Vector3(1.39f, 0.42f, 0.08f), glassMaterial);
            AddPart("Front Bumper", carPivot.transform,
                new Vector3(0f, 0.26f, 1.91f), new Vector3(1.93f, 0.18f, 0.18f), metalMaterial);
            AddPart("Rear Bumper", carPivot.transform,
                new Vector3(0f, 0.26f, -1.91f), new Vector3(1.93f, 0.18f, 0.18f), metalMaterial);

            for (int side = -1; side <= 1; side += 2)
            {
                for (int axle = -1; axle <= 1; axle += 2)
                {
                    var wheel = AddPart("Wheel", carPivot.transform,
                        new Vector3(side * 1.08f, 0.10f, axle * 1.22f),
                        new Vector3(0.68f, 0.18f, 0.68f), rubberMaterial, PrimitiveType.Cylinder);
                    wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                }
                AddPart("Headlight", carPivot.transform,
                    new Vector3(side * 0.67f, 0.47f, 1.87f),
                    new Vector3(0.40f, 0.18f, 0.08f), lightMaterial);
                AddPart("Tail Light", carPivot.transform,
                    new Vector3(side * 0.67f, 0.47f, -1.87f),
                    new Vector3(0.40f, 0.18f, 0.08f), rearLightMaterial);
            }

            var cameraObject = new GameObject("Garage Preview Camera");
            cameraObject.layer = PreviewLayer;
            cameraObject.transform.SetParent(previewWorld.transform, false);
            cameraObject.transform.localPosition = new Vector3(5.5f, 2.85f, 6.15f);
            cameraObject.transform.LookAt(previewWorld.transform.position + new Vector3(0f, 0.4f, 0f));
            previewCamera = cameraObject.AddComponent<Camera>();
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = new Color32(18, 26, 42, 255);
            previewCamera.cullingMask = 1 << PreviewLayer;
            previewCamera.fieldOfView = 36f;
            previewCamera.targetTexture = renderTexture;
            previewCamera.depth = -20f;

            var lampObject = new GameObject("Garage Preview Light");
            lampObject.layer = PreviewLayer;
            lampObject.transform.SetParent(previewWorld.transform, false);
            lampObject.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
            var lamp = lampObject.AddComponent<Light>();
            lamp.type = LightType.Directional;
            lamp.intensity = 2.1f;
            lamp.cullingMask = 1 << PreviewLayer;

            SetPreviewActive(false);
        }

        private static GameObject AddPart(string name, Transform parent, Vector3 position,
            Vector3 scale, Material material, PrimitiveType primitive = PrimitiveType.Cube)
        {
            GameObject part = GameObject.CreatePrimitive(primitive);
            part.name = name;
            part.layer = PreviewLayer;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            var collider = part.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            part.GetComponent<Renderer>().sharedMaterial = material;
            return part;
        }

        private Material NewMaterial(Color tint)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            var material = new Material(shader)
            {
                name = "DripKings Preview Runtime Material",
                hideFlags = HideFlags.HideAndDontSave
            };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", tint);
            if (material.HasProperty("_Color")) material.SetColor("_Color", tint);
            materials.Add(material);
            return material;
        }

        private void Update()
        {
            bool inGarage = DripKingsMenuController.IsGarageOpen;
            if (previewWorld.activeSelf != inGarage)
                SetPreviewActive(inGarage);

            if (!inGarage) return;

            int nextPaint = LocalGarage.SavedPaintIndex;
            if (nextPaint != appliedPaint)
            {
                appliedPaint = nextPaint;
                Color color = LocalGarage.SavedPaintColor;
                if (paintMaterial.HasProperty("_BaseColor")) paintMaterial.SetColor("_BaseColor", color);
                if (paintMaterial.HasProperty("_Color")) paintMaterial.SetColor("_Color", color);
            }

            carPivot.transform.Rotate(0f, 17f * Time.unscaledDeltaTime, 0f, Space.Self);
        }

        private void SetPreviewActive(bool active)
        {
            if (previewWorld != null) previewWorld.SetActive(active);
            if (previewCamera != null) previewCamera.enabled = active;
        }

        private void OnDestroy()
        {
            if (previewCamera != null) previewCamera.targetTexture = null;
            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
            }
            foreach (Material material in materials)
                if (material != null) Destroy(material);
            if (previewWorld != null) Destroy(previewWorld);
        }
    }
}
