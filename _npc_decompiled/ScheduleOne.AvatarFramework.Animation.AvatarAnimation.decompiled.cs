using System;
using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Skating;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.AvatarFramework.Animation;

public class AvatarAnimation : MonoBehaviour
{
	public enum EFlinchType
	{
		Light,
		Heavy
	}

	public enum EFlinchDirection
	{
		Forward,
		Backward,
		Left,
		Right
	}

	public const bool ImpostorsEnabled = true;

	public const float AnimationRangeSqr = 2500f;

	public const float FrustrumCullMinDist = 225f;

	public const float RunningAnimationSpeed = 8f;

	public const float MaxBoneOffset = 0.01f;

	public const float MaxBoneOffsetSqr = 0.0001f;

	public static Vector3 SITTING_OFFSET = new Vector3(0f, -0.825f, 0f);

	public const float SEAT_TIME = 0.5f;

	private const string StandUpFromBackClipName = "Stand up from back";

	private const string StandUpFromFrontClipName = "Stand up from front";

	public bool DEBUG_MODE;

	[Header("References")]
	public Animator animator;

	public Transform HipBone;

	public Transform[] Bones;

	protected Avatar avatar;

	public Transform LeftHandContainer;

	public Transform RightHandContainer;

	public Transform RightHandAlignmentPoint;

	public Transform LeftHandAlignmentPoint;

	public AvatarIKController IKController;

	public AvatarFootstepDetector FootstepDetector;

	[Header("Settings")]
	public LayerMask GroundingMask;

	public bool AllowCulling = true;

	public UnityEvent onStandupStart;

	public UnityEvent onStandupDone;

	public UnityEvent onHeavyFlinch;

	private BoneTransform[] standUpFromBackBoneTransforms;

	private BoneTransform[] standUpFromFrontBoneTransforms;

	private BoneTransform[] ragdollBoneTransforms;

	private Coroutine standUpRoutine;

	private Coroutine seatRoutine;

	private Skateboard activeSkateboard;

	private bool animationEnabled = true;

	private BoneTransform[] _lastFrameBoneTransforms;

	public bool IsCrouched { get; protected set; }

	public bool IsSeated => CurrentSeat != null;

	public float TimeSinceSitEnd { get; protected set; } = 1000f;

	public AvatarSeat CurrentSeat { get; protected set; }

	public bool StandUpAnimationPlaying { get; protected set; }

	public bool IsAvatarCulled { get; private set; }

	protected virtual void Awake()
	{
		avatar = GetComponent<Avatar>();
		avatar.onRagdollChange.AddListener(RagdollChange);
		ragdollBoneTransforms = new BoneTransform[Bones.Length];
		for (int i = 0; i < Bones.Length; i++)
		{
			ragdollBoneTransforms[i] = new BoneTransform();
		}
		Player componentInParent = GetComponentInParent<Player>();
		if (componentInParent != null)
		{
			componentInParent.onSkateboardMounted = (Action<Skateboard>)Delegate.Combine(componentInParent.onSkateboardMounted, new Action<Skateboard>(SkateboardMounted));
			componentInParent.onSkateboardDismounted = (Action)Delegate.Combine(componentInParent.onSkateboardDismounted, new Action(SkateboardDismounted));
		}
		_lastFrameBoneTransforms = new BoneTransform[Bones.Length];
		animator.keepAnimatorStateOnDisable = true;
		InvokeRepeating("UpdateAnimationActive", UnityEngine.Random.Range(0f, 0.5f), 0.1f);
	}

	private void Start()
	{
		if (standUpFromBackBoneTransforms == null)
		{
			standUpFromBackBoneTransforms = new BoneTransform[Bones.Length];
			for (int i = 0; i < Bones.Length; i++)
			{
				standUpFromBackBoneTransforms[i] = new BoneTransform();
			}
			PopulateAnimationStartBoneTransforms("Stand up from back", standUpFromBackBoneTransforms);
		}
		if (standUpFromFrontBoneTransforms == null)
		{
			standUpFromFrontBoneTransforms = new BoneTransform[Bones.Length];
			for (int j = 0; j < Bones.Length; j++)
			{
				standUpFromFrontBoneTransforms[j] = new BoneTransform();
			}
			PopulateAnimationStartBoneTransforms("Stand up from front", standUpFromFrontBoneTransforms);
		}
	}

	private void Update()
	{
		if (IsSeated)
		{
			TimeSinceSitEnd = 0f;
		}
		else
		{
			TimeSinceSitEnd += Time.deltaTime;
		}
		if (seatRoutine == null && CurrentSeat != null)
		{
			base.transform.position = CurrentSeat.SittingPoint.position + SITTING_OFFSET * base.transform.localScale.y;
			base.transform.rotation = CurrentSeat.SittingPoint.rotation;
		}
	}

	private void LateUpdate()
	{
		PopulateBoneTransforms(_lastFrameBoneTransforms);
	}

	private void UpdateAnimationActive()
	{
		if (!PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			return;
		}
		float num = Vector3.SqrMagnitude(PlayerSingleton<PlayerCamera>.Instance.transform.position - avatar.CenterPoint);
		bool flag = num < 2500f * QualitySettings.lodBias;
		if (flag && num > 225f)
		{
			flag = Vector3.Dot(PlayerSingleton<PlayerCamera>.Instance.transform.forward, avatar.CenterPoint - PlayerSingleton<PlayerCamera>.Instance.transform.position) > 0f;
		}
		if (DEBUG_MODE)
		{
			Debug.Log("Avatar Animation In Range: " + flag + " | DistSqr: " + num, this);
		}
		if (Time.timeSinceLevelLoad < 3f)
		{
			flag = true;
		}
		if (!AllowCulling)
		{
			flag = true;
		}
		bool isAvatarCulled = IsAvatarCulled;
		IsAvatarCulled = !flag;
		if (IsAvatarCulled != isAvatarCulled)
		{
			avatar.BodyContainer.gameObject.SetActive(!IsAvatarCulled);
			if (IsAvatarCulled)
			{
				avatar.Impostor.EnableImpostor();
			}
			else
			{
				avatar.Impostor.DisableImpostor();
			}
		}
		animator.enabled = animationEnabled && !IsAvatarCulled;
	}

	public void SetDirection(float dir)
	{
		animator.SetFloat(AAId.DIRECTION, dir);
	}

	public void SetStrafe(float strafe)
	{
		animator.SetFloat(AAId.STRAFE, strafe);
	}

	public void SetTimeAirborne(float airbone)
	{
		animator.SetFloat(AAId.TIME_AIRBORNE, airbone);
	}

	public void SetCrouched(bool crouched)
	{
		IsCrouched = crouched;
		animator.SetBool(AAId.IS_CROUCHED, crouched);
	}

	public void SetGrounded(bool grounded)
	{
		animator.SetBool(AAId.IS_GROUNDED, grounded);
	}

	public void Jump()
	{
		animator.SetTrigger(AAId.JUMP);
	}

	public void SetAnimationEnabled(bool enabled)
	{
		if (DEBUG_MODE)
		{
			Console.Log("Setting animation enabled: " + enabled, this);
		}
		animationEnabled = enabled;
		UpdateAnimationActive();
	}

	public void ResetAnimatorState()
	{
		bool keepAnimatorStateOnDisable = animator.keepAnimatorStateOnDisable;
		animator.keepAnimatorStateOnDisable = false;
		if (base.gameObject.activeInHierarchy)
		{
			animator.gameObject.SetActive(value: false);
			animator.gameObject.SetActive(value: true);
		}
		animator.keepAnimatorStateOnDisable = keepAnimatorStateOnDisable;
	}

	public void Flinch(Vector3 forceDirection, EFlinchType flinchType)
	{
		Vector3 vector = base.transform.InverseTransformDirection(forceDirection);
		EFlinchDirection eFlinchDirection = EFlinchDirection.Forward;
		eFlinchDirection = ((Mathf.Abs(vector.z) > Mathf.Abs(vector.x)) ? ((!(vector.z > 0f)) ? EFlinchDirection.Backward : EFlinchDirection.Forward) : ((!(vector.x > 0f)) ? EFlinchDirection.Left : EFlinchDirection.Right));
		if (flinchType == EFlinchType.Light)
		{
			switch (eFlinchDirection)
			{
			case EFlinchDirection.Forward:
				animator.SetTrigger(AAId.FLINCH_FORWARD);
				break;
			case EFlinchDirection.Backward:
				animator.SetTrigger(AAId.FLINCH_BACKWARD);
				break;
			case EFlinchDirection.Left:
				animator.SetTrigger(AAId.FLINCH_LEFT);
				break;
			case EFlinchDirection.Right:
				animator.SetTrigger(AAId.FLINCH_RIGHT);
				break;
			}
			return;
		}
		switch (eFlinchDirection)
		{
		case EFlinchDirection.Forward:
			animator.SetTrigger(AAId.FLINCH_HEAVY_FORWARD);
			break;
		case EFlinchDirection.Backward:
			animator.SetTrigger(AAId.FLINCH_HEAVY_BACKWARD);
			break;
		case EFlinchDirection.Left:
			animator.SetTrigger(AAId.FLINCH_HEAVY_LEFT);
			break;
		case EFlinchDirection.Right:
			animator.SetTrigger(AAId.FLINCH_HEAVY_RIGHT);
			break;
		}
		if (onHeavyFlinch != null)
		{
			onHeavyFlinch.Invoke();
		}
	}

	public void PlayStandUpAnimation()
	{
		StandUpAnimationPlaying = true;
		if (onStandupStart != null)
		{
			onStandupStart.Invoke();
		}
		PopulateBoneTransforms(ragdollBoneTransforms);
		bool standUpFromBack = ShouldGetUpFromBack();
		PopulateAnimationStartBoneTransforms("Stand up from front", standUpFromFrontBoneTransforms);
		PopulateAnimationStartBoneTransforms("Stand up from back", standUpFromBackBoneTransforms);
		BoneTransform[] finalBoneTransforms = (standUpFromBack ? standUpFromBackBoneTransforms : standUpFromFrontBoneTransforms);
		if (standUpRoutine != null)
		{
			Singleton<CoroutineService>.Instance.StopCoroutine(standUpRoutine);
		}
		standUpRoutine = Singleton<CoroutineService>.Instance.StartCoroutine(StandUpRoutine());
		IEnumerator StandUpRoutine()
		{
			float time = 0.3f;
			for (int i = 0; i < Bones.Length; i++)
			{
				Rigidbody component = null;
				if (Bones[i].TryGetComponent<Rigidbody>(out component))
				{
					component.interpolation = RigidbodyInterpolation.None;
				}
			}
			for (float i2 = 0f; i2 < time; i2 += Time.deltaTime)
			{
				for (int j = 0; j < Bones.Length; j++)
				{
					Bones[j].localPosition = Vector3.Lerp(ragdollBoneTransforms[j].Position, finalBoneTransforms[j].Position, i2 / time);
					Bones[j].localRotation = Quaternion.Lerp(ragdollBoneTransforms[j].Rotation, finalBoneTransforms[j].Rotation, i2 / time);
				}
				yield return new WaitForEndOfFrame();
			}
			for (int k = 0; k < Bones.Length; k++)
			{
				Bones[k].localPosition = finalBoneTransforms[k].Position;
				Bones[k].localRotation = finalBoneTransforms[k].Rotation;
			}
			SetAnimationEnabled(enabled: true);
			if (animator.enabled)
			{
				int trigger = (standUpFromBack ? AAId.STANDUP_BACK : AAId.STANDUP_FRONT);
				animator.SetTrigger(trigger);
			}
			for (int l = 0; l < Bones.Length; l++)
			{
				Rigidbody component2 = null;
				if (Bones[l].TryGetComponent<Rigidbody>(out component2))
				{
					component2.interpolation = RigidbodyInterpolation.Interpolate;
				}
			}
			yield return new WaitForSecondsRealtime(1.5f);
			if (onStandupDone != null)
			{
				onStandupDone.Invoke();
			}
			standUpRoutine = null;
			StandUpAnimationPlaying = false;
		}
	}

	protected void RagdollChange(bool wasRagdolled, bool ragdoll, bool playStandUpAnim)
	{
		if (wasRagdolled == ragdoll)
		{
			return;
		}
		if (ragdoll && IsSeated)
		{
			if (CurrentSeat != null)
			{
				CurrentSeat.SetOccupant(null);
				CurrentSeat = null;
			}
			animator.SetBool(AAId.SITTING, value: false);
			GetComponentInParent<NPCMovement>().SpeedController.RemoveSpeedControl("seated");
		}
		if (ragdoll)
		{
			if (standUpRoutine != null)
			{
				StopCoroutine(standUpRoutine);
			}
			SetAnimationEnabled(enabled: false);
			ApplyBoneTransforms(_lastFrameBoneTransforms);
		}
		else
		{
			ResetAnimatorState();
			AlignPositionToHips();
			if (playStandUpAnim)
			{
				PlayStandUpAnimation();
			}
			else
			{
				SetAnimationEnabled(enabled: true);
			}
		}
	}

	private void AlignPositionToHips()
	{
		Vector3 position = HipBone.position;
		Quaternion rotation = HipBone.rotation;
		base.transform.position = HipBone.position;
		if (Physics.Raycast(base.transform.position, Vector3.down, out var hitInfo, 10f, GroundingMask))
		{
			base.transform.position = new Vector3(base.transform.position.x, hitInfo.point.y, base.transform.position.z);
		}
		base.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(ShouldGetUpFromBack() ? (-HipBone.up) : HipBone.up, Vector3.up), Vector3.up);
		HipBone.position = position;
		HipBone.rotation = rotation;
	}

	private bool ShouldGetUpFromBack()
	{
		return Vector3.Angle(HipBone.forward, Vector3.up) < 90f;
	}

	private void PopulateBoneTransforms(BoneTransform[] boneTransforms)
	{
		for (int i = 0; i < Bones.Length; i++)
		{
			boneTransforms[i] = new BoneTransform();
			boneTransforms[i].Position = Bones[i].localPosition;
			boneTransforms[i].Rotation = Bones[i].localRotation;
		}
	}

	private void PopulateAnimationStartBoneTransforms(string clipName, BoneTransform[] boneTransforms)
	{
		Vector3 position = animator.transform.position;
		Quaternion rotation = animator.transform.rotation;
		if (animator.runtimeAnimatorController == null)
		{
			return;
		}
		AnimationClip[] animationClips = animator.runtimeAnimatorController.animationClips;
		foreach (AnimationClip animationClip in animationClips)
		{
			if (animationClip.name == clipName)
			{
				animationClip.SampleAnimation(animator.gameObject, 0f);
				PopulateBoneTransforms(boneTransforms);
				break;
			}
		}
		animator.transform.position = position;
		animator.transform.rotation = rotation;
	}

	private void ApplyBoneTransforms(BoneTransform[] boneTransforms)
	{
		if (boneTransforms == null)
		{
			return;
		}
		for (int i = 0; i < Bones.Length; i++)
		{
			if (boneTransforms[i] != null)
			{
				Bones[i].localPosition = boneTransforms[i].Position;
				Bones[i].localRotation = boneTransforms[i].Rotation;
			}
		}
	}

	public void SetTrigger(string trigger)
	{
		if (!string.IsNullOrEmpty(trigger))
		{
			if (animator != null)
			{
				int trigger2 = AAId.Get(trigger);
				animator.SetTrigger(trigger2);
			}
			UpdateAnimationActive();
		}
	}

	public void ResetTrigger(string trigger)
	{
		int id = AAId.Get(trigger);
		animator.ResetTrigger(id);
	}

	public void SetBool(string id, bool value)
	{
		if (animator != null)
		{
			int id2 = AAId.Get(id);
			animator.SetBool(id2, value);
		}
		UpdateAnimationActive();
	}

	public void SetSeat(AvatarSeat seat)
	{
		Vector3 startPos;
		Quaternion startRot;
		Vector3 endPos;
		Quaternion endRot;
		if (!(seat == CurrentSeat))
		{
			if (CurrentSeat != null)
			{
				CurrentSeat.SetOccupant(null);
			}
			CurrentSeat = seat;
			if (CurrentSeat != null)
			{
				CurrentSeat.SetOccupant(GetComponentInParent<NPC>());
			}
			animator.SetBool(AAId.SITTING, IsSeated);
			startPos = base.transform.position;
			startRot = base.transform.rotation;
			if (seatRoutine != null)
			{
				Singleton<CoroutineService>.Instance.StopCoroutine(seatRoutine);
			}
			if (CurrentSeat != null)
			{
				endPos = CurrentSeat.SittingPoint.position + SITTING_OFFSET * base.transform.localScale.y;
				endRot = CurrentSeat.SittingPoint.rotation;
				seatRoutine = Singleton<CoroutineService>.Instance.StartCoroutine(Lerp(resetLocalCoordinates: false));
				GetComponentInParent<NPCMovement>().SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("seated", 100, -1f));
			}
			else
			{
				endPos = base.transform.parent.position;
				endRot = base.transform.parent.rotation;
				seatRoutine = Singleton<CoroutineService>.Instance.StartCoroutine(Lerp(resetLocalCoordinates: true));
			}
		}
		IEnumerator Lerp(bool resetLocalCoordinates)
		{
			for (float i = 0f; i < 0.5f; i += Time.deltaTime)
			{
				base.transform.position = Vector3.Lerp(startPos, endPos, i / 0.5f);
				base.transform.rotation = Quaternion.Lerp(startRot, endRot, i / 0.5f);
				yield return new WaitForEndOfFrame();
			}
			base.transform.position = endPos;
			base.transform.rotation = endRot;
			if (resetLocalCoordinates)
			{
				NPCMovement componentInParent = GetComponentInParent<NPCMovement>();
				if (componentInParent != null)
				{
					componentInParent.transform.position = endPos;
					componentInParent.transform.rotation = endRot;
					componentInParent.SpeedController.RemoveSpeedControl("seated");
				}
				base.transform.localPosition = Vector3.zero;
				base.transform.localRotation = Quaternion.identity;
			}
			seatRoutine = null;
		}
	}

	public void SkateboardMounted(Skateboard board)
	{
		IKController.BodyIK.solvers.pelvis.target = board.Animation.PelvisAlignment.Transform;
		IKController.BodyIK.solvers.spine.target = board.Animation.SpineAlignment.Transform;
		IKController.BodyIK.solvers.leftFoot.target = board.Animation.LeftFootAlignment.Transform;
		IKController.BodyIK.solvers.rightFoot.target = board.Animation.RightFootAlignment.Transform;
		IKController.BodyIK.solvers.leftHand.target = board.Animation.LeftHandAlignment.Transform;
		IKController.BodyIK.solvers.rightHand.target = board.Animation.RightHandAlignment.Transform;
		IKController.BodyIK.solvers.rightFoot.SetBendPlaneToCurrent();
		IKController.BodyIK.solvers.leftFoot.SetBendPlaneToCurrent();
		IKController.OverrideLegBendTargets(board.Animation.LeftLegBendTarget.Transform, board.Animation.RightLegBendTarget.Transform);
		IKController.SetIKActive(active: true);
		avatar.SetEquippable(string.Empty);
		avatar.LookController.ForceLookTarget = board.Animation.AvatarFaceTarget;
		avatar.LookController.ForceLookRotateBody = true;
		SetBool("SkateIdle", value: true);
		activeSkateboard = board;
		activeSkateboard.OnPushStart.AddListener(SkateboardPush);
	}

	public void SkateboardDismounted()
	{
		IKController.ResetLegBendTargets();
		IKController.SetIKActive(active: false);
		avatar.LookController.ForceLookTarget = null;
		avatar.LookController.ForceLookRotateBody = false;
		SetBool("SkateIdle", value: false);
		activeSkateboard.OnPushStart.RemoveListener(SkateboardPush);
		activeSkateboard = null;
	}

	private void SkateboardPush()
	{
		SetTrigger("SkatePush");
	}
}
