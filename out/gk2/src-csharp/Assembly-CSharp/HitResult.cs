public readonly struct HitResult
{
	public readonly HitResultType type;

	public readonly float damageMultiplier;

	public bool ShouldDealDamage
	{
		get
		{
			if (type == HitResultType.Pass)
			{
				return damageMultiplier > 0f;
			}
			return false;
		}
	}

	public bool ShouldStopProjectile
	{
		get
		{
			HitResultType hitResultType = type;
			return hitResultType == HitResultType.Blocked || hitResultType == HitResultType.Deflected;
		}
	}

	private HitResult(HitResultType type, float damageMultiplier = 1f)
	{
		this.type = type;
		this.damageMultiplier = damageMultiplier;
	}

	public static HitResult Pass(float multiplier = 1f)
	{
		return new HitResult(HitResultType.Pass, multiplier);
	}

	public static HitResult Blocked()
	{
		return new HitResult(HitResultType.Blocked, 0f);
	}

	public static HitResult Absorbed()
	{
		return new HitResult(HitResultType.Absorbed, 0f);
	}

	public static HitResult Deflected()
	{
		return new HitResult(HitResultType.Deflected, 0f);
	}

	public static HitResult ZoneMarked()
	{
		return new HitResult(HitResultType.ZoneMarked);
	}
}
