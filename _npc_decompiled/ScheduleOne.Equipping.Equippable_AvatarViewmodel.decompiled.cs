using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Equipping;

public class Equippable_AvatarViewmodel : Equippable_Viewmodel
{
	public RuntimeAnimatorController AnimatorController;

	public Vector3 ViewmodelAvatarOffset = Vector3.zero;

	public Vector3 ViewmodelAvatarRotationOffset = Vector3.zero;

	[Header("Equipping")]
	public float EquipTime = 0.4f;

	public string EquipTrigger = "Equip";

	protected float timeEquipped;

	protected bool equipAnimDone => timeEquipped >= EquipTime;

	public override void Equip(ItemInstance item)
	{
		base.transform.SetParent(Singleton<ViewmodelAvatar>.Instance.RightHandContainer);
		if (AnimatorController != null)
		{
			Singleton<ViewmodelAvatar>.Instance.SetAnimatorController(AnimatorController);
			Singleton<ViewmodelAvatar>.Instance.SetVisibility(isVisible: true);
			Singleton<ViewmodelAvatar>.Instance.SetOffset(ViewmodelAvatarOffset);
			Singleton<ViewmodelAvatar>.Instance.SetRotationOffset(ViewmodelAvatarRotationOffset);
		}
		base.Equip(item);
	}

	public override void Unequip()
	{
		base.Unequip();
		Singleton<ViewmodelAvatar>.Instance.SetVisibility(isVisible: false);
	}

	protected override void PlayEquipAnimation()
	{
		base.PlayEquipAnimation();
		if (EquipTrigger != string.Empty)
		{
			Singleton<ViewmodelAvatar>.Instance.Animator.SetTrigger(EquipTrigger);
		}
	}

	protected override void Update()
	{
		base.Update();
		timeEquipped += Time.deltaTime;
	}
}
