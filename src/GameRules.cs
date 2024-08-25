using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace ImbalanceRespawnPlugin;

public class GameRules
{
	private CCSGameRules? CCSGameRules;

	public CCSGameRules Get()
	{
		if (CCSGameRules != null) return CCSGameRules;
		CCSGameRules = Utilities
			.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
			.First()
			.GameRules!;
		return CCSGameRules;
	}
}
