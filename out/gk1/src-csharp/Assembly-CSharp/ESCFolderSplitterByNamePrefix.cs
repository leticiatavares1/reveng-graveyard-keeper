using System;
using System.Collections.Generic;
using UnityEngine;

public class ESCFolderSplitterByNamePrefix : ESCFolderSplitter
{
	[Serializable]
	public class ESCGroupDescription
	{
		[SerializeField]
		public List<string> items = new List<string>();
	}

	[SerializeField]
	public List<ESCGroupDescription> groups = new List<ESCGroupDescription>();

	public override int GetCollectionID(string filename)
	{
		int num = 0;
		foreach (ESCGroupDescription group in groups)
		{
			foreach (string item in group.items)
			{
				if (filename.IndexOf(item, StringComparison.Ordinal) == 0)
				{
					return num;
				}
			}
			num++;
		}
		return -1;
	}
}
