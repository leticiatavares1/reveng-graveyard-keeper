using PI.NGSS;

public readonly struct NgssQualitySettings
{
	public readonly int directionalSamplingTest;

	public readonly int directionalSamplingFilter;

	public readonly bool directionalPcssEnabled;

	public readonly bool directionalCascadesBlending;

	public readonly int localSamplingTest;

	public readonly int localSamplingFilter;

	public NgssQualitySettings(int directionalSamplingTest, int directionalSamplingFilter, bool directionalPcssEnabled, bool directionalCascadesBlending, int localSamplingTest, int localSamplingFilter)
	{
		this.directionalSamplingTest = directionalSamplingTest;
		this.directionalSamplingFilter = directionalSamplingFilter;
		this.directionalPcssEnabled = directionalPcssEnabled;
		this.directionalCascadesBlending = directionalCascadesBlending;
		this.localSamplingTest = localSamplingTest;
		this.localSamplingFilter = localSamplingFilter;
	}

	public void Apply(NGSS_Directional directional, NGSS_Local local)
	{
		if (directional != null)
		{
			directional.NGSS_SAMPLING_TEST = directionalSamplingTest;
			directional.NGSS_SAMPLING_FILTER = directionalSamplingFilter;
			directional.NGSS_PCSS_ENABLED = directionalPcssEnabled;
			directional.NGSS_CASCADES_BLENDING = directionalCascadesBlending;
		}
		if (local != null)
		{
			local.NGSS_SAMPLING_TEST = localSamplingTest;
			local.NGSS_SAMPLING_FILTER = localSamplingFilter;
		}
	}
}
