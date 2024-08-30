using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace ImbalanceRespawnPlugin;

public class GlobalObjects
{
	private CCSGameRules? _GameRules;
	public CCSGameRules GameRules
	{
		get {
			if (_GameRules != null) return _GameRules;
			_GameRules = Utilities
				.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
				.First()
				.GameRules!;
			return _GameRules;
		}
	}

	public GlobalObjects(ImbalanceRespawnPlugin plugin)
	{
		plugin.RegisterListener<Listeners.OnMapEnd>(() =>
		{
			_GameRules = null;
		});
	}
}
