using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

[ExecuteInEditMode]

public class CustomTerrain : MonoBehaviour
{
	// Heightmap Vars -------------------------------
	public Vector2 randomHeightRange = new Vector2(0, 0.1f);
	public Texture2D heightMapImage;
	public Vector3 heightMapScale = Vector3.one;

	public bool resetTerrain = true;
	public int smoothingIterations = 1;
	public RenderTexture heightMapTexture;

	// Water ----------------------------------------
	public float waterHeight = 0.1f;
	public GameObject waterObject;
	public Material shorelineMaterial;

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

	public List<SplatHeights> splatheights = new List<SplatHeights>() { new SplatHeights() };
	string terrainLayerFolderName = "TerrainLayers (Generated)";

	// Vegetation -----------------------------------
	[System.Serializable]
	public class Vegetation
	{
		public GameObject prefab;
		public float density = 1;
		public float minHeight = 0.1f;
		public float maxHeight = 0.2f;
		public float minSlope = 0;
		public float maxSlope = 90;
		public float minHScale = 0.8f;
		public float maxHScale = 1.1f;
		public float minWScale = 0.8f;
		public float maxWScale = 1.1f;
		public Color tint1 = Color.white;
		public Color tint2 = Color.white;
		public Color lightColor = Color.white;
		public bool remove = false;
	}

	public List<Vegetation> vegetation = new List<Vegetation>() { new Vegetation() };
	public int maxTrees = 5000;
	public int treeSpacing = 5;

	// Detail ---------------------------------------
	[System.Serializable]
	public class Detail
	{
		public GameObject prototype = null;
		public Texture2D prototypeTexture = null;
		public float density = 0.5f;
		public float minHeight = 0.1f;
		public float maxHeight = 0.2f;
		public float minSlope = 0;
		public float maxSlope = 15;
		public float bendFactor = 1;
		public Color dryColor = Color.white;
		public Color healthyColor = Color.white;
		public Vector2 widthRange = new Vector2(0.95f, 1.1f);
		public Vector2 heightRange = new Vector2(0.95f, 1.1f);
		public float noiseSpread = 1f;
		public float overlap = 0.1f;
		public float feather = 0.05f;
		public bool remove = false;
	}

	public List<Detail> details = new List<Detail>() { new Detail() };
	public int maxDetails = 5000;
	public int detailSpacing = 5;

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

	public List<PerlinParameters> perlinParameters = new List<PerlinParameters>() { new PerlinParameters() };

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

	public void RefreshHeightMap() => heightMapTexture = terrainData.heightmapTexture;

	public void AddWater()
	{
		GameObject water = GameObject.Find("water");
		if (!water)
		{
			water = Instantiate(waterObject, transform.position, transform.rotation);
			water.name = "water";
		}
		water.transform.position = transform.position + new Vector3(
			terrainData.size.x / 2,
			waterHeight * terrainData.size.y,
			terrainData.size.z / 2);
		water.transform.localScale = new Vector3(terrainData.size.x*0.75f, 1, terrainData.size.z * 0.75f);
	}

	public void RemoveWater()
	{
		GameObject water = GameObject.Find("water");
		if (!water)
			return;
		DestroyImmediate(water);
	}

	public void DrawShoreline()
	{
		float[,] heightMap = terrainData.GetHeights(0, 0,
			terrainData.heightmapResolution, terrainData.heightmapResolution);

		for (int y = 0; y < terrainData.heightmapResolution; y++)
			for (int x = 0; x < terrainData.heightmapResolution; x++)
			{
				// find spot on the shore
				Vector2 thisLocation = new Vector2(x, y);
				List<Vector2> neighbors = GenerateNeighbors(
					thisLocation, terrainData.heightmapResolution, terrainData.heightmapResolution);

				foreach (Vector2 n in neighbors)
				{
					if (heightMap[x, y] < waterHeight && heightMap[(int)n.x, (int)n.y] > waterHeight)
					{
						GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
						go.transform.localScale *= 20f;

						go.transform.position = transform.position + new Vector3(
							y / (float)terrainData.heightmapResolution * terrainData.size.z,
							waterHeight * terrainData.size.y,
							x / (float)terrainData.heightmapResolution * terrainData.size.x);
						// look at the high point of the shore
						go.transform.LookAt(new Vector3(
							n.y / terrainData.heightmapResolution * terrainData.size.z,
							waterHeight * terrainData.size.y,
							n.x / terrainData.heightmapResolution * terrainData.size.x));
						// rotate mesh to look up
						go.transform.Rotate(90,0,0);
						go.tag = "Shore";
					}
				}
			}

		// prepare meshes to be combined
		GameObject[] shoreQuads = GameObject.FindGameObjectsWithTag("Shore");
		MeshFilter[] meshFilters = new MeshFilter[shoreQuads.Length];
		for (int m = 0; m < shoreQuads.Length; m++)
			meshFilters[m] = shoreQuads[m].GetComponent<MeshFilter>();
		CombineInstance[] combine = new CombineInstance[meshFilters.Length];
		int i = 0;
		while (i < meshFilters.Length)
		{
			combine[i].mesh = meshFilters[i].sharedMesh;
			combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
			meshFilters[i].gameObject.active = false;	// can not combine meshes that are active
			i++;
		}

		// construct shoreline game object+components and combine meshes
		GameObject currentShoreline = GameObject.Find("Shoreline");
		if (currentShoreline)
			DestroyImmediate(currentShoreline);
		GameObject shoreLine = new GameObject();
		shoreLine.name = "Shoreline";
		shoreLine.AddComponent<WaveAnimation>();
		shoreLine.transform.position = transform.position;
		shoreLine.transform.rotation = transform.rotation;
		MeshFilter thisMF = shoreLine.AddComponent<MeshFilter>();
		thisMF.mesh = new Mesh();
		shoreLine.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);
		MeshRenderer r = shoreLine.AddComponent<MeshRenderer>();
		r.sharedMaterial = shorelineMaterial;

		// delete original quads
		for (int sQ = 0; sQ < shoreQuads.Length; sQ++)
			DestroyImmediate(shoreQuads[sQ]);
	}

	public void RemoveShoreline()
	{
		GameObject currentShoreline = GameObject.Find("Shoreline");
		if (!currentShoreline)
			return;
		DestroyImmediate(currentShoreline);
	}

	public void PlantVegetation()
	{
		TreePrototype[] newTreePrototypes;
		newTreePrototypes = new TreePrototype[vegetation.Count];
		int tIndex = 0;
		foreach (Vegetation t in vegetation)
		{
			newTreePrototypes[tIndex] = new TreePrototype();
			newTreePrototypes[tIndex].prefab = t.prefab;
			tIndex++;
		}
		terrainData.treePrototypes = newTreePrototypes;

		List<TreeInstance> allVegetation = new List<TreeInstance>();
		for (int z = 0; z < terrainData.size.z; z += treeSpacing)
			for (int x = 0; x < terrainData.size.x; x += treeSpacing)
				for (int tp = 0; tp < terrainData.treePrototypes.Length; tp++)
				{
					// randomly decide to place based on density setting
					if (UnityEngine.Random.Range(0.0f, 1.0f) > vegetation[tp].density) continue;

					int randX = x + UnityEngine.Random.Range(-treeSpacing, treeSpacing);
					int randZ = z + UnityEngine.Random.Range(-treeSpacing, treeSpacing);

					float steepness = terrainData.GetSteepness(
						x / terrainData.size.x, z / terrainData.size.z);

					if (!(steepness <= vegetation[tp].maxSlope && steepness >= vegetation[tp].minSlope))
						continue;

					float thisHeight = terrainData.GetInterpolatedHeight(
						randX / terrainData.size.x,
						randZ / terrainData.size.z) / terrainData.size.y;
					float thisHeightStart = vegetation[tp].minHeight;
					float thisHeightEnd = vegetation[tp].maxHeight;

					if (thisHeight >= thisHeightStart && thisHeight <= thisHeightEnd)
					{
						TreeInstance instance = new TreeInstance();
						instance.position = new Vector3(
							randX / terrainData.size.x,
							thisHeight,
							randZ / terrainData.size.z);

						// This raycasts from the bottom of the tree to snap it to the ground
						// I've entirely bypassed this by generating the random x/z jitter before 
						// calculating the height value for the tree.

						/*Vector3 treeWorldPos = new Vector3(
							instance.position.x * terrainData.size.x,
							instance.position.y * terrainData.size.y,
							instance.position.z * terrainData.size.z) +
							this.transform.position;

						RaycastHit hit;
						int layerMask = 1 << terrainLayer;
						if (Physics.Raycast(treeWorldPos, Vector3.down, out hit, 100, layerMask))
						{
							float treeHeight = (hit.point.y - this.transform.position.y) / terrainData.size.y;
							instance.position = new Vector3(
								instance.position.x,
								treeHeight,
								instance.position.z);
						}*/

						instance.prototypeIndex = tp;
						instance.color = Color.Lerp(
							vegetation[tp].tint1,
							vegetation[tp].tint2,
							UnityEngine.Random.Range(0.0f, 1.0f));
						instance.lightmapColor = vegetation[tp].lightColor;
						instance.heightScale = UnityEngine.Random.Range(
							vegetation[tp].minHScale, vegetation[tp].maxHScale);
						instance.widthScale = UnityEngine.Random.Range(
							vegetation[tp].minWScale, vegetation[tp].maxWScale);

						allVegetation.Add(instance);
						if (allVegetation.Count >= maxTrees) goto TREESDONE;
					}
				}
			TREESDONE:
		terrainData.treeInstances = allVegetation.ToArray();
	}

	public void AddNewVegetation() => vegetation.Add(new Vegetation());

	public void RemoveVegetation()
	{
		List<Vegetation> keptVegitation = new List<Vegetation>();
		for (int i = 0; i < vegetation.Count; i++)
			if (!vegetation[i].remove)
				keptVegitation.Add(vegetation[i]);
		if (keptVegitation.Count == 0)    // make sure there is always one Vegetation for GUI
			keptVegitation.Add(vegetation[0]);
		vegetation = keptVegitation;
	}

	public void ApplyDetails()
	{

		DetailPrototype[] newDetailPrototypes;
		newDetailPrototypes = new DetailPrototype[details.Count];
		int dIndex = 0;

		foreach (Detail d in details)
		{
			newDetailPrototypes[dIndex] = new DetailPrototype();
			newDetailPrototypes[dIndex].prototype = d.prototype;
			newDetailPrototypes[dIndex].prototypeTexture = d.prototypeTexture;
			newDetailPrototypes[dIndex].healthyColor = d.healthyColor;
			newDetailPrototypes[dIndex].dryColor = d.dryColor;
			newDetailPrototypes[dIndex].minWidth = d.widthRange.x;
			newDetailPrototypes[dIndex].maxWidth = d.widthRange.y;
			newDetailPrototypes[dIndex].minHeight = d.heightRange.x;
			newDetailPrototypes[dIndex].minHeight = d.heightRange.y;
			newDetailPrototypes[dIndex].bendFactor = d.bendFactor;
			newDetailPrototypes[dIndex].noiseSpread = d.noiseSpread;
			if (newDetailPrototypes[dIndex].prototype)
			{
				newDetailPrototypes[dIndex].usePrototypeMesh = true;
				newDetailPrototypes[dIndex].renderMode = DetailRenderMode.VertexLit;
			}
			else
			{
				newDetailPrototypes[dIndex].usePrototypeMesh = false;
				newDetailPrototypes[dIndex].renderMode = DetailRenderMode.GrassBillboard;
			}
			dIndex++;
		}
		terrainData.detailPrototypes = newDetailPrototypes;

		float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);

		for (int i = 0; i < terrainData.detailPrototypes.Length; i++)
		{
			int[,] detailMap = new int[terrainData.detailWidth, terrainData.detailHeight];

			for (int y = 0; y < terrainData.detailHeight; y += detailSpacing)
				for (int x = 0; x < terrainData.detailWidth; x += detailSpacing)
				{
					if (UnityEngine.Random.Range(0.0f, 1.0f) > details[i].density) 
						continue;
					int xHM = (int)((x / (float)terrainData.detailWidth) * terrainData.heightmapResolution);
					int yHM = (int)((y / (float)terrainData.detailHeight) * terrainData.heightmapResolution);
					int xWU = (int)(x / (float)terrainData.detailWidth * terrainData.size.x);
					int zWU = (int)(y / (float)terrainData.detailHeight * terrainData.size.z);

					float thisNoise = Utils.Map(Mathf.PerlinNoise(
						x * details[i].feather,
						y * details[i].feather),
						0, 1, 0.5f, 1);

					float convertedMinHeight = details[i].minHeight * thisNoise -
						details[i].overlap * thisNoise;

					float convertedMaxHeight = details[i].maxHeight * thisNoise +
						details[i].overlap * thisNoise;

					float thisHeight = heightMap[yHM, xHM];

					float steepness = terrainData.GetSteepness(
						(xWU + thisNoise) / (float)terrainData.size.x,
						(zWU + thisNoise) / (float)terrainData.size.z);

					if ((thisHeight >= convertedMinHeight && thisHeight <= convertedMaxHeight) &&
						(steepness >= details[i].minSlope && steepness <= details[i].maxSlope))
					detailMap[y, x] = 1;
				}
			terrainData.SetDetailLayer(0, 0, i, detailMap);
		}
	}

	public void AddNewDetail() => details.Add(new Detail());

	public void RemoveDetail()
	{
		List<Detail> keptDetails = new List<Detail>();
		for (int i = 0; i < details.Count; i++)
			if (!details[i].remove)
				keptDetails.Add(details[i]);
		if (keptDetails.Count == 0)
			keptDetails.Add(new Detail());
		details = keptDetails;
	}

	public void SplatMaps()
	{
		// Make sure we have a folder to dump terrain layer data
		string generatedTerrainLayerPath = "Assets/" + terrainLayerFolderName;
		if (!AssetDatabase.IsValidFolder(generatedTerrainLayerPath))
			AssetDatabase.CreateFolder("Assets", terrainLayerFolderName);

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
			string path = 
				generatedTerrainLayerPath + "Scene_" + SceneManager.GetActiveScene().name + "_" + spIndex + ".terrainlayer";
			AssetDatabase.CreateAsset(newSplatPrototypes[spIndex], path);
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

	public void AddNewSplatHeight() => splatheights.Add(new SplatHeights());

	public void RemoveSplatHeight()
	{
		List<SplatHeights> keptSplatHeights = new List<SplatHeights>();
		for (int i = 0; i < splatheights.Count; i++)
			if (!splatheights[i].remove)
				keptSplatHeights.Add(splatheights[i]);
		if (keptSplatHeights.Count == 0)	// make sure there is always one SplatHeight for GUI
			keptSplatHeights.Add(splatheights[0]);
		splatheights = keptSplatHeights;
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
		RefreshHeightMap();
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
		RefreshHeightMap();
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
		RefreshHeightMap();
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
		RefreshHeightMap();
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
		RefreshHeightMap();
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
		RefreshHeightMap();
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
		RefreshHeightMap();
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
		RefreshHeightMap();
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

	public enum TagType { Tag = 0,  Layer = 1}
	[SerializeField]
	int terrainLayer = 0;

	private void Awake()
	{
		SerializedObject tagManager = new SerializedObject(
			AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
		SerializedProperty tagsProp = tagManager.FindProperty("tags");

		AddTag(tagsProp, "Terrain", TagType.Tag);
		AddTag(tagsProp, "Cloud", TagType.Tag);
		AddTag(tagsProp, "Shore", TagType.Tag);
		// Apply tag changes to tag database
		tagManager.ApplyModifiedProperties();

		SerializedProperty layerProp = tagManager.FindProperty("layers");
		terrainLayer = AddTag(layerProp, "Terrain", TagType.Layer);
		tagManager.ApplyModifiedProperties();

		// take this object
		this.gameObject.tag = "Terrain";
		this.gameObject.layer = terrainLayer;
	}

	int AddTag(SerializedProperty tagsProp, string newTag, TagType tType)
	{
		bool found = false;
		// ensure the tag doesnt already exist
		for (int i=0; i< tagsProp.arraySize; i++)
		{
			SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
			if(t.stringValue.Equals(newTag)) { found = true; return i; }
		}
		// add new tag
		if(!found && tType == TagType.Tag)
		{
			tagsProp.InsertArrayElementAtIndex(0);
			SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
			newTagProp.stringValue = newTag;
		}
		// add new layer
		else if (!found && tType == TagType.Layer)
		{
			for (int j = 8; j <  tagsProp.arraySize; j++) // start after Unity built-in layers
			{
				SerializedProperty newLayer = tagsProp.GetArrayElementAtIndex(j);
				// add new layer in next empty spot
				if (newLayer.stringValue == "")
				{
					Debug.Log("Adding new layer: " + newTag);
					newLayer.stringValue = newTag;
					return j;
				}
			}
		}
		return -1;
	}

}
