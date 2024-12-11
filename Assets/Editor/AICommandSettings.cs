using UnityEngine;
using UnityEditor;

namespace AICommand
{
    [FilePath("UserSettings/AICommandSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class AICommandSettings : ScriptableSingleton<AICommandSettings>
    {
        [Header("OpenAI Settings")]
        [Tooltip("The API key for accessing the OpenAI API.")]
        public string apiKey = null;

        [Tooltip("The timeout value (in seconds) for API requests.")]
        public int timeout = 10;

        /// <summary>
        /// Saves the settings to disk.
        /// </summary>
        public void Save() => Save(true);

        private void OnDisable() => Save();
    }

    sealed class AICommandSettingsProvider : SettingsProvider
    {
        public AICommandSettingsProvider()
            : base("Project/AI Command", SettingsScope.Project) { }

        public override void OnGUI(string search)
        {
            var settings = AICommandSettings.instance;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("AI Command Settings", EditorStyles.boldLabel);
            settings.apiKey = EditorGUILayout.TextField(new GUIContent("API Key", "Your OpenAI API key."), settings.apiKey);

            settings.timeout = EditorGUILayout.IntField(new GUIContent("Timeout", "Request timeout in seconds (default: 10)."), settings.timeout);

            // Ensure timeout is a positive value
            if (settings.timeout < 1)
            {
                EditorGUILayout.HelpBox("Timeout must be greater than 0.", MessageType.Warning);
                settings.timeout = 10; // Default value
            }

            if (EditorGUI.EndChangeCheck())
            {
                settings.Save();
            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateCustomSettingsProvider()
            => new AICommandSettingsProvider();
    }
}
