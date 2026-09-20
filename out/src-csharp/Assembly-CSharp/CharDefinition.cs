using System;
using System.Collections.Generic;

[Serializable]
public class CharDefinition : BalanceBaseObject
{
	public List<string> jobs_can_do = new List<string>();

	public float time_k = 1f;

	public GameRes perc_vector = new GameRes();
}
