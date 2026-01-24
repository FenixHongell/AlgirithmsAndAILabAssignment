using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonGenerator))]
public class DungeonGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var generator = (DungeonGenerator)target;

        EditorGUILayout.Space();
        if (GUILayout.Button("Generate Dungeon"))
        {
            generator.ButtonEventGenerateDungeon();
            EditorUtility.SetDirty(generator);
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Generate Rooms"))
        {
            generator.ButtonEventGenerateRooms();
            EditorUtility.SetDirty(generator);
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Reset"))
        {
            generator.ButtonEventReset();
            EditorUtility.SetDirty(generator);
        }
    }
}
