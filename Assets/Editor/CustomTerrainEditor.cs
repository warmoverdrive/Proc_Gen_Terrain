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

	SerializedProperty voronoiPeakCount;
	SerializedProperty voronoiMinHeight;
	SerializedProperty voronoiMaxHeight;
	SerializedProperty voronoiFalloff;
	SerializedProperty voronoiDropoff;
	SerializedProperty voronoiType;

	SerializedProperty MPDHeightMin;
	SerializedProperty MPDHeightMax;
	SerializedProperty MPDHeightDampenerPower;
	SerializedProperty MPDRoughness;

	// fold outs -------------
	bool showRandom = false;
	bool showLoadHeights = false;
	bool showPerlin = false;
	bool showMultiplePerlin = false;
	bool showVoronoi = false;
	bool showMPD = false;

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

		voronoiPeakCount = serializedObject.FindProperty("voronoiPeakCount");
		voronoiMinHeight = serializedObject.FindProperty("voronoiMinHeight");
		voronoiMaxHeight = serializedObject.FindProperty("voronoiMaxHeight");
		voronoiFalloff = serializedObject.FindProperty("voronoiFalloff");
		voronoiDropoff = serializedObject.FindProperty("voronoiDropoff");
		voronoiType = serializedObject.FindProperty("voronoiType");

		MPDHeightMin = serializedObject.FindProperty("MPDHeightMin");
		MPDHeightMax = serializedObject.FindProperty("MPDHeightMax");
		MPDHeightDampenerPower = serializedObject.FindProperty("MPDHeightDampenerPower");
		MPDRoughness = serializedObject.FindProperty("MPDRoughness");
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

		showVoronoi = EditorGUILayout.Foldout(showVoronoi, "Voronoi");
		if (showVoronoi)
		{
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			GUILayout.Label("Voronoi", EditorStyles.boldLabel);
			EditorGUILayout.IntSlider(voronoiPeakCount, 1, 10, new GUIContent("Number of Peaks"));
			EditorGUILayout.Slider(voronoiMinHeight, 0.0f, 1f, new GUIContent("Minimum Peak Height"));
			EditorGUILayout.Slider(voronoiMaxHeight, 0.0f, 1f, new GUIContent("Maximum Peak Height"));
			EditorGUILayout.Slider(voronoiFalloff, 0.0f, 10f, new GUIContent("Falloff"));
			EditorGUILayout.Slider(voronoiDropoff, 0.0f, 10f, new GUIContent("Dropoff"));
			EditorGUILayout.PropertyField(voronoiType);
			if (GUILayout.Button("Apply Voronoi"))
				terrain.Voronoi();
		}

		showMPD = EditorGUILayout.Foldout(showMPD, "Midpoint Displacement");
		if(showMPD)
		{
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			GUILayout.Label("Midpoint Displacement", EditorStyles.boldLabel);
			EditorGUILayout.Slider(MPDHeightMin, -20f, 0f, new GUIContent("Minimum Height"));
			EditorGUILayout.Slider(MPDHeightMax, 0f, 20f, new GUIContent("Maximum Height"));
			EditorGUILayout.Slider(MPDHeightDampenerPower, 0f, 10f, new GUIContent("Dampener Exponent"));
			EditorGUILayout.Slider(MPDRoughness, 0f, 10f, new GUIContent("Roughness"));
			if (GUILayout.Button("Run Midpoint Displacement"))
				terrain.MidPointDisplacement();
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
