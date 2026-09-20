using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class WsoConstructorPartsBuildResult
{
	public readonly List<ConstructorPart> Parts = new List<ConstructorPart>();

	public readonly List<AsyncOperationHandle<Texture2D>> LutHandles = new List<AsyncOperationHandle<Texture2D>>();
}
