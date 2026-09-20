using UnityEngine;

namespace DefaultNamespace;

public class TestObject : MonoBehaviour
{
	private void OutputObjToWorldMatrix()
	{
		OutputMatrix(base.transform.localToWorldMatrix);
	}

	private void OutputWorldToObjMatrix()
	{
		OutputMatrix(base.transform.worldToLocalMatrix);
	}

	private void OutputWorldToCameraMatrix()
	{
		OutputMatrix(Camera.main.worldToCameraMatrix * base.transform.localToWorldMatrix);
	}

	private void OutputMatrix(Matrix4x4 m, float k = 1f)
	{
		Debug.Log(m);
		string text = "";
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				if (text.Length > 0)
				{
					text += "; ";
				}
				text += $"{m[i, j] * k:0.000000}";
			}
		}
		text = text.Replace(",", ".");
		text = text.Replace(";", ",");
		text = "float3x3(" + text + ")";
		Debug.Log(text);
	}
}
