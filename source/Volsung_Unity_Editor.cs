#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Volsung_Unity))]
public class Volsung_Unity_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Welcome to Volsung.");

        Volsung_Unity plugin = (Volsung_Unity) target;

        plugin.file = (TextAsset) EditorGUILayout.ObjectField("Program:", plugin.file, typeof(TextAsset), false);

        if (GUILayout.Button("Compile"))
        {
            plugin.Compile();
        }

        plugin.stereo = GUILayout.Toggle(plugin.stereo, "Stereo");
        plugin.compileOnAwake = GUILayout.Toggle(plugin.compileOnAwake, "Compile on Awake");

        EditorGUILayout.Space();
        plugin.numParameters = EditorGUILayout.IntField("Parameter count", plugin.numParameters);

        if (plugin.numParameters > 512)
        {
            EditorGUILayout.LabelField("What? Really?");
            return;
        }

        if (plugin.numParameters < 0) plugin.numParameters = 0;

        while (plugin.numParameters > plugin.parameterNames.Count) {
            plugin.parameterNames.Add("Unnamed Parameter");
            plugin.parameterValues.Add(0.0f);
        }

        while (plugin.numParameters < plugin.parameterValues.Count) {
            plugin.parameterNames.RemoveAt(plugin.parameterNames.Count - 1);
            plugin.parameterValues.RemoveAt(plugin.parameterValues.Count - 1);
        }

        for (int n = 0; n < plugin.numParameters; n++)
        {
            EditorGUILayout.Space();

            float value = plugin.parameterValues[n];
            float min = Mathf.Clamp(value, value, 0);
            float max = Mathf.Clamp(value, 1, value);

            plugin.parameterNames[n] = EditorGUILayout.TextField(plugin.parameterNames[n]);
            plugin.parameterValues[n] = EditorGUILayout.Slider("", value, min, max);
            EditorGUILayout.Space();
        }
    }
}
#endif
