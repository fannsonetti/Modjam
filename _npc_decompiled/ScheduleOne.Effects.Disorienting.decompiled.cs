using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Effects;

[CreateAssetMenu(fileName = "Disorienting", menuName = "Properties/Disorienting Property")]
public class Disorienting : Effect
{
	public override void ApplyToNPC(NPC npc)
	{
		npc.Movement.Disoriented = true;
		npc.Avatar.Eyes.leftEye.AngleOffset = new Vector2(20f, 10f);
		npc.Avatar.EmotionManager.AddEmotionOverride("Concerned", "disoriented");
	}

	public override void ApplyToPlayer(Player player)
	{
		player.Disoriented = true;
		player.Avatar.Eyes.leftEye.AngleOffset = new Vector2(20f, 10f);
		if (player.IsOwner)
		{
			PlayerSingleton<PlayerCamera>.Instance.SmoothLookSmoother.AddOverride(0.8f, Tier, "disoriented");
		}
	}

	public override void ClearFromNPC(NPC npc)
	{
		npc.Movement.Disoriented = false;
		npc.Avatar.Eyes.leftEye.AngleOffset = Vector2.zero;
		npc.Avatar.Eyes.rightEye.AngleOffset = Vector2.zero;
		npc.Avatar.EmotionManager.RemoveEmotionOverride("disoriented");
	}

	public override void ClearFromPlayer(Player player)
	{
		player.Disoriented = false;
		player.Avatar.Eyes.leftEye.AngleOffset = Vector2.zero;
		player.Avatar.Eyes.rightEye.AngleOffset = Vector2.zero;
		if (player.IsOwner)
		{
			PlayerSingleton<PlayerCamera>.Instance.SmoothLookSmoother.RemoveOverride("disoriented");
		}
	}
}
