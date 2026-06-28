using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Equipping;

public class Equippable_PumpShotgun : Equippable_RangedWeapon
{
	[Header("Shotgun Settings")]
	public int PelletCount = 8;

	protected override Vector3[] GetBulletDirections()
	{
		float spreadAngle = GetSpreadAngle();
		Vector3 forward = PlayerSingleton<PlayerCamera>.Instance.transform.forward;
		Vector3[] array = new Vector3[PelletCount];
		for (int i = 0; i < PelletCount; i++)
		{
			array[i] = Equippable_RangedWeapon.SpreadDirection(forward, spreadAngle);
		}
		return array;
	}
}
