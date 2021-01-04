using UnityEngine;
using UnityEditor;
using EditorGUITable;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]

public class CustomTerrainEditor : Editor
{
	// properties ------------
	SerializedProperty resetTerrain;
	SerializedProperty randomHeightRange;
	SerializedProperty heightMapScale;
	SerializedProperty heightMapImage;
	SerializedProperty perlinXScale;
	SerializedProperty perlinYScale;
	SerializedProperty perlinOffsetX;
	SerializedProperty perlinOffsetY;
	SerializedProperty perlinOctaves;
	SerializedProperty perlinPersistance;
	SerializedProperty perlinHeightScale;
	SerializedProperty perlinFreqMultiplier;

	GUITableState perlinParameterTable;
	SerializedProperty perlinParameters;

	// fold outs -------------
	bool showRandom = false;
	bool showLoadHeights = false;
	bool showPerlin = false;
	bool showMultiplePerlin = false;

	private void OnEnable()
	{
		resetTerrain = serializedObject.FindProperty("resetTerrain");
		randomHeightRange = serializedObject.FindProperty("randomHeightRange");
		heightMapScale = serializedObject.FindProperty("heightMapScale");
		heightMapImage = serializedObject.FindProperty("heightMapImage");
		perlinXScale = serializedObject.FindProperty("perlinXScale");
		perlinYScale = serializedObject.FindProperty("perlinYScale");
		perlinOffsetX = serializedObject.FindProperty("perlinOffsetX");
		perlinOffsetY = serializedObject.FindProperty("perlinOffsetY");
		perlinOctaves = serializedObject.FindProperty("perlinOctaves");
		perlinPersistance = serializedObject.FindProperty("perlinPersistance");
		perlinHeightScale = serializedObject.FindProperty("perlinHeightScale");
		perlinFreqMultiplier = serializedObject.FindProperty("perlinFreqMultiplier");

		perlinParameterTable = new GUITableState("perlineParameterTable");
		perlinParameters = serializedObject.FindProperty("perlineParameters");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		CustomTerrain terrain = (CustomTerrain) target;
		EditorGUILayout.PropertyField(resetTerrain);

		showMultiplePerlin = EditorGUILayout.Foldout(showMultiplePerlin, "Multiple Perlin Noise");
		if (showMultiplePerlin)
		{
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			GUILayout.Label("Multiple Perlin Noise", EditorStyles.boldLabel);
			perlinParameterTable = GUITableLayout.DrawTable(
				perlinParameterTable,
				serializedObject.FindProperty("perlinParameters"));
			EditorGUILayout.Space(20);
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("+"))
				terrain.AddNewPerlin();
			if (GUILayout.Button("-"))
				terrain.RemovePerlin();
			EditorGUILayout.EndHorizontal();
			if (GUILayout.Button("Apply Multiple Perlin"))
				terrain.MultiplePerlinTerrain();
		}

		showPerlin = EditorGUILayout.Foldout(showPerlin, "Single Perlin Noise");
		if (showPerlin)
		{
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			GUILayout.Label("Perlin Noise", EditorStyles.boldLabel);
			EditorGUILayout.Slider(perlinXScale, 0, 0.1f, new GUIContent("X Scale"));
			EditorGUILayout.Slider(perlinYScale, 0, 0.1f, new GUIContent("Y Scale"));
			EditorGUILayout.IntSlider(perlinOffsetX, 0, 10000, new GUIContent("X Offset"));
			EditorGUILayout.IntSlider(perlinOffsetY, 0, 10000, new GUIContent("Y Offset"));
			EditorGUILayout.IntSlider(perlinOctaves, 1, 10, new GUIContent("Octaves"));
			EditorGUILayout.Slider(perlinPersistance, 0f, 1f, new GUIContent("Persistance"));
			EditorGUILayout.Slider(perlinHeightScale, 0, 1, new GUIContent("Height Scale"));
			EditorGUILayout.Slider(perlinFreqMultiplier, 1, 5, new GUIContent("Freqency Multiplier"));
			if (GUILayout.Button("Generate and Apply Perlin Heights"))
				terrain.Perlin();
		}

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
