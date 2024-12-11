using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace AICommand
{
    public sealed class AICommandWindow : EditorWindow
    {
        #region Temporary script file operations

        private const string TempFilePath = "Assets/AICommandTemp.cs";

        private bool TempFileExists => System.IO.File.Exists(TempFilePath);

        private void CreateScriptAsset(string code)
        {
            // Use UnityEditor internal method: ProjectWindowUtil.CreateScriptAssetWithContent
            var flags = BindingFlags.Static | BindingFlags.NonPublic;
            var method = typeof(ProjectWindowUtil).GetMethod("CreateScriptAssetWithContent", flags);

            if (method == null)
            {
                Debug.LogError("Failed to locate CreateScriptAssetWithContent method.");
                return;
            }

            try
            {
                method.Invoke(null, new object[] { TempFilePath, code });
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to create script asset: {ex.Message}");
            }
        }

        #endregion

        #region Script generator

        private static string WrapPrompt(string input)
            => "Write a Unity Editor script.\n" +
               " - It provides its functionality as a menu item placed \"Edit\" > \"Do Task\".\n" +
               " - It doesn’t provide any editor window. It immediately does the task when the menu item is invoked.\n" +
               " - Don’t use GameObject.FindGameObjectsWithTag.\n" +
               " - There is no selected object. Find game objects manually.\n" +
               " - I only need the script body. Don’t add any explanation.\n" +
               "The task is described as follows:\n" + input;

        private void RunGenerator()
        {
            var code = OpenAIUtil.InvokeChat(WrapPrompt(_prompt));
            if (string.IsNullOrWhiteSpace(code))
            {
                Debug.LogError("Failed to generate script. Please check the OpenAI API response.");
                return;
            }

            Debug.Log("AI command script generated:\n" + code);
            CreateScriptAsset(code);
        }

        #endregion

        #region Editor GUI

        private string _prompt = "Create 100 cubes at random points.";

        private const string ApiKeyErrorText =
            "API Key hasn't been set. Please check the project settings " +
            "(Edit > Project Settings > AI Command > API Key).";

        private bool IsApiKeyOk => !string.IsNullOrEmpty(AICommandSettings.instance.apiKey);

        [MenuItem("Window/AI Command")]
        private static void Init() => GetWindow<AICommandWindow>(true, "AI Command");

        private void OnGUI()
        {
            if (IsApiKeyOk)
            {
                EditorGUILayout.LabelField("Task Prompt", EditorStyles.boldLabel);
                _prompt = EditorGUILayout.TextArea(_prompt, GUILayout.ExpandHeight(true));

                if (GUILayout.Button("Run"))
                {
                    RunGenerator();
                }
            }
            else
            {
                EditorGUILayout.HelpBox(ApiKeyErrorText, MessageType.Error);
            }
        }

        #endregion

        #region Script lifecycle

        private void OnEnable()
            => AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

        private void OnDisable()
            => AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;

        private void OnAfterAssemblyReload()
        {
            if (!TempFileExists) return;

            try
            {
                EditorApplication.ExecuteMenuItem("Edit/Do Task");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to execute the temporary script: {ex.Message}");
            }

            AssetDatabase.DeleteAsset(TempFilePath);
        }

        #endregion
    }
}
