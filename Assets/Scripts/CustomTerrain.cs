using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[ExecuteInEditMode]

public class CustomTerrain : MonoBehaviour
{
	// Heightmap Vars -------------------------------
	public Vector2 randomHeightRange = new Vector2(0, 0.1f);
	public Texture2D heightMapImage;
	public Vector3 heightMapScale = Vector3.one;

	public bool resetTerrain = true;

	// Perlin Noise ---------------------------------
	public float perlinXScale = 0.01f;
	public float perlinYScale = 0.01f;
	public int perlinOffsetX = 0;
	public int perlinOffsetY = 0;
	public int perlinOctaves = 3;
	public float perlinPersistance = 8;
	public float perlinHeightScale = 0.09f;
	public float perlinFreqMultiplier = 2f;

	// Multiple Perlin ------------------------------
	[System.Serializable]
	public class PerlinParameters
	{
		public float perlinXScale = 0.01f;
		public float perlinYScale = 0.01f;
		public int perlinOffsetX = 0;
		public int perlinOffsetY = 0;
		public int perlinOctaves = 3;
		public float perlinPersistance = 0.3f;
		public float perlinHeightScale = 0.3f;
		public float perlinFreqMultiplier = 2f;
		public bool remove = false;
	}

	public List<PerlinParameters> perlinParameters = new List<PerlinParameters>()
	{ 
		new PerlinParameters()
	};

	// Terrain Data Objs ----------------------------
	public Terrain terrain;
	public TerrainData terrainData;

	float[,] GetHeightMap()
	{
		if (!resetTerrain)
			return terrainData.GetHeights(
				0, 0, terrainData.heightmapResolution, 
				terrainData.heightmapResolution);
		else
			return new float[
				terrainData.heightmapResolution, 
				terrainData.heightmapResolution];
	}

	public void Perlin()
	{
		float[,] heightMap = GetHeightMap();

		for (int y = 0; y < terrainData.heightmapResolution; y++)
		{
			for (int x = 0; x < terrainData.heightmapResolution; x++)
			{
				heightMap[x, y] += Utils.FBM(
					(x+perlinOffsetX) * perlinXScale,
					(y+perlinOffsetY) * perlinYScale,
					perlinOctaves,
					perlinPersistance,
					perlinFreqMultiplier) * perlinHeightScale;
			}
		}
		terrainData.SetHeights(0, 0, heightMap);
	}

	public void MultiplePerlinTerrain()
	{
		float[,] heightMap = GetHeightMap();
		for (int y = 0; y < terrainData.heightmapResolution; y++)
		{
			for (int x = 0; x < terrainData.heightmapResolution; x++)
			{
				foreach (PerlinParameters p in perlinParameters)
				{
					heightMap[x, y] += Utils.FBM(
						(x + p.perlinOffsetX) * p.perlinXScale,
						(y + p.perlinOffsetY) * p.perlinYScale,
						p.perlinOctaves,
						p.perlinPersistance,
						p.perlinFreqMultiplier) * p.perlinHeightScale;
				}
			}
		}
		terrainData.SetHeights(0, 0, heightMap);
	}

	public void RandomTerrain()
	{
		float[,] heightMap = GetHeightMap();

		for (int x = 0; x < terrainData.heightmapResolution; x++)
		{
			for (int y = 0; y < terrainData.heightmapResolution; y++)
			{
				heightMap[x, y] += UnityEngine.Random.Range(randomHeightRange.x, randomHeightRange.y);
			}
		}
		terrainData.SetHeights(0, 0, heightMap);
	}

	public void LoadHeightMapTexture()
	{
		float[,] heightMap = GetHeightMap();

		for (int x = 0; x < terrainData.heightmapResolution; x++)
		{
			for (int y = 0; y < terrainData.heightmapResolution; y++)
			{
				heightMap[x, y] += heightMapImage.GetPixel((int)(x * heightMapScale.x),
					(int)(y * heightMapScale.z)).grayscale * heightMapScale.y;
			}
		}
		terrainData.SetHeights(0, 0, heightMap);
	}

	public void ResetTerrainHeight()
	{
		float[,] heightMap;
		heightMap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];

		for (int x = 0; x < terrainData.heightmapResolution; x++)
		{
			for (int y = 0; y < terrainData.heightmapResolution; y++)
			{
				heightMap[x, y] = 0;
			}
		}
		terrainData.SetHeights(0, 0, heightMap);
	}

	public void AddNewPerlin() => perlinParameters.Add(new PerlinParameters());

	public void RemovePerlin()
	{
		List<PerlinParameters> keptPerlinParamenters = new List<PerlinParameters>();
		for (int i= 0; i< perlinParameters.Count; i++)
		{
			if (!perlinParameters[i].remove)
				keptPerlinParamenters.Add(perlinParameters[i]);
		}
		if (keptPerlinParamenters.Count == 0)
			keptPerlinParamenters.Add(perlinParameters[0]);
		perlinParameters = keptPerlinParamenters;
	}

	private void OnEnable()
	{
		Debug.Log("Initializing Terrain Data");
		terrain = this.GetComponent<Terrain>();
		terrainData = Terrain.activeTerrain.terrainData;
	}

	private void Awake()
	{
		SerializedObject tagManager = new SerializedObject(
			AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
		SerializedProperty tagsProp = tagManager.FindProperty("tags");

		AddTag(tagsProp, "Terrain");
		AddTag(tagsProp, "Cloud");
		AddTag(tagsProp, "Shore");

		// Apply tag changes to tag database
		tagManager.ApplyModifiedProperties();

		// take this object
		this.gameObject.tag = "Terrain";
	}

	void AddTag(SerializedProperty tagsProp, string newTag)
	{
		bool found = false;
		// ensure the tag doesnt already exist
		for (int i=0; i< tagsProp.arraySize; i++)
		{
			SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
			if(t.stringValue.Equals(newTag)) { found = true; break; }
		}
		// add new tag
		if(!found)
		{
			tagsProp.InsertArrayElementAtIndex(0);
			SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
			newTagProp.stringValue = newTag;
		}
	}

	void Start()
	{
		
	}

	void Update()
	{
		
	}
}
