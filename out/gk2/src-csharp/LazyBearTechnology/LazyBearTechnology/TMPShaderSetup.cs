using System;
using System.Collections.Generic;
using UnityEngine;

namespace LazyBearTechnology;

[CreateAssetMenu(menuName = "LazyFont/TMPShaderSetup", fileName = "TMPShaderSetup")]
public class TMPShaderSetup : LazySingletonSO<TMPShaderSetup>
{
	[Serializable]
	public class ShaderConditionData
	{
		public bool outline;

		public bool secondOutline;

		public bool shadow;

		public bool eightSide;

		public Shader shader;
	}

	[SerializeField]
	private List<ShaderConditionData> shaderDataList = new List<ShaderConditionData>();

	private Shader Default => LazySingletonSO<TMPShaderSetup>.Instance.shaderDataList[0].shader;

	public static Shader FindShader(bool outline, bool secondOutline, bool shadow, bool overlayTexture, bool eightSide)
	{
		Shader shader = LazySingletonSO<TMPShaderSetup>.Instance.shaderDataList.Find((ShaderConditionData x) => x.outline == outline && x.secondOutline == secondOutline && x.shadow == shadow && x.eightSide == eightSide)?.shader;
		if (shader == null)
		{
			Debug.LogWarning($"Cant find shader for setup:(outline:[{outline}] shadow:[{shadow}] overlayTexture:[{overlayTexture}] eightSide:[{eightSide}]) return default");
			return LazySingletonSO<TMPShaderSetup>.Instance.Default;
		}
		return shader;
	}

	public void AddDefault()
	{
		if (shaderDataList.Count == 0)
		{
			ShaderConditionData shaderConditionData = new ShaderConditionData();
			shaderConditionData.shader = Shader.Find("TextMeshPro/Pixel Font Simple");
			shaderDataList.Add(shaderConditionData);
		}
	}
}
