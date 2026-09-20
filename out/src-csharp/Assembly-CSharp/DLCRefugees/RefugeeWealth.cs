namespace DLCRefugees;

public class RefugeeWealth
{
	public float water_satiety_coeff;

	public float energy_satiety_coeff;

	public int bed_wealth;

	public int refugees_count;

	public RefugeeWealth(float water_satiety_coeff, float energy_satiety_coeff, int refugees_count)
	{
		this.water_satiety_coeff = water_satiety_coeff;
		this.energy_satiety_coeff = energy_satiety_coeff;
		this.refugees_count = refugees_count;
	}
}
