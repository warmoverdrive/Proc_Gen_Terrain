using UnityEngine;
using UnityEditor;
using EditorGUITable;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]

public class CustomTerrainEditor : Editor
{
    // properties ------------
    SerializedProperty randomHeightRange;
    SerializedProperty heightMapScale;
    SerializedProperty heightMapImage;

    // fold outs -------------
    bool showRandom = false;
    bool showLoadHeights = false;

	private void OnEnable()
	{
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        heightMapScale = serializedObject.FindProperty("heightMapScale");
        heightMapImage = serializedObject.FindProperty("heightMapImage");
	}

    public override void OnInspectorGUI()
	{
        serializedObject.Update();

        CustomTerrain terrain = (CustomTerrain) target;

        showRandom = EditorGUILayout.Foldout(showRandom, "Random");
        if(showRandom)
		{
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Heights Between Random Values", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(randomHeightRange);
            if (GUILayout.Button("Apply Random Heights"))
                terrain.RandomTerrain();
		}

        showLoadHeights = EditorGUILayout.Foldout(showLoadHeights, "Load HeightMap Image");
        if(showLoadHeights)
		{
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Load HeightMap Image", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(heightMapImage);
            EditorGUILayout.PropertyField(heightMapScale);
            if (GUILayout.Button("Load & Apply Texture"))
                terrain.LoadHeightMapTexture();
		}

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if (GUILayout.Button("Reset Terrain Height"))
            terrain.ResetTerrainHeight();

        serializedObject.ApplyModifiedProperties();
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
