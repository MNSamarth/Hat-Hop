using UnityEditor;
using UnityEngine;

namespace HatHop.Editor
{
    [CustomEditor(typeof(LevelCatalog))]
    public sealed class LevelCatalogEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("Play opens Easy when assigned; otherwise it opens the prototype. " +
                "Unassigned levels appear as Coming Soon. Assign each new level here, then refresh build scenes.", MessageType.Info);
            SceneField("prototypeScenePath", "Prototype");
            SceneField("easyScenePath", "Easy");
            SceneField("mediumScenePath", "Medium");
            SceneField("hardScenePath", "Hard");
            serializedObject.ApplyModifiedProperties();
            if (GUILayout.Button("Refresh Menu Build Scenes"))
            {
                AssetDatabase.SaveAssets();
                MainMenuSceneBuilder.RefreshBuildScenes();
            }
        }

        private void SceneField(string propertyName, string label)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            SceneAsset current = AssetDatabase.LoadAssetAtPath<SceneAsset>(property.stringValue);
            EditorGUI.BeginChangeCheck();
            SceneAsset chosen = (SceneAsset)EditorGUILayout.ObjectField(label, current, typeof(SceneAsset), false);
            if (EditorGUI.EndChangeCheck())
                property.stringValue = chosen == null ? "" : AssetDatabase.GetAssetPath(chosen);
            if (!string.IsNullOrEmpty(property.stringValue) && current == null)
                EditorGUILayout.HelpBox(label + " scene not found: " + property.stringValue, MessageType.Warning);
        }
    }
}
