using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class PathCalculationUnit : MonoBehaviour
{
	public Seeker seeker;

	private List<Vector3> vectorPath = new List<Vector3>();

	public List<Vector3> VectorPath
	{
		set
		{
			vectorPath = value;
		}
	}

	private void OnDrawGizmos()
	{
		if (vectorPath != null && vectorPath.Count >= 2)
		{
			int num = vectorPath.Count - 1;
			for (int i = 0; i < num; i++)
			{
				float t = (float)i / (float)num;
				Gizmos.color = Color.Lerp(Color.red, Color.green, t);
				Gizmos.DrawLine(vectorPath[i], vectorPath[i + 1]);
			}
		}
	}
}
