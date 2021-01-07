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
	public int smoothingIterations = 1;

	// Splatmaps ------------------------------------
	[System.Serializable]
	public class SplatHeights
	{
		public Texture2D texture = null;
		public float minHeight = 0.1f;
		public float maxHeight = 0.2f;
		public float minSlope = 0f;
		public float maxSlope = 1.5f;
		public Vector2 tileOffset = Vector2.zero;
		public Vector2 tileSize = new Vector2(50, 50);
		public float blendNoiseInputScaler = 0.01f;
		public float blendNoiseMultiplier = 0.1f;
		public float blendOffset = 0.1f;
		public bool remove = false;
	}

	public List<SplatHeights> splatheights = new List<SplatHeights>()
	{
		new SplatHeights()
	};

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

	// Voronoi --------------------------------------
	public int voronoiPeakCount = 4;
	public float voronoiMinHeight = 0.1f;
	public float voronoiMaxHeight = 0.9f;
	public float voronoiFalloff = 0.2f;
	public float voronoiDropoff = 0.6f;
	public enum VoronoiType { Linear = 0, Power = 1, Combined = 2, PowerSin = 3 }
	public VoronoiType voronoiType = VoronoiType.Linear;

	// Midpoint Displacement ------------------------
	public float MPDHeightMin = -2f;
	public float MPDHeightMax = 2f;
	public float MPDHeightDampenerPower = 2.0f;
	public float MPDRoughness = 2.0f;

	// Terrain Data Objs ----------------------------
	public Terrain terrain;
	public TerrainData terrainData;

	public void AddNewSplatHeight()
	{
		splatheights.Add(new SplatHeights());
	}

	public void RemoveSplatHeight()
	{
		List<SplatHeights> keptSplatHeights = new List<SplatHeights>();
		for (int i = 0; i < splatheights.Count; i++)
		{
			if (!splatheights[i].remove)
				keptSplatHeights.Add(splatheights[i]);
		}
		if (keptSplatHeights.Count == 0)	// make sure there is always one SplatHeight for GUI
			keptSplatHeights.Add(splatheights[0]);
		splatheights = keptSplatHeights;
	}

	float GetSteepness(float[,] heightMap, int x, int y, int width, int height)
	{
		float h = heightMap[x, y];
		int nx = x + 1;
		int ny = y + 1;

		// if on the upper edge of the map, find the gradient by going backwards
		if (nx > width - 1) nx = x - 1;
		if (ny > height - 1) ny = y - 1;

		float dx = heightMap[nx, y] - h;
		float dy = heightMap[x, ny] - h;
		Vector2 gradient = new Vector2(dx, dy);
		float steep = gradient.magnitude;

		return steep;
	}

	public void SplatMaps()
	{
		TerrainLayer[] newSplatPrototypes;
		newSplatPrototypes = new TerrainLayer[splatheights.Count];
		int spIndex = 0;
		foreach (SplatHeights sh in splatheights)
		{
			newSplatPrototypes[spIndex] = new TerrainLayer();
			newSplatPrototypes[spIndex].diffuseTexture = sh.texture;
			newSplatPrototypes[spIndex].tileOffset = sh.tileOffset;
			newSplatPrototypes[spIndex].tileSize = sh.tileSize;
			newSplatPrototypes[spIndex].diffuseTexture.Apply(true);
			spIndex++;
		}
		terrainData.terrainLayers = newSplatPrototypes;

		float[,] heightMap = terrainData.GetHeights(0, 0,
			terrainData.heightmapResolution, terrainData.heightmapResolution);
		float[,,] splatmapData = new float[
			terrainData.alphamapWidth,
			terrainData.alphamapHeight,
			terrainData.alphamapLayers];

		for (int y = 0; y < terrainData.alphamapHeight; y++)
		{
			for (int x = 0; x < terrainData.alphamapWidth; x++)
			{
				float[] splat = new float[terrainData.alphamapLayers];
				for (int i = 0; i < splatheights.Count; i++)
				{
					float noise = 
						Mathf.PerlinNoise(x * splatheights[i].blendNoiseInputScaler, y * splatheights[i].blendNoiseInputScaler) * 
						splatheights[i].blendNoiseMultiplier;
					float offset = splatheights[i].blendOffset + noise;
					float thisHeightStart = splatheights[i].minHeight - offset;
					float thisHeightStop = splatheights[i].maxHeight + offset;

					// returns a steepness in degrees
					float steepness = terrainData.GetSteepness(
						y / (float)terrainData.alphamapHeight, x / (float)terrainData.alphamapWidth);

					if ((heightMap[x, y] >= thisHeightStart && heightMap[x, y] <= thisHeightStop) &&
						(steepness >= splatheights[i].minSlope && steepness <= splatheights[i].maxSlope))
						splat[i] = 1;
				}
				NormalizeVector(splat);
				for (int j = 0; j < splatheights.Count; j++)
					splatmapData[x, y, j] = splat[j];
			}
		}
		terrainData.SetAlphamaps(0, 0, splatmapData);
	}

	void NormalizeVector(float[] v)
	{
		// Generate a normalized average value bewteen 0.0 and 1.0
		float total = 0;

		for (int i = 0; i < v.Length; i++)
			total += v[i];

		for (int i = 0; i < v.Length; i++)
			v[i] /= total;
	}

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

	List<Vector2> GenerateNeighbors(Vector2 pos, int width, int height)
	{
		List<Vector2> neighbors = new List<Vector2>();
		for (int y = -1; y <= 1; y++)
			for (int x = -1; x <= 1; x++)
				if(!(x==0 && y==0))
				{
					Vector2 nPos = new Vector2(
						Mathf.Clamp(pos.x + x, 0, width - 1),
						Mathf.Clamp(pos.y + y, 0, height - 1));
					if (!neighbors.Contains(nPos))
						neighbors.Add(nPos);
				}
		return neighbors;
	}

	public void Smooth()
	{
		float[,] currentHeightMap = terrainData.GetHeights(0, 0, 
			terrainData.heightmapResolution, terrainData.heightmapResolution);
		float[,] newHeightMap = currentHeightMap;
		float smoothProgress = 0;
		EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress",
			smoothProgress);

		for (int i = 0; i < smoothingIterations; i++)
		{
			for (int y = 0; y < terrainData.heightmapResolution; y++)
			{
				for (int x = 0; x < terrainData.heightmapResolution; x++)
				{
					float avgHeight = currentHeightMap[x, y];
					List<Vector2> neighbors = GenerateNeighbors(new Vector2(x, y),
						terrainData.heightmapResolution, terrainData.heightmapResolution);

					foreach (var n in neighbors)
						avgHeight += currentHeightMap[(int)n.x, (int)n.y];
					newHeightMap[x, y] = avgHeight / ((float)neighbors.Count + 1);
				}
			}
			currentHeightMap = newHeightMap;
			smoothProgress++;
			EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress",
				smoothProgress/smoothingIterations);
		}
		terrainData.SetHeights(0, 0, newHeightMap);
		EditorUtility.ClearProgressBar();
	}

	public void Voronoi()
	{
		float[,] heightMap = GetHeightMap();

		for (int i = 0; i < voronoiPeakCount; i++)
		{
			Vector3 peak = new Vector3(
				UnityEngine.Random.Range(0, terrainData.heightmapResolution),
				UnityEngine.Random.Range(voronoiMinHeight, voronoiMaxHeight),
				UnityEngine.Random.Range(0, terrainData.heightmapResolution));

			if (heightMap[(int)peak.x, (int)peak.z] < peak.y)
				heightMap[(int)peak.x, (int)peak.z] = peak.y;
			else continue;

			Vector2 peakLocation = new Vector2(peak.x, peak.z);
			float maxDistance = Vector2.Distance(Vector2.zero,
				new Vector2(terrainData.heightmapResolution, terrainData.heightmapResolution));

			for (int y = 0; y < terrainData.heightmapResolution; y++)
			{
				for (int x = 0; x < terrainData.heightmapResolution; x++)
				{
					float distanceToPeak = Vector2.Distance(
							peakLocation, new Vector2(x, y)) / maxDistance;
					float h;

					if (voronoiType == VoronoiType.Combined)
						h = peak.y - distanceToPeak * voronoiFalloff -
							Mathf.Pow(distanceToPeak, voronoiDropoff);

					else if (voronoiType == VoronoiType.PowerSin)
						h = peak.y - Mathf.Pow(distanceToPeak * 3, voronoiFalloff) - 
							(Mathf.Sin(distanceToPeak * 2 * Mathf.PI) / voronoiDropoff);

					else if (voronoiType == VoronoiType.Power)
						h = peak.y - Mathf.Pow(distanceToPeak, voronoiDropoff) * voronoiFalloff;

					else	// Linear
						h = peak.y - distanceToPeak * voronoiFalloff;

					// only adjust height if existing point is less than new value
					if (heightMap[x, y] < h)
						heightMap[x, y] = h;
				}
			}
		}

		terrainData.SetHeights(0, 0, heightMap);
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

	public void MidPointDisplacement()
	{
		float[,] heightMap = GetHeightMap();
		int width = terrainData.heightmapResolution - 1;
		int squareSize = width;
		float heightMin = MPDHeightMin;
		float heightMax = MPDHeightMax;
		float heightDampener = (float)Mathf.Pow(MPDHeightDampenerPower, -1 * MPDRoughness);

		int cornerX, cornerY;
		int midX, midY;
		int pmidXL, pmidXR, pmidYU, pmidYD;

/*	Sets random heights to corners, not super attractive imo
		heightMap[0, 0] = UnityEngine.Random.Range(0f, 0.2f);
		heightMap[0, terrainData.heightmapResolution - 2] = UnityEngine.Random.Range(0f, 0.2f);
		heightMap[terrainData.heightmapResolution - 2, 0] = UnityEngine.Random.Range(0f, 0.2f);
		heightMap[terrainData.heightmapResolution - 2, terrainData.heightmapResolution - 2] =
			UnityEngine.Random.Range(0f, 0.2f);
*/

		while (squareSize > 0)
		{
			// Diamond Step
			for (int x = 0; x < width; x += squareSize)
			{
				for (int y = 0; y < width; y += squareSize)
				{
					cornerX = x + squareSize;
					cornerY = y + squareSize;

					// calc halfway points
					midX = (int)(x + squareSize / 2.0f);
					midY = (int)(y + squareSize / 2.0f);

					// calculate average
					heightMap[midX, midY] =
						(heightMap[x, y] + heightMap[cornerX, y] +
						heightMap[x, cornerY] + heightMap[cornerX, cornerY]) / 4.0f +
						UnityEngine.Random.Range(heightMin, heightMax);
				}
			}
			// Square Step
			for (int x = 0; x < width; x += squareSize)
			{
				for (int y = 0; y < width; y += squareSize)
				{
					cornerX = (x + squareSize);
					cornerY = (y + squareSize);

					// calc halfway points
					midX = (int)(x + squareSize / 2.0f);
					midY = (int)(y + squareSize / 2.0f);

					// calc square points (midpoints between corners)
					pmidXR = midX + squareSize;
					pmidYU = midY + squareSize;
					pmidXL = midX - squareSize;
					pmidYD = midY - squareSize;

					if (pmidXL <= 0 || pmidYD <= 0
						|| pmidXR >= width - 1 || pmidYU >= width - 1) continue;

					// square value for bottom side
					heightMap[midX, y] = 
						(heightMap[midX,pmidYD] + heightMap[midX, midY] +
						heightMap[x,y] + heightMap[cornerX, y]) / 4.0f + 
						UnityEngine.Random.Range(heightMin, heightMax);
					// top side
					heightMap[midX, cornerY] =
						(heightMap[midX, pmidYU] + heightMap[midX, midY] +
						heightMap[x, cornerY] + heightMap[cornerX, cornerY]) / 4.0f +
						UnityEngine.Random.Range(heightMin, heightMax);
					// left side
					heightMap[x, midY] =
						(heightMap[pmidXL, midY] + heightMap[midX, midY] +
						heightMap[x, cornerY] + heightMap[x, y]) / 4.0f +
						UnityEngine.Random.Range(heightMin, heightMax);
					// right side
					heightMap[cornerX, midY] =
						(heightMap[pmidXR, midY] + heightMap[midX, midY] +
						heightMap[cornerX, cornerY] + heightMap[cornerX, y]) / 4.0f +
						UnityEngine.Random.Range(heightMin, heightMax);
				}
			}
			squareSize = (int)(squareSize / 2.0f);
			heightMax *= heightDampener;
			heightMin *= heightDampener;
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
