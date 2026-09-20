using System;
using System.Collections.Generic;

[Serializable]
public class ObjectGroupDefinition : BalanceBaseObject
{
	public List<string> aura_emitters = new List<string>();

	public List<string> aura_receivers = new List<string>();

	public string tech_icon = "";

	[NonSerialized]
	public List<ObjectDefinition> objects = new List<ObjectDefinition>();

	public static void LinkObjectsToGroups()
	{
		foreach (ObjectGroupDefinition object_group in GameBalance.me.object_groups)
		{
			object_group.objects = new List<ObjectDefinition>();
			foreach (ObjectDefinition objs_datum in GameBalance.me.objs_data)
			{
				if (objs_datum.DoesBelongToGroup(object_group.id))
				{
					object_group.objects.Add(objs_datum);
				}
			}
		}
	}
}
