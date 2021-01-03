using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[ExecuteInEditMode]

public class CustomTerrain : MonoBehaviour
{
	public Vector2 randomHeightRange = new Vector2(0, 0.1f);
	public Texture2D heightMapImage;
	public Vector3 heightMapScale = Vector3.one;
	public Terrain terrain;
	public TerrainData terrainData;

	public void RandomTerrain()
	{
		float[,] heightMap = terrainData.GetHeights(0, 0, 
			terrainData.heightmapResolution, terrainData.heightmapResolution);

		for (int x = 0; x < terrainData.heightmapResolution; x++)
		{
			for (int z = 0; z < terrainData.heightmapResolution; z++)
			{
				heightMap[x, z] += UnityEngine.Random.Range(randomHeightRange.x, randomHeightRange.y);
			}
		}
		terrainData.SetHeights(0, 0, heightMap);
	}

	public void LoadHeightMapTexture()
	{
		float[,] heightMap;
		heightMap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];

		for (int x = 0; x < terrainData.heightmapResolution; x++)
		{
			for (int z = 0; z < terrainData.heightmapResolution; z++)
			{
				heightMap[x, z] = heightMapImage.GetPixel((int)(x * heightMapScale.x),
					(int)(z * heightMapScale.z)).grayscale * heightMapScale.y;
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
			for (int z = 0; z < terrainData.heightmapResolution; z++)
			{
				heightMap[x, z] = 0;
			}
		}
		terrainData.SetHeights(0, 0, heightMap);
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
