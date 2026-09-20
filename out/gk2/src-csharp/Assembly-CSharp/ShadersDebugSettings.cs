using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[CreateAssetMenu(fileName = "DebugShaders")]
public class ShadersDebugSettings : LazySingletonSO<ShadersDebugSettings>
{
	[Serializable]
	public class ShaderPreset
	{
		public Material material;

		public List<Shader> shaderVariants;
	}

	[Tooltip("Global shaders will be applied to materials alongside preset shaders")]
	public List<Shader> globalShaders;

	[Tooltip("Note: you don't need to add original shader that is currently applied to a material")]
	public List<ShaderPreset> overridableMaterials;

	public List<ShaderPreset> seaMaterials = new List<ShaderPreset>();
}
