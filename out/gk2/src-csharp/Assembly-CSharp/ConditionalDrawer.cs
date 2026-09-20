using System;
using System.Collections.Generic;
using UnityEngine;

public class ConditionalDrawer : MonoBehaviour
{
	[SerializeField]
	[SerializeReference]
	private List<ConditionalDrawerRule> rules = new List<ConditionalDrawerRule>();

	private ConditionalDrawerContext context;

	private Dictionary<ConditionalEventType, List<ConditionalDrawerRule>> rulesByEvent;

	private WgoPart wgoPart;

	private BuildController buildController;

	public List<ConditionalDrawerRule> Rules => rules;

	private PlayerWorkComponent PlayerWorkComp => MainGame.PlayerController?.PlayerWorkComponent;

	private ConveyorComponent ConveyorComponent
	{
		get
		{
			if (!(wgoPart?.Wgo?.Data is ConveyorWgoData conveyorWgoData))
			{
				return null;
			}
			return conveyorWgoData.ConveyorComponent;
		}
	}

	public void Init(WgoPart wgoPart)
	{
		this.wgoPart = wgoPart;
		buildController = BuildController.Instance;
		if (rules.Count != 0)
		{
			context = new ConditionalDrawerContext(wgoPart, buildController);
			BuildRulesIndex();
			SubscribeToEvents();
			EvaluateAll();
		}
	}

	public void DeInit()
	{
		UnsubscribeFromEvents();
		context = null;
	}

	public void UpdateVisualsByConditions()
	{
		EvaluateAll();
	}

	public void AddRule(ConditionalDrawerRule rule)
	{
		rules.Add(rule);
		if (context != null)
		{
			BuildRulesIndex();
		}
	}

	public bool RemoveRule(ConditionalDrawerRule rule)
	{
		bool num = rules.Remove(rule);
		if (num && context != null)
		{
			BuildRulesIndex();
		}
		return num;
	}

	public void SubscribeToParentCraftStatusChanged(WgoData wgoData)
	{
		if (!rulesByEvent.ContainsKey(ConditionalEventType.CraftStatusChanged))
		{
			return;
		}
		foreach (SGuid workbenchParent in wgoData.WorkbenchParents)
		{
			WgoData wgoData2 = MainGame.Instance.GameSave.WorldData.GetWgoData(workbenchParent);
			if (wgoData2 != null)
			{
				wgoData2.CraftComponent.OnStatusChanged += OnParentCraftStatusChanged;
			}
		}
	}

	private void BuildRulesIndex()
	{
		rulesByEvent = new Dictionary<ConditionalEventType, List<ConditionalDrawerRule>>();
		foreach (ConditionalEventType value in Enum.GetValues(typeof(ConditionalEventType)))
		{
			if (value == ConditionalEventType.None || value == ConditionalEventType.All)
			{
				continue;
			}
			List<ConditionalDrawerRule> list = new List<ConditionalDrawerRule>();
			foreach (ConditionalDrawerRule rule in rules)
			{
				if (rule.condition != null && rule.EventType.HasFlag(value))
				{
					list.Add(rule);
				}
			}
			if (list.Count > 0)
			{
				rulesByEvent[value] = list;
			}
		}
	}

	private void EvaluateAll()
	{
		if (context == null)
		{
			return;
		}
		context.UpdateCachedValues();
		foreach (ConditionalDrawerRule rule in rules)
		{
			rule.Evaluate(context);
		}
	}

	private void EvaluateByEvent(ConditionalEventType eventType)
	{
		if (context == null)
		{
			return;
		}
		context.UpdateCachedValues(fromCalledEvent: true);
		if (!rulesByEvent.TryGetValue(eventType, out var value))
		{
			return;
		}
		foreach (ConditionalDrawerRule item in value)
		{
			item.Evaluate(context);
		}
	}

	private void SubscribeToEvents()
	{
		if (wgoPart?.Wgo?.Data == null)
		{
			return;
		}
		WgoData data = wgoPart.Wgo.Data;
		Inventory inventory = data.Inventory;
		if (rulesByEvent.ContainsKey(ConditionalEventType.GameResChanged))
		{
			data.OnGameResChanged += OnGameResChanged;
		}
		if (rulesByEvent.ContainsKey(ConditionalEventType.ItemsChanged))
		{
			inventory.OnItemsAdd += OnItemsChanged;
			inventory.OnItemsRemove += OnItemsChanged;
		}
		if (rulesByEvent.ContainsKey(ConditionalEventType.CraftProgressChanged))
		{
			data.CraftComponent.OnCraftCurProgressNormalizedChanged += OnCraftProgressChanged;
		}
		if (rulesByEvent.ContainsKey(ConditionalEventType.CraftStatusChanged))
		{
			data.CraftComponent.OnStatusChanged += OnCraftStatusChanged;
		}
		if (rulesByEvent.ContainsKey(ConditionalEventType.WorkStateChanged))
		{
			PlayerWorkComponent playerWorkComp = PlayerWorkComp;
			playerWorkComp.OnInteractionUpdate = (Action)Delegate.Combine(playerWorkComp.OnInteractionUpdate, new Action(OnWorkStateChanged));
			data.OnWorkerChanged += OnWorkStateChanged;
			data.OnToolTickApply += OnWorkStateChanged;
			data.CraftComponent.OnStatusChanged += OnWorkStateChanged;
		}
		if (rulesByEvent.ContainsKey(ConditionalEventType.ConveyorChanged) && ConveyorComponent != null)
		{
			ConveyorComponent.OnConnected += OnConveyorChanged;
			ConveyorComponent.OnDisconnected += OnConveyorChanged;
		}
		if (rulesByEvent.ContainsKey(ConditionalEventType.ConveyorSystemChanged))
		{
			MainGame.Instance.conveyorSystem.ConveyorWorldZone.OnWgoDataAdded += OnConveyorSystemChanged;
			MainGame.Instance.conveyorSystem.ConveyorWorldZone.OnWgoDataRemoved += OnConveyorSystemChanged;
		}
		if (rulesByEvent.ContainsKey(ConditionalEventType.FightingAgentChanged) && wgoPart.TryGetComponent<FightingAgent>(out var component))
		{
			component.OnInitialized += OnFightingAgentInitialized;
		}
		if (rulesByEvent.ContainsKey(ConditionalEventType.BuildingModeChanged) && buildController != null)
		{
			buildController.OnBuildModeStateChanged += OnBuildModeStateChanged;
		}
		if (rulesByEvent.ContainsKey(ConditionalEventType.InteractableStateChanged) && data != null)
		{
			data.OnInteractableStateChanged += OnInteractableStateChanged;
		}
		if (rulesByEvent.ContainsKey(ConditionalEventType.ParentWorkCondition) && data != null)
		{
			foreach (SGuid workbenchParent in data.WorkbenchParents)
			{
				WgoData wgoData = MainGame.Instance.GameSave.WorldData.GetWgoData(workbenchParent);
				if (wgoData != null)
				{
					wgoData.CraftComponent.OnStatusChanged += OnParentWorkStatusChanged;
					wgoData.OnToolTickApply += OnParentWorkStatusChanged;
				}
			}
		}
		if (data is ZombieWgoData zombieWgoData)
		{
			zombieWgoData.OnCaretakerStateChanged += OnZombieCaretakerStateChanged;
		}
		foreach (DockPointData dockPointData in data.MainWgoPartData.DockPointDataList)
		{
			dockPointData.OnOccupiedStatusChanged += OnOccupiedDockPointStatusChanged;
		}
		data.OnTakenDockPointChanged += OnTakenDockPointChanged;
		if (data.HpComponent != null)
		{
			data.HpComponent.OnHpChanged += OnHpChanged;
		}
	}

	private void UnsubscribeFromEvents()
	{
		if (wgoPart?.Wgo?.Data == null)
		{
			return;
		}
		WgoData data = wgoPart.Wgo.Data;
		Inventory inventory = data.Inventory;
		data.OnGameResChanged -= OnGameResChanged;
		inventory.OnItemsAdd -= OnItemsChanged;
		inventory.OnItemsRemove -= OnItemsChanged;
		data.CraftComponent.OnCraftCurProgressNormalizedChanged -= OnCraftProgressChanged;
		data.CraftComponent.OnStatusChanged -= OnCraftStatusChanged;
		PlayerWorkComponent playerWorkComp = PlayerWorkComp;
		playerWorkComp.OnInteractionUpdate = (Action)Delegate.Remove(playerWorkComp.OnInteractionUpdate, new Action(OnWorkStateChanged));
		data.OnWorkerChanged -= OnWorkStateChanged;
		data.OnToolTickApply -= OnWorkStateChanged;
		data.CraftComponent.OnStatusChanged -= OnWorkStateChanged;
		foreach (SGuid workbenchParent in data.WorkbenchParents)
		{
			WgoData wgoData = MainGame.Instance.GameSave.WorldData.GetWgoData(workbenchParent);
			if (wgoData != null)
			{
				wgoData.CraftComponent.OnStatusChanged -= OnParentCraftStatusChanged;
			}
		}
		foreach (SGuid workbenchParent2 in data.WorkbenchParents)
		{
			WgoData wgoData2 = MainGame.Instance.GameSave.WorldData.GetWgoData(workbenchParent2);
			if (wgoData2 != null)
			{
				data.CraftComponent.OnStatusChanged -= OnParentWorkStatusChanged;
				wgoData2.OnToolTickApply -= OnParentWorkStatusChanged;
			}
		}
		if (ConveyorComponent != null)
		{
			ConveyorComponent.OnConnected -= OnConveyorChanged;
			ConveyorComponent.OnDisconnected -= OnConveyorChanged;
		}
		MainGame.Instance.conveyorSystem.ConveyorWorldZone.OnWgoDataAdded -= OnConveyorSystemChanged;
		MainGame.Instance.conveyorSystem.ConveyorWorldZone.OnWgoDataRemoved -= OnConveyorSystemChanged;
		if (wgoPart.TryGetComponent<FightingAgent>(out var component))
		{
			component.OnInitialized -= OnFightingAgentInitialized;
		}
		if (buildController != null)
		{
			buildController.OnBuildModeStateChanged -= OnBuildModeStateChanged;
		}
		if (data != null)
		{
			data.OnInteractableStateChanged -= OnInteractableStateChanged;
		}
		if (data is ZombieWgoData zombieWgoData)
		{
			zombieWgoData.OnCaretakerStateChanged -= OnZombieCaretakerStateChanged;
		}
		foreach (DockPointData dockPointData in data.MainWgoPartData.DockPointDataList)
		{
			dockPointData.OnOccupiedStatusChanged -= OnOccupiedDockPointStatusChanged;
		}
		data.OnTakenDockPointChanged -= OnTakenDockPointChanged;
		if (data.HpComponent != null)
		{
			data.HpComponent.OnHpChanged -= OnHpChanged;
		}
	}

	private void OnGameResChanged(string id)
	{
		EvaluateByEvent(ConditionalEventType.GameResChanged);
	}

	private void OnItemsChanged(List<Item> items)
	{
		EvaluateByEvent(ConditionalEventType.ItemsChanged);
	}

	private void OnCraftProgressChanged(float progress)
	{
		EvaluateByEvent(ConditionalEventType.CraftProgressChanged);
	}

	private void OnCraftStatusChanged(CraftComponentStatus status)
	{
		EvaluateByEvent(ConditionalEventType.CraftStatusChanged);
	}

	private void OnParentCraftStatusChanged(CraftComponentStatus status)
	{
		EvaluateByEvent(ConditionalEventType.CraftStatusChanged);
	}

	private void OnParentWorkStatusChanged(CraftComponentStatus status)
	{
		EvaluateByEvent(ConditionalEventType.ParentWorkCondition);
	}

	private void OnParentWorkStatusChanged(bool isFirstHit)
	{
		EvaluateByEvent(ConditionalEventType.ParentWorkCondition);
	}

	private void OnWorkStateChanged()
	{
		EvaluateByEvent(ConditionalEventType.WorkStateChanged);
	}

	private void OnWorkStateChanged(bool isFirstHit)
	{
		EvaluateByEvent(ConditionalEventType.WorkStateChanged);
	}

	private void OnWorkStateChanged(CraftComponentStatus status)
	{
		EvaluateByEvent(ConditionalEventType.WorkStateChanged);
	}

	private void OnConveyorChanged()
	{
		EvaluateByEvent(ConditionalEventType.ConveyorChanged);
	}

	private void OnFightingAgentInitialized(bool isValid)
	{
		EvaluateByEvent(ConditionalEventType.FightingAgentChanged);
	}

	private void OnBuildModeStateChanged(bool isActive)
	{
		EvaluateByEvent(ConditionalEventType.BuildingModeChanged);
	}

	private void OnZombieCaretakerStateChanged()
	{
		EvaluateByEvent(ConditionalEventType.CaretakerStateChanged);
		EvaluateByEvent(ConditionalEventType.ZombieWorkerStateChanged);
	}

	private void OnOccupiedDockPointStatusChanged()
	{
		EvaluateByEvent(ConditionalEventType.DockPointStatusChanged);
	}

	private void OnTakenDockPointChanged()
	{
		EvaluateByEvent(ConditionalEventType.TakenDockPointChanged);
	}

	private void OnHpChanged(HPComponent hpComponent)
	{
		EvaluateByEvent(ConditionalEventType.HPChanged);
	}

	private void OnInteractableStateChanged(bool state)
	{
		EvaluateByEvent(ConditionalEventType.InteractableStateChanged);
	}

	private void OnConveyorSystemChanged(WgoData wgoData)
	{
		EvaluateByEvent(ConditionalEventType.ConveyorSystemChanged);
	}
}
