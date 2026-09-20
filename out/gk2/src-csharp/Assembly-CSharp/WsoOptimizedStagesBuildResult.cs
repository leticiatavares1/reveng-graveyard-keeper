using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class WsoOptimizedStagesBuildResult
{
	public readonly List<GameObject> Instances = new List<GameObject>();

	public readonly List<AsyncOperationHandle<GameObject>> Handles = new List<AsyncOperationHandle<GameObject>>();
}
