using System;
using System.Collections.Generic;
using UnityEngine;

namespace LazyBearTechnology;

public abstract class SkinPresetBase : ScriptableObject
{
	public static LazySkinPreset Load(string id)
	{
		throw new NotImplementedException();
	}

	public virtual int DefineSkinIdFor(char char4, char char5, char char6)
	{
		throw new NotImplementedException();
	}

	public virtual void ApplyShaderParametersTo(List<SpriteRenderer> sprites)
	{
		throw new NotImplementedException();
	}
}
