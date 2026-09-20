using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MovementData
{
	public List<Vector3> worldPath = new List<Vector3>();

	public string transitionWorldId;

	public bool lastPointTransitToNextMovementData;
}
