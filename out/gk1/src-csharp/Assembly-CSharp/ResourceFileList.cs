using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ResourceFileList : ScriptableObject
{
	public string file_type = "prefab";

	[HideInInspector]
	public bool can_be_rescanned = true;

	public List<string> folders = new List<string>();

	public List<string> files = new List<string>();
}
