using System.Collections.Generic;
using DG.Tweening;
using LazyBearTechnology;
using UnityEngine;

public class DamageEffectComponent : MonoBehaviour
{
	private static Dictionary<SGuid, DamageEffectComponent> cache = new Dictionary<SGuid, DamageEffectComponent>();

	private static readonly int TintColor = Shader.PropertyToID("_TintColor");

	private static readonly int AdditiveTintColor = Shader.PropertyToID("_AdditiveTintColor");

	[SerializeField]
	private DamageEffectSettings settings;

	[SerializeField]
	private List<GenericSprite> sprites = new List<GenericSprite>();

	[SerializeField]
	private bool initManually;

	private SGuid attachedSguid;

	private Tween damageTween;

	private MaterialPropertyBlock matPropertyBlock;

	public DamageEffectSettings Settings => settings;

	private float EvaluationTime { get; set; }

	public static void TryPlayEffect(SGuid guid, Vector3 position, Vector3 attackDirection, DamageEffectSettings effectOverride = null)
	{
		if (!cache.TryGetValue(guid, out var value))
		{
			return;
		}
		DamageEffectSettings damageEffectSettings = effectOverride ?? value.settings;
		if ((bool)damageEffectSettings)
		{
			if (!string.IsNullOrEmpty(damageEffectSettings.fxName))
			{
				WorldFX.Spawn(position, damageEffectSettings.fxName);
			}
			if (damageEffectSettings.spawnBloodPaddle && Random.value < LazySingletonSO<GlobalResources>.Instance.fighting.bloodDecalSpawnProbability)
			{
				float y = Random.Range(-15f, 15f);
				Quaternion quaternion = Quaternion.Euler(0f, y, 0f);
				Vector3 position2 = position + (quaternion * attackDirection).normalized * Random.Range(0.8f, 1.2f);
				LazySingleton<FightingGameController>.Instance.FightEffectsManager.bloodDecalsCollection.SpawnDecal(position2, Direction.None);
			}
			value.PlayColorBlink(damageEffectSettings);
		}
	}

	public static void TryPlayEffect(ICombatEntity target, AttackContext context)
	{
		TryPlayEffect(target.CombatEntityUID, context.hitPosition, context.direction, context.damageEffectOverride);
	}

	public void Init(ICombatEntity entity = null)
	{
		ICombatEntity combatEntity = entity ?? GetComponentInParent<ICombatEntity>();
		if (combatEntity == null)
		{
			Debug.LogError("DamageEffectComponent: combatEntity is null");
			return;
		}
		cache[combatEntity.CombatEntityUID] = this;
		attachedSguid = combatEntity.CombatEntityUID;
	}

	private void Start()
	{
		if (!initManually)
		{
			Init();
		}
	}

	private void OnDestroy()
	{
		if (!SGuid.IsNullOrEmpty(attachedSguid))
		{
			if (cache.TryGetValue(attachedSguid, out var value) && value == this)
			{
				cache.Remove(attachedSguid);
			}
			attachedSguid = null;
			if (damageTween != null)
			{
				damageTween.Kill(complete: true);
				damageTween = null;
			}
		}
	}

	private void PlayColorBlink(DamageEffectSettings blinkSettings)
	{
		if (!blinkSettings)
		{
			return;
		}
		int colorProperty = (blinkSettings.useAdditiveTintColor ? AdditiveTintColor : TintColor);
		if (damageTween == null)
		{
			EvaluationTime = 0f;
			if (matPropertyBlock == null)
			{
				matPropertyBlock = new MaterialPropertyBlock();
			}
			damageTween = DOTween.To(() => EvaluationTime, delegate(float t)
			{
				EvaluationTime = t;
				Color value = blinkSettings.colorGradient.Evaluate(t);
				foreach (GenericSprite sprite in sprites)
				{
					if ((bool)sprite?.SpriteRenderer)
					{
						sprite.SpriteRenderer.GetPropertyBlock(matPropertyBlock);
						matPropertyBlock.SetColor(colorProperty, value);
						sprite.SpriteRenderer.SetPropertyBlock(matPropertyBlock);
					}
				}
			}, 1f, blinkSettings.blinkDuration).SetEase(blinkSettings.blinkEase).OnComplete(delegate
			{
				damageTween = null;
			});
		}
		else
		{
			damageTween.Restart();
			EvaluationTime = 0.4f;
			damageTween.Goto(EvaluationTime * blinkSettings.blinkDuration, andPlay: true);
		}
	}
}
