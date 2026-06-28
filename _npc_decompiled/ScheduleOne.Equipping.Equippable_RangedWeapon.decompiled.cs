using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne.Audio;
using ScheduleOne.Combat;
using ScheduleOne.DevUtilities;
using ScheduleOne.FX;
using ScheduleOne.ItemFramework;
using ScheduleOne.NPCs;
using ScheduleOne.Noise;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Storage;
using ScheduleOne.Trash;
using ScheduleOne.UI;
using ScheduleOne.Vision;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Equipping;

public class Equippable_RangedWeapon : Equippable_AvatarViewmodel
{
	public enum EReloadType
	{
		Magazine,
		Incremental
	}

	public const float NPC_AIM_DETECTION_RANGE = 10f;

	public int MagazineSize = 7;

	[Header("Aim Settings")]
	public float AimDuration = 0.2f;

	public float MinAimFOVReduction = 5f;

	public float MaxAimFOVReduction = 10f;

	[Header("Firing")]
	public AudioSourceController FireSound;

	public AudioSourceController EmptySound;

	public float FireCooldown = 0.3f;

	public string[] FireAnimTriggers;

	public float AccuracyChangeDuration = 0.6f;

	public float AccuracyDropPerShot = 0.4f;

	[Header("Raycasting")]
	public float Range = 40f;

	public float RayRadius = 0.05f;

	[Header("Spread")]
	public float MinSpread = 5f;

	public float MaxSpread = 15f;

	[Header("Damage")]
	public float Damage = 60f;

	public float ImpactForce = 300f;

	public float HeadshotMultiplier = 1.75f;

	[Header("Reloading")]
	public bool CanReload = true;

	public EReloadType ReloadType;

	public StorableItemDefinition Magazine;

	public float ReloadStartTime = 1.5f;

	public float ReloadIndividalTime;

	public float ReloadEndTime;

	public string ReloadStartAnimTrigger = "MagazineReload";

	public string ReloadIndividualAnimTrigger = string.Empty;

	public string ReloadEndAnimTrigger = string.Empty;

	public TrashItem ReloadTrash;

	[Header("Cocking")]
	public bool MustBeCocked;

	public bool CockedByDefault;

	public bool AutoCockAfterReload;

	public float CockTime = 0.5f;

	public string CockAnimTrigger = "MagazineReload";

	[Header("Effects")]
	public float TracerSpeed = 50f;

	public UnityEvent onFire;

	public UnityEvent onReloadStart;

	public UnityEvent onReloadIndividual;

	public UnityEvent onReloadEnd;

	public UnityEvent onCockStart;

	protected IntegerItemInstance weaponItem;

	private bool aimStarted;

	private float aimVelocity;

	private Coroutine reloadRoutine;

	private bool shotQueued;

	private bool reloadQueued;

	private float timeSincePrimaryClick = 100f;

	private float timeSinceReloadStart;

	private float timeSinceAimStart;

	private bool interruptReload;

	public float Aim { get; private set; }

	public float Accuracy { get; private set; }

	public float TimeSinceFire { get; set; } = 1000f;

	public bool IsReloading { get; private set; }

	public bool IsCocked { get; private set; }

	public bool IsCocking { get; private set; }

	public int Ammo
	{
		get
		{
			if (weaponItem == null)
			{
				return 0;
			}
			return weaponItem.Value;
		}
	}

	private float fov => Singleton<Settings>.Instance.CameraFOV - (aimStarted ? Mathf.Lerp(MinAimFOVReduction, MaxAimFOVReduction, Mathf.Clamp01(timeSinceAimStart / AccuracyChangeDuration)) : 0f);

	public override void Equip(ItemInstance item)
	{
		base.Equip(item);
		Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
		Singleton<InputPromptsCanvas>.Instance.LoadModule("gun");
		weaponItem = item as IntegerItemInstance;
		InvokeRepeating("CheckAimingAtNPC", 0f, 0.5f);
		if (CockedByDefault)
		{
			IsCocked = true;
		}
	}

	public override void Unequip()
	{
		base.Unequip();
		Singleton<HUD>.Instance.SetCrosshairVisible(vis: true);
		Singleton<InputPromptsCanvas>.Instance.UnloadModule();
		if (aimStarted)
		{
			PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(AimDuration);
			PlayerSingleton<PlayerMovement>.Instance.RemoveSprintBlocker("Aiming");
			aimStarted = false;
		}
		Singleton<HUD>.Instance.HideFirearmReticle();
		if (reloadRoutine != null)
		{
			StopCoroutine(reloadRoutine);
		}
	}

	protected override void Update()
	{
		base.Update();
		UpdateInput();
		UpdateAnim();
		Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
		TimeSinceFire += Time.deltaTime;
		if (IsReloading)
		{
			timeSinceReloadStart += Time.deltaTime;
		}
	}

	private void UpdateInput()
	{
		if (Time.timeScale == 0f)
		{
			return;
		}
		if ((GameInput.GetButton(GameInput.ButtonCode.SecondaryClick) || timeSincePrimaryClick < 0.5f || IsCocking) && CanAim())
		{
			Aim = Mathf.SmoothDamp(Aim, 1f, ref aimVelocity, AimDuration / 2f);
			Accuracy = Mathf.MoveTowards(Accuracy, 1f, Time.deltaTime / AccuracyChangeDuration);
			timeSinceAimStart += Time.deltaTime;
			if (!aimStarted)
			{
				PlayerSingleton<PlayerMovement>.Instance.AddSprintBlocker("Aiming");
				aimStarted = true;
				timeSinceAimStart = 0f;
				Player.Local.SendEquippableMessage_Networked("Raise", UnityEngine.Random.Range(int.MinValue, int.MaxValue));
			}
		}
		else
		{
			if (TimeSinceFire > FireCooldown)
			{
				Aim = Mathf.SmoothDamp(Aim, 0f, ref aimVelocity, AimDuration / 2f);
			}
			Accuracy = Mathf.MoveTowards(Accuracy, 0f, Time.deltaTime / AccuracyChangeDuration * 2f);
			if (aimStarted)
			{
				PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(AimDuration);
				PlayerSingleton<PlayerMovement>.Instance.RemoveSprintBlocker("Aiming");
				aimStarted = false;
				Player.Local.SendEquippableMessage_Networked("Lower", UnityEngine.Random.Range(int.MinValue, int.MaxValue));
			}
		}
		float t = Mathf.Clamp01(PlayerSingleton<PlayerMovement>.Instance.Controller.velocity.magnitude / 3.25f);
		float num = Mathf.Lerp(1f, 0f, t);
		if (Accuracy > num)
		{
			Accuracy = Mathf.MoveTowards(Accuracy, num, Time.deltaTime / AccuracyChangeDuration * 2f);
		}
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(fov, AimDuration);
		if (Singleton<PauseMenu>.Instance.IsPaused)
		{
			return;
		}
		if (GameInput.GetButton(GameInput.ButtonCode.PrimaryClick))
		{
			timeSincePrimaryClick = 0f;
			if (IsReloading && ReloadType == EReloadType.Incremental && timeSinceReloadStart > ReloadStartTime + ReloadIndividalTime * 0.5f)
			{
				interruptReload = true;
			}
		}
		else
		{
			timeSincePrimaryClick += Time.deltaTime;
		}
		if (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick) || shotQueued)
		{
			if (CanFire(checkAmmo: false))
			{
				if (Ammo > 0)
				{
					if (!MustBeCocked || IsCocked)
					{
						Fire();
					}
					else
					{
						Cock();
					}
				}
				else if (EmptySound != null)
				{
					EmptySound.Play();
					shotQueued = false;
					if (IsReloadReady(ignoreTiming: false))
					{
						Reload();
					}
				}
			}
			else if (TimeSinceFire < FireCooldown || IsCocking)
			{
				shotQueued = true;
			}
		}
		if (reloadQueued || GameInput.GetButtonDown(GameInput.ButtonCode.Reload))
		{
			if (IsReloadReady(ignoreTiming: false))
			{
				Reload();
			}
			else if (GameInput.GetButtonDown(GameInput.ButtonCode.Reload) && IsReloadReady(ignoreTiming: true) && TimeSinceFire > FireCooldown * 0.5f)
			{
				Console.Log("Reload qeueued");
				reloadQueued = true;
			}
		}
	}

	private void UpdateAnim()
	{
		Singleton<ViewmodelAvatar>.Instance.Animator.SetFloat("Aim", Aim);
		Singleton<HUD>.Instance.SetFirearmReticle(GetSpreadAngle());
		if (Aim > 0.5f)
		{
			Singleton<HUD>.Instance.ShowFirearmReticle();
		}
		else
		{
			Singleton<HUD>.Instance.HideFirearmReticle();
		}
	}

	private bool CanAim()
	{
		return true;
	}

	public virtual void Fire()
	{
		IsCocked = false;
		shotQueued = false;
		TimeSinceFire = 0f;
		Singleton<ViewmodelAvatar>.Instance.Animator.SetTrigger(FireAnimTriggers[UnityEngine.Random.Range(0, FireAnimTriggers.Length)]);
		PlayerSingleton<PlayerCamera>.Instance.JoltCamera();
		FireSound.Play();
		weaponItem.ChangeValue(-1);
		Vector3[] bulletDirections = GetBulletDirections();
		Vector3 position = PlayerSingleton<PlayerCamera>.Instance.transform.position;
		position += PlayerSingleton<PlayerCamera>.Instance.transform.forward * 0.4f;
		position += PlayerSingleton<PlayerCamera>.Instance.transform.right * 0.1f;
		position += PlayerSingleton<PlayerCamera>.Instance.transform.up * -0.03f;
		NoiseUtility.EmitNoise(base.transform.position, ENoiseType.Gunshot, 25f, Player.Local.gameObject);
		if (Player.Local.CurrentProperty == null)
		{
			Player.Local.VisualState.ApplyState("shooting", EVisualState.DischargingWeapon, 4f);
		}
		Dictionary<IDamageable, List<RaycastHit>> dictionary = new Dictionary<IDamageable, List<RaycastHit>>();
		Vector3[] array = bulletDirections;
		foreach (Vector3 vector in array)
		{
			Singleton<FXManager>.Instance.CreateBulletTrail(position, vector, TracerSpeed, Range, NetworkSingleton<CombatManager>.Instance.RangedWeaponLayerMask);
			Vector3 data = PlayerSingleton<PlayerCamera>.Instance.transform.position + vector * Range;
			RaycastHit[] array2 = Physics.SphereCastAll(position, RayRadius, vector, Range, NetworkSingleton<CombatManager>.Instance.RangedWeaponLayerMask);
			Array.Sort(array2, (RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance));
			RaycastHit[] array3 = array2;
			for (int num = 0; num < array3.Length; num++)
			{
				RaycastHit item = array3[num];
				if (item.collider.gameObject.CompareTag("CombatIgnore"))
				{
					continue;
				}
				IDamageable componentInParent = item.collider.GetComponentInParent<IDamageable>();
				if (componentInParent != null && componentInParent == Player.Local)
				{
					continue;
				}
				if (componentInParent != null)
				{
					if (!dictionary.ContainsKey(componentInParent))
					{
						dictionary.Add(componentInParent, new List<RaycastHit>());
					}
					dictionary[componentInParent].Add(item);
				}
				data = item.point;
				break;
			}
			Player.Local.SendEquippableMessage_Networked_Vector("Shoot", UnityEngine.Random.Range(int.MinValue, int.MaxValue), data);
		}
		foreach (IDamageable key in dictionary.Keys)
		{
			List<RaycastHit> list = dictionary[key];
			Vector3 hitPoint = Vector3.zero;
			list.ForEach(delegate(RaycastHit hit)
			{
				hitPoint += hit.point;
			});
			hitPoint /= (float)list.Count;
			float impactForce = ImpactForce * (float)list.Count;
			float num2 = 0f;
			for (int num3 = 0; num3 < list.Count; num3++)
			{
				float num4 = Damage;
				if (list[num3].collider.CompareTag("Head"))
				{
					num4 *= HeadshotMultiplier;
					Debug.Log("Headshot! Damage: " + num4);
				}
				num2 += num4;
			}
			Impact impact = new Impact(hitPoint, PlayerSingleton<PlayerCamera>.Instance.transform.forward, impactForce, num2, EImpactType.Bullet, Player.Local.NetworkObject, UnityEngine.Random.Range(int.MinValue, int.MaxValue));
			key.SendImpact(impact);
			Singleton<FXManager>.Instance.CreateImpactFX(impact, key);
		}
		Accuracy = Mathf.Max(Accuracy - AccuracyDropPerShot, 0f);
		if (onFire != null)
		{
			onFire.Invoke();
		}
	}

	protected virtual Vector3[] GetBulletDirections()
	{
		return new Vector3[1] { SpreadDirection(PlayerSingleton<PlayerCamera>.Instance.transform.forward, GetSpreadAngle()) };
	}

	protected static Vector3 SpreadDirection(Vector3 direction, float maxAngle)
	{
		direction.Normalize();
		float maxInclusive = maxAngle * (MathF.PI / 180f);
		float f = UnityEngine.Random.Range(0f, maxInclusive);
		float f2 = UnityEngine.Random.Range(0f, MathF.PI * 2f);
		float x = Mathf.Sin(f) * Mathf.Cos(f2);
		float y = Mathf.Sin(f) * Mathf.Sin(f2);
		float z = Mathf.Cos(f);
		Vector3 vector = new Vector3(x, y, z);
		return (Quaternion.FromToRotation(Vector3.forward, direction) * vector).normalized;
	}

	public virtual void Reload()
	{
		reloadQueued = false;
		IsReloading = true;
		interruptReload = false;
		timeSinceReloadStart = 0f;
		Console.Log("Reloading...");
		reloadRoutine = StartCoroutine(ReloadRoutine());
		IEnumerator ReloadRoutine()
		{
			if (onReloadStart != null)
			{
				onReloadStart.Invoke();
			}
			Singleton<ViewmodelAvatar>.Instance.Animator.SetTrigger(ReloadStartAnimTrigger);
			yield return new WaitForSeconds(ReloadStartTime);
			StorableItemInstance mag2;
			if (ReloadType == EReloadType.Incremental)
			{
				StorableItemInstance mag;
				while (weaponItem.Value < MagazineSize && GetMagazine(out mag) && !interruptReload)
				{
					if (onReloadIndividual != null)
					{
						onReloadIndividual.Invoke();
					}
					Singleton<ViewmodelAvatar>.Instance.Animator.SetTrigger(ReloadIndividualAnimTrigger);
					yield return new WaitForSeconds(ReloadIndividalTime);
					weaponItem.ChangeValue(1);
					if (mag is IntegerItemInstance)
					{
						IntegerItemInstance obj = mag as IntegerItemInstance;
						obj.ChangeValue(-1);
						if (obj.Value <= 0)
						{
							mag.ChangeQuantity(-1);
							if (ReloadTrash != null)
							{
								Vector3 posiiton = PlayerSingleton<PlayerCamera>.Instance.transform.position - PlayerSingleton<PlayerCamera>.Instance.transform.up * 0.4f;
								NetworkSingleton<TrashManager>.Instance.CreateTrashItem(ReloadTrash.ID, posiiton, UnityEngine.Random.rotation);
							}
						}
					}
					else
					{
						mag.ChangeQuantity(-1);
					}
					NotifyIncrementalReload();
				}
				yield return new WaitForSeconds(0.05f);
				if (onReloadEnd != null)
				{
					onReloadEnd.Invoke();
				}
				Singleton<ViewmodelAvatar>.Instance.Animator.SetTrigger(ReloadEndAnimTrigger);
				yield return new WaitForSeconds(ReloadEndTime);
			}
			else if (ReloadType == EReloadType.Magazine && GetMagazine(out mag2))
			{
				IntegerItemInstance obj2 = mag2 as IntegerItemInstance;
				obj2.ChangeValue(-(MagazineSize - weaponItem.Value));
				if (obj2.Value <= 0)
				{
					mag2.ChangeQuantity(-1);
					if (ReloadTrash != null)
					{
						Vector3 posiiton2 = PlayerSingleton<PlayerCamera>.Instance.transform.position - PlayerSingleton<PlayerCamera>.Instance.transform.up * 0.4f;
						NetworkSingleton<TrashManager>.Instance.CreateTrashItem(ReloadTrash.ID, posiiton2, UnityEngine.Random.rotation);
					}
				}
				weaponItem.SetValue(MagazineSize);
			}
			if (MustBeCocked && !IsCocked && AutoCockAfterReload)
			{
				Cock();
			}
			Console.Log("Reloading done!");
			IsReloading = false;
			reloadRoutine = null;
		}
	}

	protected virtual void NotifyIncrementalReload()
	{
	}

	private bool IsReloadReady(bool ignoreTiming)
	{
		if (!CanReload)
		{
			return false;
		}
		if (IsReloading)
		{
			return false;
		}
		if (!GetMagazine(out var _))
		{
			return false;
		}
		if (weaponItem.Value >= MagazineSize)
		{
			return false;
		}
		if (TimeSinceFire < FireCooldown && !ignoreTiming)
		{
			return false;
		}
		if (!base.equipAnimDone && !ignoreTiming)
		{
			return false;
		}
		if (IsCocking)
		{
			return false;
		}
		return true;
	}

	protected virtual bool GetMagazine(out StorableItemInstance mag)
	{
		mag = null;
		for (int i = 0; i < PlayerSingleton<PlayerInventory>.Instance.hotbarSlots.Count; i++)
		{
			if (PlayerSingleton<PlayerInventory>.Instance.hotbarSlots[i].Quantity != 0 && PlayerSingleton<PlayerInventory>.Instance.hotbarSlots[i].ItemInstance.ID == Magazine.ID)
			{
				mag = PlayerSingleton<PlayerInventory>.Instance.hotbarSlots[i].ItemInstance as StorableItemInstance;
				return true;
			}
		}
		return false;
	}

	private bool CanFire(bool checkAmmo = true)
	{
		if (TimeSinceFire < FireCooldown)
		{
			return false;
		}
		if (Aim < 0.1f)
		{
			return false;
		}
		if (!base.equipAnimDone)
		{
			return false;
		}
		if (checkAmmo && Ammo <= 0)
		{
			return false;
		}
		if (IsReloading)
		{
			return false;
		}
		if (IsCocking)
		{
			return false;
		}
		return true;
	}

	private bool CanCock()
	{
		if (IsCocked)
		{
			return false;
		}
		if (IsCocking)
		{
			return false;
		}
		if (weaponItem.Value <= 0)
		{
			return false;
		}
		if (!base.equipAnimDone)
		{
			return false;
		}
		if (IsReloading)
		{
			return false;
		}
		if (TimeSinceFire < FireCooldown)
		{
			return false;
		}
		return true;
	}

	private void Cock()
	{
		shotQueued = false;
		IsCocking = true;
		StartCoroutine(CockRoutine());
		IEnumerator CockRoutine()
		{
			if (onCockStart != null)
			{
				onCockStart.Invoke();
			}
			Singleton<ViewmodelAvatar>.Instance.Animator.SetTrigger(CockAnimTrigger);
			yield return new WaitForSeconds(CockTime);
			IsCocked = true;
			IsCocking = false;
		}
	}

	protected float GetSpreadAngle()
	{
		return Mathf.Lerp(MaxSpread, MinSpread, Accuracy);
	}

	private void CheckAimingAtNPC()
	{
		if (Aim < 0.5f)
		{
			return;
		}
		RaycastHit[] array = Physics.SphereCastAll(new Ray(PlayerSingleton<PlayerCamera>.Instance.transform.position, PlayerSingleton<PlayerCamera>.Instance.transform.forward), 0.5f, 10f, NetworkSingleton<CombatManager>.Instance.RangedWeaponLayerMask);
		List<NPC> list = new List<NPC>();
		RaycastHit[] array2 = array;
		foreach (RaycastHit raycastHit in array2)
		{
			NPC componentInParent = raycastHit.collider.GetComponentInParent<NPC>();
			if (componentInParent != null && !list.Contains(componentInParent))
			{
				list.Add(componentInParent);
				if (componentInParent.Awareness.VisionCone.IsPlayerVisible(Player.Local))
				{
					componentInParent.Responses.RespondToAimedAt(Player.Local);
				}
			}
		}
	}
}
