using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class Object3DMeshData
{
	public string id;

	[FormerlySerializedAs("objTextures")]
	public List<TexturesData> texturesData;

	public GameObject obj;

	public Object3DMeshData()
	{
	}

	public Object3DMeshData(string id, GameObject obj, int materialsCount = 1)
	{
		this.id = id;
		this.obj = obj;
		texturesData = new List<TexturesData>();
		for (int i = 0; i < materialsCount; i++)
		{
			texturesData.Add(new TexturesData());
		}
	}

	public static string GetIdWithoutMetaData(string originalString)
	{
		if (originalString.Contains("-MERGE"))
		{
			originalString = originalString.Split("-MERGE")[0];
		}
		return originalString;
	}

	public static bool NeedUnlitMaterial(string meshName)
	{
		foreach (string meshUnlitMaterialPostfix in DevUtils.MeshUnlitMaterialPostfixes)
		{
			if (meshName.EndsWith(meshUnlitMaterialPostfix))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsBlackoutMesh()
	{
		if (id.EndsWith("-blackout"))
		{
			return true;
		}
		return false;
	}
}
