using System.Collections.Generic;
using UnityEngine;

public class CustomUpdateManager : MonoBehaviour
{
	public static List<WorldGameObject> wgos = new List<WorldGameObject>();

	public static List<ICustomUpdateMonoBehaviour> updates = new List<ICustomUpdateMonoBehaviour>();

	public static CustomUpdateManager me = null;

	public static void Init()
	{
		me = SingletonGameObjects.FindOrCreate<CustomUpdateManager>();
	}

	public void Update()
	{
		if (!MainGame.game_started || MainGame.paused)
		{
			if (GUIElements.me.game_gui.is_shown)
			{
				if (LazyInput.GetKeyDown(GameKey.Inventory))
				{
					GUIElements.me.game_gui.OpenOrSelectTab(GameGUI.TabType.Inventory);
				}
				if (LazyInput.GetKeyDown(GameKey.KnownNPCs))
				{
					GUIElements.me.game_gui.OpenOrSelectTab(GameGUI.TabType.NPCs);
				}
				if (LazyInput.GetKeyDown(GameKey.Techs))
				{
					GUIElements.me.game_gui.OpenOrSelectTab(GameGUI.TabType.Techs);
				}
				if (LazyInput.GetKeyDown(GameKey.Map))
				{
					GUIElements.me.game_gui.OpenOrSelectTab(GameGUI.TabType.Map);
				}
			}
		}
		else
		{
			for (int i = 0; i < wgos.Count; i++)
			{
				wgos[i].CustomUpdate();
			}
			for (int j = 0; j < updates.Count; j++)
			{
				updates[j].CustomUpdate();
			}
			CraftComponent.UpdateAllCrafts(Time.deltaTime);
			ItemsDurabilityManager.EveryFrameUpdate(Time.deltaTime);
			BuffsLogics.UpdateEveryFrame();
		}
	}

	public void FixedUpdate()
	{
		if (MainGame.game_started && !MainGame.paused)
		{
			for (int i = 0; i < wgos.Count; i++)
			{
				wgos[i].CustomFixedUpdate();
			}
		}
	}

	public void LateUpdate()
	{
		if (MainGame.game_started)
		{
			for (int i = 0; i < wgos.Count; i++)
			{
				wgos[i].CustomLateUpdate();
			}
		}
	}
}
