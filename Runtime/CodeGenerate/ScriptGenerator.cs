#if UNITY_EDITOR
namespace Cat.CodeGen
{
    using System;
    using System.Linq;
    using System.IO;
    using UnityEditor;
    internal static class ScriptGenerator
    {
        const string KEY_ISGENERATING = "CatGen-IsGenerating";

        static bool IsGenerating
        {
            get
            {
                if (bool.TryParse(EditorUserSettings.GetConfigValue(KEY_ISGENERATING), out var result))
                {
                    return result;
                }
                return false;
            }
            set
            {
                EditorUserSettings.SetConfigValue(KEY_ISGENERATING, value.ToString());
            }
        }

        // static List<string> fileNames = new List<string>();

        [UnityEditor.Callbacks.DidReloadScripts]
        [InitializeOnLoadMethod]
        static void Initialize()
        {
            if (IsGenerating)
            {
                IsGenerating = false;
            }
            else
            {
                Generate();
            }
        }

        [MenuItem("Window/Cat/CodeGen/Generate")]
        internal static void GenerateMenu()
        {
            IsGenerating = false;
            Generate();
        }
        internal static void Generate()
        {
            if (IsGenerating) return;
            IsGenerating = true;

            // fileNames.Clear();
            var generatorTypes = TypeCache.GetTypesDerivedFrom<ICodeGenerator>()
                .Where(x => !x.IsAbstract);

            var changed = false;
            foreach (var t in generatorTypes)
            {
                var generator = (ICodeGenerator)Activator.CreateInstance(t);
                var context = new GeneratorContext();
                generator.Execute(context);

                if (GenerateScriptFromContext(context))
                {
                    changed = true;
                }
            }

            if (changed)
            {
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
        }

        static bool GenerateScriptFromContext(GeneratorContext context)
        {
            var changed = false;

            var folderPath = context.GerateFolderPath;

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            foreach (var code in context.CodeList)
            {
                var hierarchy = code.FileName.Split('/');
                var fileName = hierarchy[hierarchy.Length - 1];
                var path = folderPath;
                for (int i = 0; i < hierarchy.Length; i++)
                {
                    path += "/" + hierarchy[i];
                    if (i == hierarchy.Length - 1) break;
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }

                if (File.Exists(path))
                {
                    var text = File.ReadAllText(path);
                    if (text == code.Text)
                    {
                        // fileNames.Add(code.fileName);
                        continue;
                    }
                }

                File.WriteAllText(path, code.Text);
                // fileNames.Add(code.fileName);
                changed = true;
            }

            return changed;
        }
    }

}
#endif