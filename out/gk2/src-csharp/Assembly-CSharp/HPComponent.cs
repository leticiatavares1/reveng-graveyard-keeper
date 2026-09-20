using System;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class HPComponent : IComponent
{
	private const float DEATH_DELAY_TIME = 0.25f;

	private Action onDelayedDeadAction;

	private Action hp0Action;

	[SerializeField]
	private int maxHpValue;

	[SerializeField]
	private int hp;

	[NonSerialized]
	public int prevHp;

	public bool wasDamagedAtLeastOnce;

	public bool isDeathDelayed;

	public bool firstDamageWasMax;

	public bool IsImmuneToDamage { get; set; }

	public int Hp
	{
		get
		{
			return hp;
		}
		set
		{
			if (!isDeathDelayed)
			{
				prevHp = hp;
				hp = value;
				if (hp < 0)
				{
					hp = 0;
				}
				if (hp == 0)
				{
					HandleHp0();
				}
			}
		}
	}

	public int MaxHpValue => maxHpValue;

	public bool WasDamagedAtLeastOnce => wasDamagedAtLeastOnce;

	public bool HasFullHp => hp == maxHpValue;

	public event Action OnFirstDamageDealt;

	public event Action OnFullHpRestored;

	public event Action<HPComponent> OnHpChanged;

	public HPComponent()
	{
	}

	public HPComponent(int maxHpValue)
	{
		hp = maxHpValue;
		prevHp = maxHpValue;
		this.maxHpValue = maxHpValue;
	}

	public HPComponent(int maxHpValue, int startHpValue)
	{
		hp = startHpValue;
		prevHp = startHpValue;
		this.maxHpValue = maxHpValue;
	}

	public void Init(Action onDelayedDeadAction = null, Action hp0Action = null)
	{
		this.onDelayedDeadAction = onDelayedDeadAction;
		this.hp0Action = hp0Action;
		prevHp = hp;
		if (isDeathDelayed)
		{
			ScheduleCompleteDelayedDeathAfterWorldReady();
		}
	}

	public void ApplyDamage(int damage)
	{
		if (!IsImmuneToDamage && maxHpValue != -1 && !isDeathDelayed)
		{
			prevHp = hp;
			hp -= damage;
			hp = Mathf.Clamp(hp, 0, maxHpValue);
			if (!wasDamagedAtLeastOnce && hp != prevHp)
			{
				wasDamagedAtLeastOnce = true;
				this.OnFirstDamageDealt?.Invoke();
			}
			if (hp != prevHp)
			{
				this.OnHpChanged?.Invoke(this);
			}
			if (hp == 0)
			{
				HandleHp0();
			}
		}
	}

	public void AddHp(int value)
	{
		if (maxHpValue != -1 && !isDeathDelayed)
		{
			prevHp = hp;
			hp += value;
			hp = Mathf.Clamp(hp, 0, maxHpValue);
			if (hp != prevHp)
			{
				this.OnHpChanged?.Invoke(this);
			}
		}
	}

	public void RestoreFullHp()
	{
		prevHp = hp;
		hp = maxHpValue;
		wasDamagedAtLeastOnce = false;
		isDeathDelayed = false;
		this.OnHpChanged?.Invoke(this);
		this.OnFullHpRestored?.Invoke();
	}

	public void SetCustomHpValue(int newHp, bool overrideMaxHpValue = true)
	{
		hp = newHp;
		prevHp = newHp;
		if (overrideMaxHpValue)
		{
			maxHpValue = newHp;
		}
	}

	private void HandleHp0()
	{
		hp0Action?.Invoke();
		RunDelayedDeath();
	}

	private void HandleDeath()
	{
		onDelayedDeadAction?.Invoke();
	}

	private void RunDelayedDeath()
	{
		isDeathDelayed = true;
		LazyTimer.AddTimer(0.25f, delegate
		{
			DoDeath();
		});
	}

	private void DoDeath()
	{
		if (isDeathDelayed)
		{
			isDeathDelayed = false;
			HandleDeath();
		}
	}

	private void ScheduleCompleteDelayedDeathAfterWorldReady()
	{
		if (MainGame.Instance != null && MainGame.PlayerController.TryGetCurrentGameScene(out var _))
		{
			DoDeath();
		}
		else
		{
			MainGame.OnGameStarted = (Action)Delegate.Combine(MainGame.OnGameStarted, new Action(CompleteDelayedDeathOnGameStarted));
		}
	}

	private void CompleteDelayedDeathOnGameStarted()
	{
		MainGame.OnGameStarted = (Action)Delegate.Remove(MainGame.OnGameStarted, new Action(CompleteDelayedDeathOnGameStarted));
		if (isDeathDelayed)
		{
			DoDeath();
		}
	}
}
