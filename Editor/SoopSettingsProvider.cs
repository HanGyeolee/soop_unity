using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SoopExtension.Editor
{
    public class SoopSettingsProvider : SettingsProvider
    {
        private const string SETTINGS_PATH = "Project/SOOP Extension";
        private SerializedObject serializedSettings;
        private SerializedProperty baseUrlsProperty;
        private SerializedProperty userAgentProperty;

        public SoopSettingsProvider(string path, SettingsScope scope = SettingsScope.Project)
            : base(path, scope) { }

        public static bool IsSettingsAvailable()
        {
            return SoopSettings.Instance != null;
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            var settings = SoopSettings.GetOrCreateSettings();
            serializedSettings = new SerializedObject(settings);
            baseUrlsProperty = serializedSettings.FindProperty("baseUrls");
            userAgentProperty = serializedSettings.FindProperty("userAgent");
        }

        public override void OnGUI(string searchContext)
        {
            if (serializedSettings == null) return;

            serializedSettings.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("SOOP Extension Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("API Base URLs", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            if (baseUrlsProperty != null)
            {
                var liveUrlProperty = baseUrlsProperty.FindPropertyRelative("soopLiveBaseUrl");
                var channelUrlProperty = baseUrlsProperty.FindPropertyRelative("soopChannelBaseUrl");
                var authUrlProperty = baseUrlsProperty.FindPropertyRelative("soopAuthBaseUrl");

                EditorGUILayout.PropertyField(liveUrlProperty, new GUIContent("Live API URL"));
                EditorGUILayout.PropertyField(channelUrlProperty, new GUIContent("Channel API URL"));
                EditorGUILayout.PropertyField(authUrlProperty, new GUIContent("Auth API URL"));
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(userAgentProperty, new GUIContent("User Agent"));
            EditorGUILayout.Space();

            if (GUILayout.Button("Reset to Default"))
            {
                var settings = SoopSettings.Instance;
                settings.baseUrls = new SoopAPIBaseUrls();
                settings.userAgent = SoopConstants.DEFAULT_USER_AGENT;
                EditorUtility.SetDirty(settings);
            }

            serializedSettings.ApplyModifiedProperties();
        }

        [SettingsProvider]
        public static SettingsProvider CreateSoopSettingsProvider()
        {
            if (IsSettingsAvailable())
            {
                var provider = new SoopSettingsProvider(SETTINGS_PATH, SettingsScope.Project);
                provider.keywords = GetSearchKeywordsFromGUIContentProperties<SoopSettings>();
                return provider;
            }
            return null;
        }
    }

    [CreateAssetMenu(fileName = "SoopSettings", menuName = "SOOP/Settings")]
    public class SoopSettings : ScriptableObject
    {
        public SoopAPIBaseUrls baseUrls = new SoopAPIBaseUrls();
        public string userAgent = SoopConstants.DEFAULT_USER_AGENT;

        private static SoopSettings instance;

        public static SoopSettings Instance
        {
            get
            {
                if (instance == null)
                    instance = GetOrCreateSettings();
                return instance;
            }
        }

        public static SoopSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<SoopSettings>("Assets/Settings/SoopSettings.asset");
            if (settings == null)
            {
                settings = CreateInstance<SoopSettings>();
                settings.baseUrls = new SoopAPIBaseUrls();
                settings.userAgent = SoopConstants.DEFAULT_USER_AGENT;

                if (!AssetDatabase.IsValidFolder("Assets/Settings"))
                    AssetDatabase.CreateFolder("Assets", "Settings");

                AssetDatabase.CreateAsset(settings, "Assets/Settings/SoopSettings.asset");
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        public static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }
}
