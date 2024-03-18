#if UNITY_EDITOR && ADDRESSABLES
namespace Cat.Utillities
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.AddressableAssets.Settings;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class ShaderVariantCollectorConfig : ConfigableObject<ShaderVariantCollectorConfig>
    {
        private enum ESteps
        {
            None,
            Prepare,
            CollectAllMaterial,
            CollectVariants,
            CollectSleeping,
        }

        public ShaderVariantCollection targetCollection;
        public AddressableAssetGroup[] materialGroups;

        public bool ResumeOpeningScene = true;


        private readonly List<Material> materials = new();
        private readonly List<GameObject> spheres = new();
        private int cnt = 0;
        private ESteps step = ESteps.None;
        private string priviousScene;

        public void Collect()
        {
            if (step != ESteps.None) return;

            EditorHelper.FocusUnityGameWindow();

            EditorSceneManager.sceneClosed += SceneClosed;

            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
            step = ESteps.Prepare;

            EditorApplication.update += EditorUpdate;
        }

        private void SceneClosed(Scene scene)
        {
            priviousScene = scene.path;

            EditorSceneManager.sceneClosed -= SceneClosed;
        }

        private void EditorUpdate()
        {
            if (step == ESteps.Prepare)
            {
                EditorHelper.ClearCurrentShaderVariantCollection();

                step = ESteps.CollectAllMaterial;
                return;
            }

            if (step == ESteps.CollectAllMaterial)
            {
                materials.Clear();
                foreach (var group in materialGroups)
                {
                    foreach (var entry in group.entries)
                    {
                        var mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(entry.guid));
                        if (mat == null) continue;
                        materials.Add(mat);
                    }
                }
                step = ESteps.CollectVariants;
                return;
            }

            if (step == ESteps.CollectVariants)
            {

                CollectVariants();
                cnt = materials.Count;
                step = ESteps.CollectSleeping;
                return;
            }

            if (step == ESteps.CollectSleeping)
            {
                if (cnt != 0)
                {
                    --cnt;
                    EditorHelper.DisplayProgressBar("Collecting...", materials.Count - cnt, materials.Count);
                    return;
                }
            }

            EditorHelper.ClearProgressBar();
            Debug.Log($"Collected {EditorHelper.GetCurrentShaderVariantCollectionVariantCount()} Variants.");
            EditorHelper.SaveCurrentShaderVariantCollection(AssetDatabase.GetAssetPath(targetCollection));


            step = ESteps.None;
            EditorApplication.update -= EditorUpdate;

            if (ResumeOpeningScene)
            {
                EditorSceneManager.OpenScene(priviousScene);
            }
            EditorUtility.UnloadUnusedAssetsImmediate(true);

        }

        private void CollectVariants()
        {
            Camera camera = Camera.main != null ? Camera.main : throw new System.Exception("Not found main camera.");

            float aspect = camera.aspect;
            int totalMaterials = materials.Count;
            float height = Mathf.Sqrt(totalMaterials / aspect) + 1;
            float width = Mathf.Sqrt(totalMaterials / aspect) * aspect + 1;
            float halfHeight = Mathf.CeilToInt(height / 2f);
            float halfWidth = Mathf.CeilToInt(width / 2f);
            camera.orthographic = true;
            camera.orthographicSize = halfHeight;
            camera.transform.position = new Vector3(0f, 0f, -10f);

            spheres.Clear();

            int xMax = (int)(width - 1);
            int x = 0, y = 0;
            int progressValue = 0;
            for (int i = 0; i < materials.Count; i++)
            {
                var material = materials[i];
                var position = new Vector3(x - halfWidth + 1f, y - halfHeight + 1f, 0f);
                var go = CreateSphere(material, position, i);
                if (go != null)
                    spheres.Add(go);
                if (x == xMax)
                {
                    x = 0;
                    y++;
                }
                else
                {
                    x++;
                }
                EditorHelper.DisplayProgressBar("Progress materials", ++progressValue, materials.Count);
            }

            EditorHelper.ClearProgressBar();
        }

        private static GameObject CreateSphere(Material material, Vector3 position, int index)
        {
            var shader = material.shader;
            if (shader == null)
                return null;

            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.GetComponent<Renderer>().sharedMaterial = material;
            go.transform.position = position;
            go.name = $"Sphere_{index} | {material.name}";
            return go;
        }

    }

    public class ShaderVariantCollectorWindow : ConfigWindow<ShaderVariantCollectorConfig>
    {
        [MenuItem("Window/Cat/Shader Variant Collector")]
        private static void ShowWindow()
        {
            var window = GetWindow<ShaderVariantCollectorWindow>();
            window.titleContent = new() { text = "Shader Variant Collector" };
            window.Show();
        }

        protected override void Display()
        {
            if (GUILayout.Button("Collect"))
            {
                ShaderVariantCollectorConfig.Get().Collect();
            }
        }
    }
}

#endif