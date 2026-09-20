public static class NgssQualityPresets
{
	public static NgssQualitySettings Get(NgssQualityPreset preset)
	{
		return preset switch
		{
			NgssQualityPreset.Medium => new NgssQualitySettings(16, 32, directionalPcssEnabled: true, directionalCascadesBlending: true, 16, 24), 
			NgssQualityPreset.Low => new NgssQualitySettings(8, 16, directionalPcssEnabled: false, directionalCascadesBlending: false, 8, 12), 
			_ => new NgssQualitySettings(20, 48, directionalPcssEnabled: true, directionalCascadesBlending: true, 15, 32), 
		};
	}
}
