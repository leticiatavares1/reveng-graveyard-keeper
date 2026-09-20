using System;
using System.Collections.Generic;

[Serializable]
public class SpawnStage
{
	public string stageName;

	public string craftId;

	public List<SpawnVariation> variations = new List<SpawnVariation>();
}
