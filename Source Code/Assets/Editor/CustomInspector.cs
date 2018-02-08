using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor (typeof (BuildWorld))]
public class CustomInspector : Editor {

	public override void OnInspectorGUI() {
        BuildWorld buildWorld = (BuildWorld)target;

        DrawDefaultInspector();

        if (GUILayout.Button ("Load Configuration")) {
            buildWorld.LoadConfigurationFile();
        }

        if (GUILayout.Button("Save Configuration"))
        {
            buildWorld.SaveConfigurationFile();
        }

       
    }
}
