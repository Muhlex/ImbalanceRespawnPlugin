using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;

namespace ImbalanceRespawnPlugin;

[MinimumApiVersion(160)]
public class ImbalanceRespawnPlugin : BasePlugin
{
	public override string ModuleName => "Imbalance Respawn";
	public override string ModuleDescription => "Gives teams as many extra lives as they are missing players.";
	public override string ModuleAuthor => "murlis";
	public override string ModuleVersion => "1.0.0";

	private readonly Dictionary<CsTeam, int> TeamExtraLives = [];
	private readonly (
		Dictionary<CsTeam, ConVar> mp_respawn_on_death,
		ConVar mp_randomspawn
	) ConVars = (
		new Dictionary<CsTeam, ConVar>() {
				{ CsTeam.Terrorist, ConVar.Find("mp_respawn_on_death_t")! },
				{ CsTeam.CounterTerrorist, ConVar.Find("mp_respawn_on_death_ct")! },
		},
		ConVar.Find("mp_randomspawn")!
	);
	private readonly GameRules GameRules = new();

	[GameEventHandler]
	public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
	{
		TeamExtraLives.Clear();
		foreach (var conVar in ConVars.mp_respawn_on_death.Values) conVar.SetValue(false);
		return HookResult.Continue;
	}

	[GameEventHandler]
	public HookResult OnRoundFreezeEnd(EventRoundFreezeEnd @event, GameEventInfo info)
	{
		if (GameRules.Get().WarmupPeriod) return HookResult.Continue;

		Dictionary<CsTeam, int> teamAliveCount = [];
		foreach (var player in Utilities.GetPlayers())
		{
			if (!player.PawnIsAlive) continue;
			if (player.Team == CsTeam.Spectator || player.Team == CsTeam.None) continue;
			if (teamAliveCount.TryGetValue(player.Team, out int value)) teamAliveCount[player.Team] = value + 1;
			else teamAliveCount[player.Team] = 1;
		}

		if (teamAliveCount.Count == 0) return HookResult.Continue;

		var lives = teamAliveCount.Max(x => x.Value);
		foreach (var (team, aliveCount) in teamAliveCount)
		{
			TeamExtraLives[team] = lives - aliveCount;
		}

		Console.WriteLine(
			"Team extra lives: {0}",
			string.Join(", ", TeamExtraLives.Select(pair => $"{pair.Key}: {pair.Value}"))
		);

		foreach (var (team, conVar) in ConVars.mp_respawn_on_death)
		{
			if (TeamExtraLives.GetValueOrDefault(team, 0) > 0) conVar.SetValue(true);
		}

		return HookResult.Continue;
	}

	[GameEventHandler]
	public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
	{
		var gameRules = GameRules.Get();
		if (gameRules.WarmupPeriod || gameRules.FreezePeriod) return HookResult.Continue;

		var player = @event.Userid;
		if (player == null || !player.IsValid) return HookResult.Continue;

		var team = player.Team;
		var teamLives = TeamExtraLives.GetValueOrDefault(team, 0);
		if (teamLives == 0) return HookResult.Continue;

		Server.NextFrame(() =>
		{
			foreach (var p in Utilities.GetPlayers())
			{
				p.PrintToCenterAlert($"{player.PlayerName} respawned due to team imbalance.");
			}
		});

		TeamExtraLives[team] = --teamLives;
		if (teamLives == 0) ConVars.mp_respawn_on_death[player.Team].SetValue(false);

		return HookResult.Continue;
	}
}
