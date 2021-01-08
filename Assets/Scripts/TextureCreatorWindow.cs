﻿using UnityEditor;
using UnityEngine;
using System.IO;

public class TextureCreatorWindow : EditorWindow
{
    string fileName = "myProceduralTexture";
    float perlinXScale;
    float perlinYScale;
    int perlinOctaves;
    float perlinPersistance;
    float perlinHeightScale;
    int perlinOffsetX;
    int perlinOffsetY;
    bool alphaToggle = false;
    bool seamlessToggle = false;
    bool mapToggle = false;

    float brightness = 0.5f;
    float contrast = 0.5f;

    Texture2D pTexture;

    [MenuItem("Window/Texture Creator Window")]
    public static void ShowWindow()
	{
        GetWindow(typeof(TextureCreatorWindow));
	}

	private void OnEnable()
	{
        pTexture = new Texture2D(513, 513, TextureFormat.ARGB32, false);
	}

	private void OnGUI()
	{
        GUILayout.Label("Settings", EditorStyles.boldLabel);
        fileName = EditorGUILayout.TextField("Texture Name", fileName);

        int wSize = (int)(EditorGUIUtility.currentViewWidth - 100);

        perlinXScale = EditorGUILayout.Slider("X Scale", perlinXScale, 0, 0.1f);
        perlinYScale = EditorGUILayout.Slider("Y Scale", perlinYScale, 0, 0.1f);
        perlinOctaves = EditorGUILayout.IntSlider("Octaves", perlinOctaves, 1, 10);
        perlinPersistance = EditorGUILayout.Slider("Persistance", perlinPersistance, 1, 10);
        perlinHeightScale = EditorGUILayout.Slider("Height Scale", perlinHeightScale, 0, 1);
        perlinOffsetX = EditorGUILayout.IntSlider("Offset X", perlinOffsetX, 0, 10000);
        perlinOffsetY = EditorGUILayout.IntSlider("Offset Y", perlinOffsetY, 0, 10000);
        brightness = EditorGUILayout.Slider("Brightness", brightness, 0, 2);
        contrast = EditorGUILayout.Slider("Contrast", contrast, 0, 2);
        alphaToggle = EditorGUILayout.Toggle("Alpha?", alphaToggle);
        mapToggle = EditorGUILayout.Toggle("Map?", mapToggle);
        seamlessToggle = EditorGUILayout.Toggle("Seamless", seamlessToggle);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Generate", GUILayout.Width(wSize)))
            GenerateTexture();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        // Texture view window
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(pTexture, GUILayout.Width(wSize), GUILayout.Height(wSize));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Save", GUILayout.Width(wSize)))
            SaveTexture();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
	}

    void GenerateTexture()
	{
        int w = 513;
        int h = 513;

        float minColor = 1;
        float maxColor = 0;

        float pValue;
        Color pixCol;

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
			{
                if (seamlessToggle)
                {
                    float u = (float)x / (float)w;
                    float v = (float)y / (float)h;
                    float noise00 = Utils.FBM(
                        (x + perlinOffsetX) * perlinXScale,
                        (y + perlinOffsetY) * perlinYScale,
                        perlinOctaves,
                        perlinPersistance) * perlinHeightScale;
                    float noise01 = Utils.FBM(
                        (x + perlinOffsetX) * perlinXScale,
                        (y + perlinOffsetY + h) * perlinYScale,
                        perlinOctaves,
                        perlinPersistance) * perlinHeightScale;
                    float noise10 = Utils.FBM(
                        (x + perlinOffsetX + w) * perlinXScale,
                        (y + perlinOffsetY) * perlinYScale,
                        perlinOctaves,
                        perlinPersistance) * perlinHeightScale;
                    float noise11 = Utils.FBM(
                        (x + perlinOffsetX + w) * perlinXScale,
                        (y + perlinOffsetY + h) * perlinYScale,
                        perlinOctaves,
                        perlinPersistance) * perlinHeightScale;

                    float noiseTotal = u * v * noise00 +
                        u * (1 - v) * noise01 +
                        (1 - u) * v * noise10 +
                        (1 - u) * (1 - v) * noise11;

                    float value = (int)(256 * noiseTotal) + 50; // 50 is an arbitrary offset value
                    float r = Mathf.Clamp((int)noise00, 0, 255);
                    float g = Mathf.Clamp(value, 0, 255);
                    float b = Mathf.Clamp(value + 50, 0, 255); // 50 is an arbitrary offset value
                    //float a = Mathf.Clamp(value + 100, 0, 255); 

                    pValue = (r + g + b) / (3 * 255.0f);
                }
                else
                {
                    pValue = Utils.FBM(
                        (x + perlinOffsetX) * perlinXScale,
                        (y + perlinOffsetY) * perlinYScale,
                        perlinOctaves,
                        perlinPersistance) * perlinHeightScale;
                }

                // contrast/brightness integration
                float colValue = contrast * (pValue - 0.5f) + 0.5f * brightness;
                if (minColor > colValue) minColor = colValue;
                if (maxColor < colValue) maxColor = colValue;
                pixCol = new Color(colValue, colValue, colValue, alphaToggle ? colValue : 1);
                pTexture.SetPixel(x, y, pixCol);
			}
        if (mapToggle)
		{
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
				{
                    pixCol = pTexture.GetPixel(x, y);
                    float colValue = pixCol.r;  // we only need 1 channel for greyscale calcs
                    colValue = Utils.Map(colValue, minColor, maxColor, 0, 1);
                    pixCol.r = colValue;
                    pixCol.b = colValue;
                    pixCol.g = colValue;
                    pixCol.a = alphaToggle ? colValue : 1;
                    pTexture.SetPixel(x, y, pixCol);
				}
		}
        pTexture.Apply(false, false);
	}

    void SaveTexture()
	{
        byte[] bytes = pTexture.EncodeToPNG();
        System.IO.Directory.CreateDirectory(Application.dataPath + "/SavedTextures");
        File.WriteAllBytes(Application.dataPath + "/SavedTextures/" + fileName + ".png", bytes);
	}
}
