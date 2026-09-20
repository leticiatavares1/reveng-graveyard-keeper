using System;
using System.Collections.Generic;
using LinqTools;
using NodeCanvas.Framework;
using Pathfinding;
using UnityEngine;

[Serializable]
public class ComponentsManager
{
	private const int CHAR_LAYER = 9;

	private static readonly Type[] ALL_COMPONENTS = new Type[9]
	{
		typeof(BaseCharacterComponent),
		typeof(HPActionComponent),
		typeof(CombatComponent),
		typeof(KickComponent),
		typeof(AnimatedBehaviour),
		typeof(TimerComponent),
		typeof(InteractionComponent),
		typeof(CraftComponent),
		typeof(ToolComponent)
	};

	private GameObject _obj;

	private WorldGameObject _wgo;

	private Dictionary<Type, WorldGameObjectComponent> _components = new Dictionary<Type, WorldGameObjectComponent>();

	private WorldGameObjectComponent[] _components_fast_list = new WorldGameObjectComponent[0];

	private bool _interaction_buttons_shown;

	[NonSerialized]
	private CustomNetworkAnimatorSync _animator;

	private bool _animator_initialized;

	public WorldGameObject wgo => _wgo;

	public InteractionComponent interaction => GetComponent<InteractionComponent>();

	public ToolComponent tool => GetComponent<ToolComponent>();

	public AnimatedBehaviour animated_behaviour => GetComponent<AnimatedBehaviour>();

	public CombatComponent combat => GetComponent<CombatComponent>();

	public HPActionComponent hp => GetComponent<HPActionComponent>();

	public BaseCharacterComponent character => GetComponent<BaseCharacterComponent>();

	public KickComponent kick => GetComponent<KickComponent>();

	public CraftComponent craft => GetComponent<CraftComponent>();

	public DropCollectorComponent drop_colldector => GetComponent<DropCollectorComponent>();

	public TimerComponent timer => GetComponent<TimerComponent>();

	public AuraReceiver aura_receiver => new AuraReceiver
	{
		enabled = false
	};

	public AuraEmitter aura_emitter => new AuraEmitter
	{
		enabled = false
	};

	public CustomNetworkAnimatorSync animator
	{
		get
		{
			if (!_animator_initialized)
			{
				_animator = CustomNetworkAnimatorSync.InitAnimator(wgo.gameObject);
				_animator_initialized = _animator != null;
			}
			return _animator;
		}
	}

	public ComponentsManager(WorldGameObject wgo)
	{
		_wgo = wgo;
		_obj = wgo.gameObject;
		_components.Clear();
		Type[] aLL_COMPONENTS = ALL_COMPONENTS;
		foreach (Type type in aLL_COMPONENTS)
		{
			WorldGameObjectComponent worldGameObjectComponent = Activator.CreateInstance(type) as WorldGameObjectComponent;
			worldGameObjectComponent.Init(_wgo);
			_components.Add(type, worldGameObjectComponent);
		}
		_components_fast_list = _components.Values.ToArray();
	}

	private void RefreshComponentsEnableState()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (_wgo.obj_def == null)
		{
			Debug.LogError("Definition is null on WGO: " + _wgo.name, _wgo);
			return;
		}
		ObjectDefinition.ObjType type = _wgo.obj_def.type;
		WorldGameObjectComponent[] components_fast_list = _components_fast_list;
		for (int i = 0; i < components_fast_list.Length; i++)
		{
			components_fast_list[i].UpdateEnableState(type);
		}
	}

	public T GetComponent<T>() where T : WorldGameObjectComponent
	{
		try
		{
			return _components[typeof(T)] as T;
		}
		catch (KeyNotFoundException)
		{
			Debug.LogError("Component of requested type not found: " + typeof(T).Name, _wgo);
			return null;
		}
		catch (NullReferenceException exception)
		{
			Debug.LogException(exception, _wgo);
			if (_components == null)
			{
				Debug.LogError("_components is null");
			}
			Debug.Log("typeof(T) = " + typeof(T));
			return null;
		}
	}

	public void PreStartComponents()
	{
		if (_wgo.obj_def == null)
		{
			Debug.LogError("Object definition is null for obj_id = \"" + _wgo.obj_id + "\"", _wgo);
			return;
		}
		if (!string.IsNullOrEmpty(_wgo.custom_tag) || craft.is_crafting || !string.IsNullOrEmpty(_wgo.obj_def.attached_script))
		{
			StartComponents();
		}
		if (_wgo.obj_def.IsPorterStation())
		{
			_ = _wgo.porter_station;
		}
	}

	public void StartComponents()
	{
		if (_wgo.obj_def == null)
		{
			Debug.LogError("Object definition is null for obj_id = \"" + _wgo.obj_id + "\"", _wgo);
			return;
		}
		ObjectDefinition.ObjType type = _wgo.obj_def.type;
		WorldGameObjectComponent[] components_fast_list = _components_fast_list;
		foreach (WorldGameObjectComponent worldGameObjectComponent in components_fast_list)
		{
			worldGameObjectComponent.UpdateEnableState(type);
			if (worldGameObjectComponent.enabled)
			{
				worldGameObjectComponent.StartComponent();
			}
		}
	}

	public void Update(float delta_time)
	{
		if (MainGame.paused)
		{
			return;
		}
		for (int i = 0; i < _components_fast_list.Length; i++)
		{
			WorldGameObjectComponent worldGameObjectComponent = _components_fast_list[i];
			if (worldGameObjectComponent.enabled && worldGameObjectComponent.HasUpdate())
			{
				worldGameObjectComponent.UpdateComponent(delta_time);
			}
		}
	}

	public void LateUpdate()
	{
		for (int i = 0; i < _components_fast_list.Length; i++)
		{
			WorldGameObjectComponent worldGameObjectComponent = _components_fast_list[i];
			if (worldGameObjectComponent.enabled && worldGameObjectComponent.HasLateUpdate())
			{
				worldGameObjectComponent.LateUpdateComponent();
			}
		}
	}

	public void FixedUpdate()
	{
		for (int i = 0; i < _components_fast_list.Length; i++)
		{
			WorldGameObjectComponent worldGameObjectComponent = _components_fast_list[i];
			if (worldGameObjectComponent.enabled && worldGameObjectComponent.HasFixedUpdate())
			{
				worldGameObjectComponent.FixedUpdateComponent(Time.fixedDeltaTime);
			}
		}
	}

	public bool DoAction(WorldGameObject other_obj, float delta_time)
	{
		bool result = false;
		for (int i = 0; i < _components_fast_list.Length; i++)
		{
			WorldGameObjectComponent worldGameObjectComponent = _components_fast_list[i];
			if (worldGameObjectComponent.enabled && worldGameObjectComponent.DoAction(other_obj, delta_time))
			{
				result = true;
			}
		}
		return result;
	}

	public void Interact(WorldGameObject other_obj, bool interaction_start, float delta_time = -1f)
	{
		for (int i = 0; i < _components_fast_list.Length; i++)
		{
			WorldGameObjectComponent worldGameObjectComponent = _components_fast_list[i];
			if (worldGameObjectComponent.enabled)
			{
				worldGameObjectComponent.Interact(other_obj, delta_time);
			}
		}
	}

	public void GetAllComponentsAndSort()
	{
	}

	public void InitAllComponents()
	{
		_animator_initialized = false;
		RefreshComponentsEnableState();
		if (_components_fast_list == null)
		{
			if (_components == null)
			{
				Debug.LogError("Broken components manager at WGO " + wgo.name, wgo);
				_components = new Dictionary<Type, WorldGameObjectComponent>();
			}
			_components_fast_list = _components.Values.ToArray();
		}
		for (int i = 0; i < _components_fast_list.Length; i++)
		{
			WorldGameObjectComponent worldGameObjectComponent = _components_fast_list[i];
			if (worldGameObjectComponent.enabled)
			{
				worldGameObjectComponent.InitComponent();
			}
		}
		if (character != null)
		{
			character.Recache();
		}
	}

	public virtual void PrepareForInteraction(BaseCharacterComponent for_whom)
	{
		WorldGameObjectComponent[] components_fast_list = _components_fast_list;
		foreach (WorldGameObjectComponent worldGameObjectComponent in components_fast_list)
		{
			if (worldGameObjectComponent.enabled)
			{
				worldGameObjectComponent.PrepareForInteraction(for_whom);
			}
		}
	}

	public virtual void UnprepareForInteraction()
	{
		for (int i = 0; i < _components_fast_list.Length; i++)
		{
			WorldGameObjectComponent worldGameObjectComponent = _components_fast_list[i];
			if (worldGameObjectComponent.enabled)
			{
				worldGameObjectComponent.UnprepareForInteraction();
			}
		}
	}

	public void UpdateComponentsSet()
	{
		CheckCharacterStuff(_wgo.obj_def.IsCharacter());
		GetAllComponentsAndSort();
		InitAllComponents();
	}

	private void CheckCharacterStuff(bool is_character)
	{
		GameObject gameObject = _wgo.gameObject;
		Rigidbody2D rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
		Seeker component = gameObject.GetComponent<Seeker>();
		Blackboard component2 = gameObject.GetComponent<Blackboard>();
		if (is_character)
		{
			if (rigidbody2D == null)
			{
				rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
			}
			if (!wgo.is_player)
			{
				ObjectDefinition obj_def = wgo.obj_def;
				rigidbody2D.mass = obj_def.mass;
				rigidbody2D.drag = obj_def.drag;
			}
			else
			{
				rigidbody2D.mass = 80f;
				rigidbody2D.drag = 50f;
			}
			rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
			rigidbody2D.sleepMode = RigidbodySleepMode2D.StartAsleep;
			rigidbody2D.interpolation = RigidbodyInterpolation2D.Extrapolate;
			rigidbody2D.freezeRotation = true;
			if (gameObject.layer != 9)
			{
				gameObject.layer = 9;
			}
			return;
		}
		if (rigidbody2D != null)
		{
			Destroy(rigidbody2D);
		}
		if (component != null)
		{
			SimpleSmoothModifierXY component3 = gameObject.GetComponent<SimpleSmoothModifierXY>();
			if (component3 != null)
			{
				Destroy(component3);
			}
			Destroy(component);
		}
		if (component2 != null)
		{
			Destroy(component2);
		}
		if (gameObject.layer == 9)
		{
			Debug.LogWarning("Non character obj (" + _wgo.name + ") has Character layer, changed to Default, wgo.def.type = " + _wgo.obj_def.type.ToString() + ", id = " + _wgo.obj_def.id);
			gameObject.layer = 0;
		}
	}

	private void Destroy(UnityEngine.Object obj)
	{
		if (Application.isPlaying)
		{
			UnityEngine.Object.Destroy(obj);
		}
		else
		{
			UnityEngine.Object.DestroyImmediate(obj);
		}
	}

	public void RefreshBubblesData(bool? show_interaction_buttons)
	{
		wgo.SetBubbleWidgetData("", BubbleWidgetData.WidgetID.Work);
		wgo.SetBubbleWidgetData("", BubbleWidgetData.WidgetID.Interaction);
		if (show_interaction_buttons.HasValue)
		{
			_interaction_buttons_shown = show_interaction_buttons.Value;
		}
		WorldGameObjectComponent[] components_fast_list = _components_fast_list;
		foreach (WorldGameObjectComponent worldGameObjectComponent in components_fast_list)
		{
			if (worldGameObjectComponent.enabled)
			{
				worldGameObjectComponent.RefreshComponentBubbleData(_interaction_buttons_shown);
			}
		}
		ObjectDefinition obj_def = wgo.obj_def;
		string text = "";
		if (_interaction_buttons_shown && !wgo.bubble.DoesContainWidgetDataWithID(BubbleWidgetData.WidgetID.Interaction) && !wgo.bubble.DoesContainWidgetDataWithID(BubbleWidgetData.WidgetID.Work))
		{
			CustomInteractionHint component = wgo.GetComponent<CustomInteractionHint>();
			ObjectInteractionDefinition validInteraction = obj_def.GetValidInteraction(wgo);
			if (component != null && component.has_hint)
			{
				text = component.GetHint();
			}
			else if (validInteraction != null)
			{
				text = validInteraction.hint;
				if (craft.enabled && craft.is_crafting && craft.current_craft.is_auto && !craft.current_craft.hidden)
				{
					text = null;
				}
			}
			else
			{
				text = obj_def.GetInteractionHint(wgo);
				if (string.IsNullOrEmpty(text))
				{
					switch (obj_def.interaction_type)
					{
					case ObjectDefinition.InteractionType.Craft:
						text = "craft";
						break;
					case ObjectDefinition.InteractionType.Chest:
						text = "open";
						break;
					case ObjectDefinition.InteractionType.Builder:
						text = "build";
						break;
					case ObjectDefinition.InteractionType.Grave:
						text = "open grave";
						break;
					}
				}
				if (craft.enabled && craft.is_crafting && craft.current_craft.is_auto)
				{
					text = null;
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				text = GameKeyTip.Get(GameKey.Interaction, text);
			}
			wgo.SetBubbleWidgetData(text, BubbleWidgetData.WidgetID.Interaction);
		}
		else if (!_interaction_buttons_shown)
		{
			wgo.SetBubbleWidgetData("", BubbleWidgetData.WidgetID.Interaction);
		}
		if (wgo.custom_interaction_events.Count > 0 && !wgo.IsMoving())
		{
			string text2 = ((!string.IsNullOrEmpty(wgo.obj_def.custom_interaction_icon)) ? wgo.obj_def.custom_interaction_icon : (wgo.components.character.enabled ? "(speak)" : "(view)"));
			if (_interaction_buttons_shown)
			{
				text2 = GameKeyTip.Get(GameKey.Interaction, text2);
			}
			wgo.SetBubbleWidgetData(text2, BubbleWidgetData.WidgetID.Interaction);
		}
		wgo.SetBubbleWidgetData(wgo.GetQualityWidgetData(), BubbleWidgetData.WidgetID.Quality);
	}
}
