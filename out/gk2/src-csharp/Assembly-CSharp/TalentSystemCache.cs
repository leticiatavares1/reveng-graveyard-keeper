using System.Collections.Generic;

public class TalentSystemCache
{
	public Dictionary<string, TalentDef> talents = new Dictionary<string, TalentDef>();

	public Dictionary<string, List<TalentLevelUpDef>> levelUpsByTalentId = new Dictionary<string, List<TalentLevelUpDef>>();

	public Dictionary<string, InspirationData> inspirations = new Dictionary<string, InspirationData>();

	public Dictionary<string, List<InspirationLevelData>> inspirationsByTalentId = new Dictionary<string, List<InspirationLevelData>>();

	private static TalentSystemCache instance;

	public static TalentSystemCache Instance
	{
		get
		{
			if (instance != null)
			{
				return instance;
			}
			instance = new TalentSystemCache();
			return instance;
		}
	}

	public void CreateCache()
	{
		foreach (TalentDef talentDef in GameBalance.Me.talentDefs)
		{
			talents.Add(talentDef.id, talentDef);
		}
		foreach (TalentLevelUpDef talentLevelUpDef in GameBalance.Me.talentLevelUpDefs)
		{
			AddLevelUp(talentLevelUpDef);
		}
		foreach (InspirationLevelData inspirationLevel in GameBalance.Me.inspirationLevels)
		{
			AddInspiration(inspirationLevel);
		}
		foreach (string key in talents.Keys)
		{
			if (!inspirationsByTalentId.TryGetValue(key, out var value))
			{
				continue;
			}
			foreach (InspirationLevelData item in value)
			{
				InspirationData inspirationData = new InspirationData();
				inspirationData.id = item.id;
				inspirationData.curLevel = 1;
				inspirationData.curProgressValue = 0;
				inspirationData.completionGoalValue = item.levels[0].completionGoalValue;
				inspirations.Add(inspirationData.id, inspirationData);
			}
		}
	}

	public void ClearCache()
	{
		talents.Clear();
		levelUpsByTalentId.Clear();
		inspirations.Clear();
		inspirationsByTalentId.Clear();
	}

	private void AddLevelUp(TalentLevelUpDef def)
	{
		if (!levelUpsByTalentId.ContainsKey(def.talentId))
		{
			levelUpsByTalentId[def.talentId] = new List<TalentLevelUpDef>();
		}
		levelUpsByTalentId[def.talentId].Add(def);
	}

	private void AddInspiration(InspirationLevelData inspirationLevelData)
	{
		string talentId = inspirationLevelData.talentId;
		if (!inspirationsByTalentId.ContainsKey(talentId))
		{
			inspirationsByTalentId[talentId] = new List<InspirationLevelData>();
		}
		inspirationsByTalentId[talentId].Add(inspirationLevelData);
	}
}
