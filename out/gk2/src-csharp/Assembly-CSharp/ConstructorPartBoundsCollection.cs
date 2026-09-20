using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ConstructorPartBoundsCollection
{
	[Serializable]
	private class Entry
	{
		[SerializeField]
		public string assetPath;

		[SerializeField]
		public ChunkBoundsPair localBounds;
	}

	[SerializeField]
	private List<Entry> entries = new List<Entry>();

	private Dictionary<string, ChunkBoundsPair> lookup;

	public int Count => entries.Count;

	public void SetBounds(string assetPath, ChunkBoundsPair localBounds)
	{
		if (!string.IsNullOrEmpty(assetPath))
		{
			entries.RemoveAll((Entry e) => e.assetPath == assetPath);
			entries.Add(new Entry
			{
				assetPath = assetPath,
				localBounds = localBounds
			});
			lookup = null;
		}
	}

	public bool TryGetBounds(string assetPath, out ChunkBoundsPair localBounds)
	{
		if (lookup == null)
		{
			BuildLookup();
		}
		return lookup.TryGetValue(assetPath, out localBounds);
	}

	public bool TryGetBounds(string assetPath, Vector3 worldPosition, Vector3 lossyScale, out BurstableChunkBoundsPair worldBounds)
	{
		if (lookup == null)
		{
			BuildLookup();
		}
		if (!lookup.TryGetValue(assetPath, out var value))
		{
			worldBounds = default(BurstableChunkBoundsPair);
			return false;
		}
		Vector3 b = new Vector3(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y), Mathf.Abs(lossyScale.z));
		Vector3 vector = Vector3.Scale(value.withShadows.size, b);
		Vector3 vector2 = Vector3.Scale(value.withoutShadows.size, b);
		worldBounds = new BurstableChunkBoundsPair(new BurstableBounds(value.withShadows.center + worldPosition, vector), new BurstableBounds(value.withoutShadows.center + worldPosition, vector2));
		return true;
	}

	public void BuildLookup()
	{
		lookup = new Dictionary<string, ChunkBoundsPair>(entries.Count);
		foreach (Entry entry in entries)
		{
			if (!string.IsNullOrEmpty(entry.assetPath))
			{
				lookup[entry.assetPath] = entry.localBounds;
			}
		}
	}

	public void Clear()
	{
		entries.Clear();
		lookup = null;
	}
}
