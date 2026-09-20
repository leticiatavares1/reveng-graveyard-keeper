using System;
using UnityEngine;

public class DockPointMarker : MonoBehaviour
{
	public GameObject right;

	public GameObject left;

	public GameObject down;

	public GameObject up;

	public GameObject GetMarker(Direction dir)
	{
		return dir switch
		{
			Direction.Right => left, 
			Direction.Up => down, 
			Direction.Left => right, 
			Direction.Down => up, 
			_ => throw new ArgumentOutOfRangeException("dir", dir, null), 
		};
	}
}
