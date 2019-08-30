using UnityEditor;
using UnityEditor.Experimental.Rendering;
using UnityEngine;

[CustomEditor(typeof(MyPipelineAsset))]
public class MyPipelineAssetEditor : Editor {

	SerializedProperty shadowCascades;
	SerializedProperty twoCascadesSplit;
	SerializedProperty fourCascadesSplit;

	void OnEnable () {
		shadowCascades = serializedObject.FindProperty("shadowCascades");
		twoCascadesSplit = serializedObject.FindProperty("twoCascadesSplit");
		fourCascadesSplit = serializedObject.FindProperty("fourCascadesSplit");
	}

	public override void OnInspectorGUI () {
		DrawDefaultInspector();

		switch (shadowCascades.enumValueIndex) {
			case 0: return;
			case 1:
				CoreEditorUtils.DrawCascadeSplitGUI<float>(ref twoCascadesSplit);
				break;
			case 2:
				CoreEditorUtils.DrawCascadeSplitGUI<Vector3>(
					ref fourCascadesSplit
				);
				break;
		}
		serializedObject.ApplyModifiedProperties();
	}
}