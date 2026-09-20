using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "StartReses", menuName = "GK2/Start Reses")]
public class StartReses : ScriptableObject
{
	[Serializable]
	public class StartItemData
	{
		public string id;

		public int count;

		public bool shouldBeEquipped;
	}

	private const string RESOURCE_FOLDER = "StartReses";

	public List<StartItemData> startItems = new List<StartItemData>();

	public GameRes startGameRes = new GameRes();

	public GameResStr startGameResStr = new GameResStr();

	public static StartReses Load()
	{
		return Addressables.LoadAssetAsync<StartReses>("StartReses/PlayerStartState.asset").WaitForCompletion();
	}
}
