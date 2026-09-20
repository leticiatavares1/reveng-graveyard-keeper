using System.Collections.Generic;
using LazyBearTechnology;

public class UIAlchemyFolioWindowData : LazyWidgetDataBase
{
	public WgoData WgoData { get; private set; }

	public Dictionary<AlchemyFormulaTab, List<AlchemyFormulaDef>> Data { get; private set; }

	public void FillFromGaveSave(WgoData wgoData)
	{
		WgoData = wgoData;
		Data = new Dictionary<AlchemyFormulaTab, List<AlchemyFormulaDef>>();
		for (int i = 0; i < GameBalance.Me.alchemyFormulaDefs.Count; i++)
		{
			AlchemyFormulaDef alchemyFormulaDef = GameBalance.Me.alchemyFormulaDefs[i];
			if (!MainGame.Instance.GameSave.knowledgeSystem.hiddenAlchemyFormulas.Contains(alchemyFormulaDef.id) || MainGame.Instance.GameSave.knowledgeSystem.unlockedAlchemyFormulas.Contains(alchemyFormulaDef.id))
			{
				if (!Data.ContainsKey(alchemyFormulaDef.tab))
				{
					Data.Add(alchemyFormulaDef.tab, new List<AlchemyFormulaDef>());
				}
				Data[alchemyFormulaDef.tab].Add(alchemyFormulaDef);
			}
		}
	}
}
