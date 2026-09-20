using System;
using UnityEngine;

public class AnimationComponent : AnimationComponentBase
{
	public enum Layers
	{
		Default = 0,
		Lighting = 1,
		Breath = 2,
		Overhead = 3,
		Backpack = 4,
		OverheadInteracting = 5,
		WeaponHitBox = 6,
		Armor = 7,
		ArmorWithSword = 8,
		ArmorWithPike = 9,
		ArmorWithBow = 10,
		SwordAttack = 11,
		BowAttack = 12,
		StanceWalk = 13,
		Talking = 14,
		SwordAttackHitbox = 15,
		Eyes = 17,
		ArmorNoHelmet = 18
	}

	[SerializeField]
	private bool isNeedSkinChanger = true;

	[SerializeField]
	[Space]
	private bool setCustomLayersOnInit;

	[SerializeField]
	private bool enableLightLayer;

	[SerializeField]
	private bool enableBreathLayer;

	[SerializeField]
	private bool enableOverheadLayer;

	[SerializeField]
	private bool enableBackpackLayer;

	[SerializeField]
	private bool enableArmorLayer;

	[SerializeField]
	private bool enableArmorWithSwordLayer;

	[SerializeField]
	private bool enableArmorWithPikeLayer;

	[SerializeField]
	private bool enableArmorWithBowLayer;

	[SerializeField]
	private bool enableArmorNoHelmetLayer;

	[SerializeField]
	[Space]
	private bool autoInitOnAwake;

	[SerializeField]
	private Direction direction = Direction.Down;

	[SerializeField]
	protected SkinPresetGK2 skinPresetDefault;

	[SerializeField]
	[HideInInspector]
	protected ArmorPresetData armorPresetData;

	[SerializeField]
	[HideInInspector]
	protected int armorColorIndex;

	[SerializeField]
	private bool useTalkingAnimation;

	[SerializeField]
	private TalkingHeadPreset talkingHeadPreset;

	protected SkinChangerGK2 skinChanger;

	private TalkingHeadPlayer talkingHeadPlayer;

	public bool UseTalkingAnimation => useTalkingAnimation;

	public bool HasTalkingHeadFrames
	{
		get
		{
			if (!useTalkingAnimation && talkingHeadPreset != null && skinChanger != null)
			{
				return skinChanger.HasCustomHeadFrames;
			}
			return false;
		}
	}

	public bool AutoInitOnAwake => autoInitOnAwake;

	public Direction Direction => direction;

	public event Action OnLateUpdate;

	public event Action OnDeathAnimFinished;

	public override void Init(string visualId)
	{
		if (isInitialized)
		{
			return;
		}
		if (isNeedSkinChanger)
		{
			if ((bool)skinPresetDefault)
			{
				base.Init(skinPresetDefault);
			}
			else
			{
				base.Init(visualId);
			}
		}
		if (armorPresetData != null)
		{
			ApplyArmorColor(armorColorIndex);
		}
		ApplyCustomLayers();
		isInitialized = true;
	}

	public override void Init(SkinPresetGK2 skinPreset)
	{
		if (!isInitialized)
		{
			base.Init(skinPreset);
			if (armorPresetData != null)
			{
				ApplyArmorColor(armorColorIndex);
			}
			ApplyCustomLayers();
			isInitialized = true;
		}
	}

	public override void InitWithSkinOrApplyCurrentSkin(string skinPreset)
	{
		if (!isInitialized)
		{
			Init(skinPreset);
		}
		else if (skinChanger != null && base.skinPreset != null)
		{
			skinChanger.ApplySkin(base.skinPreset);
		}
	}

	public override void InitWithSkinOrApplySkin(string skinPreset)
	{
		if (!isInitialized)
		{
			Init(skinPreset);
			return;
		}
		if (isNeedSkinChanger)
		{
			if (skinPresetDefault != null)
			{
				base.skinPreset = skinPresetDefault;
			}
			else if (!string.IsNullOrEmpty(skinPreset))
			{
				base.skinPreset = SkinPresetGK2.LoadAsset(skinPreset);
			}
		}
		if (skinChanger != null && base.skinPreset != null)
		{
			skinChanger.ApplySkin(base.skinPreset);
		}
	}

	public void ChangeSkinPreset(string presetId)
	{
		if (!isInitialized)
		{
			Init(presetId);
		}
		else if (isNeedSkinChanger)
		{
			skinPreset = SkinPresetGK2.LoadAsset(presetId);
			skinChanger.ApplySkin(skinPreset);
		}
	}

	public virtual void ChangeSkinPreset(SkinPresetGK2 preset)
	{
		if (!isInitialized && skinChanger == null)
		{
			base.Init(preset);
			skinChanger = new SkinChangerGK2(base.gameObject);
			isInitialized = true;
		}
		skinChanger.ApplySkin(preset);
		CreateTalkingHeadPlayer();
	}

	public void SetLayerWeight(Layers layer, float weight)
	{
		SetLayerWeight((int)layer, weight);
	}

	public float GetLayerWeight(Layers layer)
	{
		return GetLayerWeight((int)layer);
	}

	public virtual void LateUpdate()
	{
		if (talkingHeadPlayer != null)
		{
			talkingHeadPlayer.Tick(Time.deltaTime);
		}
		if (skinChanger != null)
		{
			skinChanger.CustomLateUpdate();
		}
		this.OnLateUpdate?.Invoke();
	}

	protected override void InitInternal(SkinPresetGK2 skinPreset)
	{
		skinChanger = new SkinChangerGK2(base.gameObject);
		skinChanger.ApplySkin(skinPreset);
		CreateTalkingHeadPlayer();
		base.InitInternal(skinPreset);
	}

	public void StartTalkingHead()
	{
		if (!HasTalkingHeadFrames)
		{
			return;
		}
		if (talkingHeadPlayer == null)
		{
			CreateTalkingHeadPlayer();
		}
		if (talkingHeadPlayer != null)
		{
			if (!talkingHeadPlayer.IsPlaying)
			{
				talkingHeadPlayer.PlaySeries();
			}
			else
			{
				talkingHeadPlayer.Resume();
			}
		}
	}

	public void PauseTalkingHead()
	{
		talkingHeadPlayer?.Pause();
	}

	public void StopTalkingHead()
	{
		talkingHeadPlayer?.Stop();
	}

	public void PlayTalkingHeadClip(int clipIndex)
	{
		if (!(talkingHeadPreset == null) && talkingHeadPreset.clips != null && clipIndex >= 0 && clipIndex < talkingHeadPreset.clips.Count)
		{
			if (talkingHeadPlayer == null)
			{
				CreateTalkingHeadPlayer();
			}
			talkingHeadPlayer?.Play(talkingHeadPreset.clips[clipIndex]);
		}
	}

	private void CreateTalkingHeadPlayer()
	{
		talkingHeadPlayer = null;
		if (!(talkingHeadPreset == null) && skinChanger != null)
		{
			talkingHeadPlayer = new TalkingHeadPlayer(this, skinChanger, talkingHeadPreset);
		}
	}

	private void Awake()
	{
		if (isNeedSkinChanger && autoInitOnAwake && skinPresetDefault != null)
		{
			Init(skinPresetDefault);
			if (direction.IsValid())
			{
				SetDirection(direction);
			}
		}
	}

	private void OnEnable()
	{
		if (isInitialized)
		{
			ApplyCustomLayers();
		}
	}

	private void OnDestroy()
	{
		SkinPresetGK2.ReleaseAsset(skinPreset);
		this.OnDeathAnimFinished = null;
	}

	public void ResetArmorLayers()
	{
		SetLayerWeight(Layers.Armor, 0f);
		SetLayerWeight(Layers.ArmorWithSword, 0f);
		SetLayerWeight(Layers.ArmorWithPike, 0f);
		SetLayerWeight(Layers.ArmorWithBow, 0f);
		SetLayerWeight(Layers.ArmorNoHelmet, 0f);
		animator.Update(0f);
	}

	public override void OnDeathAnimationFinished()
	{
		this.OnDeathAnimFinished?.Invoke();
	}

	public override void ResetForPool()
	{
		base.ResetForPool();
		this.OnLateUpdate = null;
		this.OnDeathAnimFinished = null;
		StopTalkingHead();
	}

	protected override void OnDisable()
	{
		StopTalkingHead();
		base.OnDisable();
	}

	public void ApplyArmorColor(int index)
	{
		Texture2D palette = armorPresetData.GetColorReplacePalette(index).palette;
		for (int i = 0; i < armorPresetData.affectedPartTypes.Count; i++)
		{
			switch (armorPresetData.affectedPartTypes[i])
			{
			case CustomizablePartType.Body:
				skinPresetDefault.body.palette = palette;
				break;
			case CustomizablePartType.Arms:
				skinPresetDefault.arms.palette = palette;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		skinChanger?.ApplyShaderParameters();
	}

	protected void ApplyCustomLayers()
	{
		if (setCustomLayersOnInit)
		{
			if (enableLightLayer)
			{
				SetLayerWeight(Layers.Lighting, 1f);
			}
			if (enableBreathLayer)
			{
				SetLayerWeight(Layers.Breath, 1f);
			}
			if (enableOverheadLayer)
			{
				SetLayerWeight(Layers.Overhead, 1f);
			}
			if (enableBackpackLayer)
			{
				SetLayerWeight(Layers.Backpack, 1f);
			}
			if (enableArmorLayer)
			{
				SetLayerWeight(Layers.Armor, 1f);
			}
			if (enableArmorWithSwordLayer)
			{
				SetLayerWeight(Layers.ArmorWithSword, 1f);
			}
			if (enableArmorWithPikeLayer)
			{
				SetLayerWeight(Layers.ArmorWithPike, 1f);
			}
			if (enableArmorWithBowLayer)
			{
				SetLayerWeight(Layers.ArmorWithBow, 1f);
			}
			if (enableArmorNoHelmetLayer)
			{
				SetLayerWeight(Layers.ArmorNoHelmet, 1f);
			}
		}
	}
}
