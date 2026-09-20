using System.Collections.Generic;

public class InteractionInfos
{
	public List<InteractionInfo> list = new List<InteractionInfo>();

	public bool IsEmpty => list.Count == 0;

	public InteractionInfos()
	{
	}

	public InteractionInfos(InteractionInfo interactionInfo)
	{
		list = new List<InteractionInfo>();
		Add(interactionInfo);
	}

	public void Add(InteractionInfo interactionInfo)
	{
		list.Add(interactionInfo);
	}
}
