using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;

namespace ImbalanceRespawnPlugin;

public class ConVars
{
	public readonly Dictionary<CsTeam, ConVar> RespawnOnDeath = new()
	{
		[CsTeam.Terrorist] = ConVar.Find("mp_respawn_on_death_t")!,
		[CsTeam.CounterTerrorist] = ConVar.Find("mp_respawn_on_death_ct")!
	};
}
