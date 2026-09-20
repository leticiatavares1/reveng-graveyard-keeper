using System;
using UnityEngine;

namespace NGTools;

[ExecuteInEditMode]
public class EditorExitBehaviour : MonoBehaviour
{
	public Action callback;

	protected virtual void OnDestroy()
	{
		if (callback != null)
		{
			callback();
		}
	}
}
