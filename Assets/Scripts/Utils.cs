using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
	/// <summary>
	/// <para>FBM (Fractal Brownian Motion) HeightMap Effector</para>
	/// <para>Calculates height data for a vertex using octaves of Perlin 
	/// Noise stacked and averaged on eachother.</para>
	/// </summary>
	/// <param name="x">X Location in Height Map</param>
	/// <param name="y">Y Location in Height Map</param>
	/// <param name="octaves">Number of Noise Octaves</param>
	/// <param name="persistance">Defines how much of the curve will be effected per octave</param>
	/// <param name="freqencyMultiplier">Set frequency multiplier (default=2)</param>
	/// <returns>Returns float representing height data for this vertex</returns>
	public static float FBM(float x, float y, int octaves, float persistance, float freqencyMultiplier = 2)
	{
		// total height value for the vertex
		float total = 0;
		// determines how close waves are together in noise pattern
		float frequency = 1;
		// Scales persistance per wave
		float amplitude = 1;
		// Takes in successive amplitude values to later return an average value
		float maxValue = 0;
		// Loop for each octave, maniputating the amplitude and frequency each iteration
		for (int i=0; i< octaves; i++)
		{
			total += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;
			maxValue += amplitude;
			amplitude *= persistance;
			frequency *= freqencyMultiplier;
		}
		// return relative average
		return total / maxValue;
	}
}
