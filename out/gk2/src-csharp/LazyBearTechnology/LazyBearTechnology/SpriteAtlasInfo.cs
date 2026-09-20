using System;
using System.Collections.Generic;

namespace LazyBearTechnology;

[Serializable]
public class SpriteAtlasInfo
{
	public string atlasName;

	public string path;

	public List<string> spriteNames = new List<string>();
}
