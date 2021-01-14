using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudManager : MonoBehaviour
{
	private void OnDrawGizmosSelected()
	{
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.color = new Color(0,0,1,0.25f);
		Gizmos.DrawCube(Vector3.zero, Vector3.one);
		Gizmos.DrawLine(Vector3.zero, Vector3.one);
	}
}
