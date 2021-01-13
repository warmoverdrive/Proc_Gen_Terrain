using UnityEngine;
using UnityEditor;
using EditorGUITable;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]

public class CustomTerrainEditor : Editor
{
	// properties ------------
	SerializedProperty resetTerrain;
	SerializedProperty smoothingIterations;
	SerializedProperty randomHeightRange;
	SerializedProperty heightMapScale;
	SerializedProperty heightMapImage;

	SerializedProperty waterHeight;
	SerializedProperty waterObject;
	SerializedProperty shorelineMaterial;

	SerializedProperty erosionType;
	SerializedProperty erosionStrength;
	SerializedProperty erosionAmount;
	SerializedProperty springsPerRiver;
	SerializedProperty solubility;
	SerializedProperty droplets;
	SerializedProperty windDirection;
	SerializedProperty erosionIterations;
	SerializedProperty erosionSmoothAmount;

	GUITableState splatMapTable;

	GUITableState vegetationTable;
	SerializedProperty maxTrees;
	SerializedProperty treeSpacing;

	GUITableState detailsTable;
	SerializedProperty maxDetails;
	SerializedProperty detailSpacing;

	SerializedProperty perlinXScale;
	SerializedProperty perlinYScale;
	SerializedProperty perlinOffsetX;
	SerializedProperty perlinOffsetY;
	SerializedProperty perlinOctaves;
	SerializedProperty perlinPersistance;
	SerializedProperty perlinHeightScale;
	SerializedProperty perlinFreqMultiplier;

	GUITableState perlinParameterTable;

	SerializedProperty voronoiPeakCount;
	SerializedProperty voronoiMinHeight;
	SerializedProperty voronoiMaxHeight;
	SerializedProperty voronoiFalloff;
	SerializedProperty voronoiDropoff;
	SerializedProperty voronoiType;

	SerializedProperty numberOfClouds;
	SerializedProperty particlesPerCloud;
	SerializedProperty cloudParticleSize;
	SerializedProperty cloudSizeMin;
	SerializedProperty cloudSizeMax;
	SerializedProperty cloudMaterial;
	SerializedProperty cloudShadowMaterial;
	SerializedProperty cloudColor;
	SerializedProperty cloudLiningColor;
	SerializedProperty cloudMinSpeed;
	SerializedProperty cloudMaxSpeed;
	SerializedProperty cloudDistancedTravelled;

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
	bool showSmooth = false;
	bool showSplatMaps = false;
	bool showVegetation = false;
	bool showDetails = false;
	bool showWater = false;
	bool showErosion = false;
	bool showClouds = false;
	bool showHeightMap = false;

	private void OnEnable()
	{
		resetTerrain = serializedObject.FindProperty("resetTerrain");
		smoothingIterations = serializedObject.FindProperty("smoothingIterations");

		randomHeightRange = serializedObject.FindProperty("randomHeightRange");
		heightMapScale = serializedObject.FindProperty("heightMapScale");
		heightMapImage = serializedObject.FindProperty("heightMapImage");

		waterHeight = serializedObject.FindProperty("waterHeight");
		waterObject = serializedObject.FindProperty("waterObject");
		shorelineMaterial = serializedObject.FindProperty("shorelineMaterial");

		erosionType = serializedObject.FindProperty("erosionType");
		erosionStrength = serializedObject.FindProperty("erosionStrength");
		erosionAmount = serializedObject.FindProperty("erosionAmount");
		springsPerRiver = serializedObject.FindProperty("springsPerRiver");
		solubility = serializedObject.FindProperty("solubility");
		droplets = serializedObject.FindProperty("droplets");
		windDirection = serializedObject.FindProperty("windDirection");
		erosionIterations = serializedObject.FindProperty("erosionIterations");
		erosionSmoothAmount = serializedObject.FindProperty("erosionSmoothAmount");

		splatMapTable = new GUITableState("splatMapTable");

		vegetationTable = new GUITableState("vegetationTable");
		maxTrees = serializedObject.FindProperty("maxTrees");
		treeSpacing = serializedObject.FindProperty("treeSpacing");

		detailsTable = new GUITableState("detailsTable");
		maxDetails = serializedObject.FindProperty("maxDetails");
		detailSpacing = serializedObject.FindProperty("detailSpacing");

		perlinXScale = serializedObject.FindProperty("perlinXScale");
		perlinYScale = serializedObject.FindProperty("perlinYScale");
		perlinOffsetX = serializedObject.FindProperty("perlinOffsetX");
		perlinOffsetY = serializedObject.FindProperty("perlinOffsetY");
		perlinOctaves = serializedObject.FindProperty("perlinOctaves");
		perlinPersistance = serializedObject.FindProperty("perlinPersistance");
		perlinHeightScale = serializedObject.FindProperty("perlinHeightScale");
		perlinFreqMultiplier = serializedObject.FindProperty("perlinFreqMultiplier");

		perlinParameterTable = new GUITableState("perlineParameterTable");

		voronoiPeakCount = serializedObject.FindProperty("voronoiPeakCount");
		voronoiMinHeight = serializedObject.FindProperty("voronoiMinHeight");
		voronoiMaxHeight = serializedObject.FindProperty("voronoiMaxHeight");
		voronoiFalloff = serializedObject.FindProperty("voronoiFalloff");
		voronoiDropoff = serializedObject.FindProperty("voronoiDropoff");
		voronoiType = serializedObject.FindProperty("voronoiType");

		numberOfClouds = serializedObject.FindProperty("numberOfClouds");
		particlesPerCloud = serializedObject.FindProperty("particlesPerCloud");
		cloudParticleSize = serializedObject.FindProperty("cloudParticleSize");
		cloudSizeMin = serializedObject.FindProperty("cloudSizeMin");
		cloudSizeMax = serializedObject.FindProperty("cloudSizeMax");
		cloudMaterial = serializedObject.FindProperty("cloudMaterial");
		cloudShadowMaterial = serializedObject.FindProperty("cloudShadowMaterial");
		cloudColor = serializedObject.FindProperty("cloudColor");
		cloudLiningColor = serializedObject.FindProperty("cloudLiningColor");
		cloudMinSpeed = serializedObject.FindProperty("cloudMinSpeed");
		cloudMaxSpeed = serializedObject.FindProperty("cloudMaxSpeed");
		cloudDistancedTravelled = serializedObject.FindProperty("cloudDistanceTravelled");

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
			EditorGUILayout.Slider(perlinHeightScale, -1, 1, new GUIContent("Height Scale"));
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

		showSmooth = EditorGUILayout.Foldout(showSmooth, "Smoothing");
		if (showSmooth)
		{
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			GUILayout.Label("Smoothing Options", EditorStyles.boldLabel);
			EditorGUILayout.IntSlider(smoothingIterations, 1, 10, new GUIContent("Smoothing Iterations"));
			if (GUILayout.Button("Apply Smoothing"))
				terrain.Smooth(smoothingIterations.intValue);
		}

		showSplatMaps = EditorGUILayout.Foldout(showSplatMaps, "Splat Maps (Texturing)");
		if (showSplatMaps)
		{
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			GUILayout.Label("Splat Maps", EditorStyles.boldLabel);
			splatMapTable = GUITableLayout.DrawTable(splatMapTable, serializedObject.FindProperty("splatheights"));
			EditorGUILayout.Space(20);
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("+"))
				terrain.AddNewSplatHeight();
			if (GUILayout.Button("-"))
				terrain.RemoveSplatHeight();
			EditorGUILayout.EndHorizontal();
			if (GUILayout.Button("Apply SplatMaps"))
				terrain.SplatMaps();
		}

		showVegetation = EditorGUILayout.Foldout(showVegetation, "Vegetation");
		if (showVegetation)
		{
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			GUILayout.Label("Vegetation", EditorStyles.boldLabel);
			EditorGUILayout.IntSlider(maxTrees, 0, 50000, new GUIContent("Maximum Trees"));
			EditorGUILayout.IntSlider(treeSpacing, 2, 100, new GUIContent("Tree Spacing"));
			vegetationTable = GUITableLayout.DrawTable(vegetationTable, serializedObject.FindProperty("vegetation"));
			EditorGUILayout.Space(20);
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("+"))
				terrain.AddNewVegetation();
			if (GUILayout.Button("-"))
				terrain.RemoveVegetation();
			EditorGUILayout.EndHorizontal();
			if (GUILayout.Button("Plant Vegetation"))
				terrain.PlantVegetation();
		}

		showDetails = EditorGUILayout.Foldout(showDetails, "Detail");
		if (showDetails)
		{
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			GUILayout.Label("Detail", EditorStyles.boldLabel);
			EditorGUILayout.IntSlider(maxDetails, 0, 10000, new GUIContent("Maximum Details"));
			EditorGUILayout.IntSlider(detailSpacing, 1, 100, new GUIContent("Detail Spacing"));
			detailsTable = GUITableLayout.DrawTable(detailsTable, serializedObject.FindProperty("details"));
			terrain.GetComponent<Terrain>().detailObjectDistance = maxDetails.intValue;
			EditorGUILayout.Space(20);
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("+"))
				terrain.AddNewDetail();
			if (GUILayout.Button("-"))
				terrain.RemoveDetail();
			EditorGUILayout.EndHorizontal();
			if (GUILayout.Button("Apply Details"))
				terrain.ApplyDetails();
		}

		showWater = EditorGUILayout.Foldout(showWater, "Water");
		if (showWater)
		{
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			GUILayout.Label("Water", EditorStyles.boldLabel);
			EditorGUILayout.Slider(waterHeight, 0f, 1f, new GUIContent("Water Height"));
			EditorGUILayout.PropertyField(waterObject);
			if (GUILayout.Button("Add Water"))
				terrain.AddWater();
			if (GUILayout.Button("Remove Water"))
				terrain.RemoveWater();
			EditorGUILayout.PropertyField(shorelineMaterial);
			if (GUILayout.Button("Draw Shoreline"))
				terrain.DrawShoreline();
			if (GUILayout.Button("Remove Shoreline"))
				terrain.RemoveShoreline();
		}

		showErosion = EditorGUILayout.Foldout(showErosion, "Erosion");
		if (showErosion)
		{
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			GUILayout.Label("Erosion", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(erosionType, new GUIContent("Erosion Type"));
			EditorGUILayout.Slider(erosionStrength, 0f, 1f, new GUIContent("Erosion Strength"));
			EditorGUILayout.Slider(erosionAmount, 0f, 1f, new GUIContent("Erosion Amount"));
			EditorGUILayout.IntSlider(droplets, 0, 5000, new GUIContent("Droplets"));
			EditorGUILayout.Slider(solubility, 0.001f, 1f, new GUIContent("Solubility"));
			EditorGUILayout.IntSlider(springsPerRiver, 0, 20, new GUIContent("Springs per River"));
			EditorGUILayout.Slider(windDirection, 0, 359, new GUIContent("Wind Direction"));
			EditorGUILayout.IntSlider(erosionIterations, 1, 20, new GUIContent("Erosion Iterations"));
			EditorGUILayout.IntSlider(erosionSmoothAmount, 0, 10, new GUIContent("Smooth Amount"));
			if (GUILayout.Button("Erode"))
				terrain.Erode();
		}

		showClouds = EditorGUILayout.Foldout(showClouds, "Clouds");
		if (showClouds)
		{
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			GUILayout.Label("Erosion", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(numberOfClouds, new GUIContent("Number of Clouds"));
			EditorGUILayout.PropertyField(particlesPerCloud, new GUIContent("Particles per Cloud"));
			EditorGUILayout.PropertyField(cloudParticleSize, new GUIContent("Particle Size"));
			EditorGUILayout.PropertyField(cloudSizeMin, new GUIContent("Cloud Size Min"));
			EditorGUILayout.PropertyField(cloudSizeMax, new GUIContent("Cloud Size Max"));
			EditorGUILayout.PropertyField(cloudMaterial, true);
			EditorGUILayout.PropertyField(cloudShadowMaterial, true);
			EditorGUILayout.PropertyField(cloudColor, new GUIContent("Color"));
			EditorGUILayout.PropertyField(cloudLiningColor, new GUIContent("Lining Color"));
			EditorGUILayout.PropertyField(cloudMinSpeed, new GUIContent("Min Speed"));
			EditorGUILayout.PropertyField(cloudMaxSpeed, new GUIContent("Max Speed"));
			EditorGUILayout.PropertyField(cloudDistancedTravelled, new GUIContent("Distance Travelled"));
			if (GUILayout.Button("Generate Clouds"))
				terrain.GenerateClouds();
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

		showHeightMap = EditorGUILayout.Foldout(showHeightMap, "Height Map");
		if (showHeightMap)
		{
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			int wSize = (int)(EditorGUIUtility.currentViewWidth - 150);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label(terrain.heightMapTexture, GUILayout.Width(wSize), GUILayout.Height(wSize));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		serializedObject.ApplyModifiedProperties();
	}
}
