using System;
using System.Collections.Generic;

[Serializable]
public class PhrasesByWgo
{
	public string wgoId;

	public List<string> phrases;

	public PhrasesByWgo(string wgoId, List<string> phrases)
	{
		this.wgoId = wgoId;
		this.phrases = phrases;
	}

	public PhrasesByWgo(string wgoId)
	{
		this.wgoId = wgoId;
		phrases = new List<string>();
	}
}
