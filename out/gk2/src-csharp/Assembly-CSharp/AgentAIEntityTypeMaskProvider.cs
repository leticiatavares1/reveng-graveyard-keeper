using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AgentAIEntityTypeMaskProvider
{
	[SerializeField]
	private List<AgentAIEntityTypeMaskEntry> entries = new List<AgentAIEntityTypeMaskEntry>();

	public bool TryGetAgentAI(LazyConsts.Fighting.EntityType entityType, out AgentAI ai)
	{
		for (int i = 0; i < entries.Count; i++)
		{
			AgentAIEntityTypeMaskEntry agentAIEntityTypeMaskEntry = entries[i];
			if (agentAIEntityTypeMaskEntry.entityTypeMask != 0 && (bool)agentAIEntityTypeMaskEntry.agentAI && (entityType & agentAIEntityTypeMaskEntry.entityTypeMask) == agentAIEntityTypeMaskEntry.entityTypeMask)
			{
				ai = agentAIEntityTypeMaskEntry.agentAI;
				return true;
			}
		}
		ai = null;
		return false;
	}
}
