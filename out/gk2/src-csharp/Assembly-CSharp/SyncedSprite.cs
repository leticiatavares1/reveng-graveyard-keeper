using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SyncedSprite : CachedSpriteRenderer
{
	private static readonly HashSet<SyncedSprite> activeSyncedSprites = new HashSet<SyncedSprite>(1000);

	private static readonly List<SyncedSprite> syncedSpritesToAdd = new List<SyncedSprite>(10);

	private static readonly List<SyncedSprite> syncedSpritesToRemove = new List<SyncedSprite>(10);

	[SerializeField]
	private bool syncStateAndPos;

	[SerializeField]
	private bool syncSprite;

	public CachedSpriteRenderer targetSprite;

	public bool ignoreMe;

	public bool SyncStateAndPos => syncStateAndPos;

	public bool SyncSprite => syncSprite;

	public static void UpdateAllSyncedSprites()
	{
		foreach (SyncedSprite item in syncedSpritesToAdd)
		{
			activeSyncedSprites.Add(item);
		}
		foreach (SyncedSprite item2 in syncedSpritesToRemove)
		{
			activeSyncedSprites.Remove(item2);
		}
		if (activeSyncedSprites.Count == 0)
		{
			return;
		}
		foreach (SyncedSprite activeSyncedSprite in activeSyncedSprites)
		{
			if (activeSyncedSprite == null || activeSyncedSprite.targetSprite == null || activeSyncedSprite.SpriteRenderer == null)
			{
				continue;
			}
			bool flag = activeSyncedSprite.targetSprite.gameObject.activeInHierarchy && activeSyncedSprite.targetSprite.SpriteRenderer.enabled && activeSyncedSprite.targetSprite.SpriteRenderer.sprite != null;
			if (!flag)
			{
				activeSyncedSprite.SpriteRenderer.enabled = false;
				continue;
			}
			if (activeSyncedSprite.syncStateAndPos)
			{
				activeSyncedSprite.SpriteRenderer.transform.position = activeSyncedSprite.targetSprite.SpriteRenderer.transform.position;
				activeSyncedSprite.SpriteRenderer.enabled = flag;
				activeSyncedSprite.SpriteRenderer.flipX = activeSyncedSprite.targetSprite.SpriteRenderer.flipX;
			}
			if (activeSyncedSprite.syncSprite)
			{
				activeSyncedSprite.SpriteRenderer.sprite = activeSyncedSprite.targetSprite.SpriteRenderer.sprite;
			}
		}
		syncedSpritesToAdd.Clear();
		syncedSpritesToRemove.Clear();
	}

	private void OnEnable()
	{
		if (targetSprite != null && !ignoreMe)
		{
			RegisterSyncedSprite(this);
		}
	}

	private void OnBecameVisible()
	{
		if (!ignoreMe && !(targetSprite == null))
		{
			RegisterSyncedSprite(this);
		}
	}

	private void OnBecameInvisible()
	{
		if (!ignoreMe && !(targetSprite == null))
		{
			UnregisterSyncedSprite(this);
		}
	}

	private static void RegisterSyncedSprite(SyncedSprite syncedSprite)
	{
		if (!(syncedSprite == null))
		{
			syncedSpritesToAdd.Add(syncedSprite);
		}
	}

	private static void UnregisterSyncedSprite(SyncedSprite syncedSprite)
	{
		if (!(syncedSprite == null))
		{
			syncedSpritesToRemove.Add(syncedSprite);
		}
	}
}
