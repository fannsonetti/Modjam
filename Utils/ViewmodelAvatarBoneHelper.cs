using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#if IL2CPP
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.PlayerScripts;
#else
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
#endif

namespace MoreWeapons.Utils;

internal static class ViewmodelAvatarBoneHelper
{
    private static Animator _animator;
    private static RuntimeAnimatorController _originalController;
    private static RuntimeAnimatorController _editController;
    private static bool _sessionActive;
    private static float _savedAnimatorSpeed = 1f;

    private static Transform _leftHand;
    private static Transform _rightHand;
    private static Transform _leftForeArm;
    private static Transform _rightForeArm;

    internal static bool HasSession => _sessionActive;

    internal static Animator Animator => _animator;

    internal static RuntimeAnimatorController OriginalController => _originalController;

    internal static RuntimeAnimatorController EditController => _editController;

    internal static void BeginEditorSession(RuntimeAnimatorController weaponController)
    {
        EndEditorSession();

        var avatar = Singleton<ViewmodelAvatar>.Instance;
        if (avatar == null)
            return;

        _animator = GetAnimator(avatar);
        if (_animator == null)
            return;

        ResolveBones(avatar);
        _originalController = _animator.runtimeAnimatorController;

        var source = weaponController != null ? weaponController : _originalController;
        if (source != null)
        {
            _editController = Object.Instantiate(source);
            _editController.name = source.name + "_ViewmodelEditor";
            _animator.runtimeAnimatorController = _editController;
        }

        _savedAnimatorSpeed = _animator.speed;
        _animator.speed = 0f;
        _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        _sessionActive = true;
    }

    internal static void EndEditorSession()
    {
        if (_animator != null)
        {
            _animator.speed = _savedAnimatorSpeed;
            if (_originalController != null)
                _animator.runtimeAnimatorController = _originalController;
        }

        if (_editController != null)
        {
            Object.Destroy(_editController);
            _editController = null;
        }

        _sessionActive = false;
        _originalController = null;
        _animator = null;
        _leftHand = null;
        _rightHand = null;
        _leftForeArm = null;
        _rightForeArm = null;
    }

    internal static void EvaluatePose(float aimAmount)
    {
        if (_animator == null)
            return;

        _animator.SetFloat("Aim", aimAmount);
        _animator.Update(0f);
    }

    internal static void ApplyBoneOverrides(ViewmodelProfile profile, float aimAmount)
    {
        if (_animator == null)
        {
            var avatar = Singleton<ViewmodelAvatar>.Instance;
            if (avatar == null)
                return;

            _animator = GetAnimator(avatar);
            ResolveBones(avatar);
        }

        if (_animator == null)
            return;

        EvaluatePose(aimAmount);
        ApplyOffset(_leftHand, profile.LeftHandOffset, profile.LeftHandEuler);
        ApplyOffset(_rightHand, profile.RightHandOffset, profile.RightHandEuler);
        ApplyOffset(_leftForeArm, profile.LeftForeArmOffset, profile.LeftForeArmEuler);
        ApplyOffset(_rightForeArm, profile.RightForeArmOffset, profile.RightForeArmEuler);
    }

    internal static void CaptureIntoProfile(ref ViewmodelProfile profile)
    {
        EvaluatePose(0f);
        profile.LeftHandOffset = ReadOffset(_leftHand);
        profile.LeftHandEuler = ReadEuler(_leftHand);
        profile.RightHandOffset = ReadOffset(_rightHand);
        profile.RightHandEuler = ReadEuler(_rightHand);
        profile.LeftForeArmOffset = ReadOffset(_leftForeArm);
        profile.LeftForeArmEuler = ReadEuler(_leftForeArm);
        profile.RightForeArmOffset = ReadOffset(_rightForeArm);
        profile.RightForeArmEuler = ReadEuler(_rightForeArm);
    }

    internal static RuntimeAnimatorController CloneController(RuntimeAnimatorController source, string name)
    {
        if (source == null)
            return null;

        var clone = Object.Instantiate(source);
        clone.name = name;
        return clone;
    }

    private static void ApplyOffset(Transform bone, Vector3 positionOffset, Vector3 eulerOffset)
    {
        if (bone == null)
            return;

        if (positionOffset != Vector3.zero)
            bone.localPosition += positionOffset;

        if (eulerOffset != Vector3.zero)
            bone.localEulerAngles += eulerOffset;
    }

    private static Vector3 ReadOffset(Transform bone) => bone != null ? bone.localPosition : Vector3.zero;

    private static Vector3 ReadEuler(Transform bone) => bone != null ? bone.localEulerAngles : Vector3.zero;

    private static Animator GetAnimator(object avatar)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var type = avatar.GetType();

        if (type.GetProperty("Animator", flags)?.GetValue(avatar) is Animator propertyAnimator)
            return propertyAnimator;

        if (type.GetField("Animator", flags)?.GetValue(avatar) is Animator fieldAnimator)
            return fieldAnimator;

        var body = type.GetProperty("BodyContainer", flags)?.GetValue(avatar) as Transform
            ?? type.GetField("BodyContainer", flags)?.GetValue(avatar) as Transform;
        return body != null ? body.GetComponentInChildren<Animator>(true) : null;
    }

    private static void ResolveBones(object avatar)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var type = avatar.GetType();
        var body = type.GetProperty("BodyContainer", flags)?.GetValue(avatar) as Transform
            ?? type.GetField("BodyContainer", flags)?.GetValue(avatar) as Transform;

        if (body == null)
            return;

        var transforms = body.GetComponentsInChildren<Transform>(true);
        _leftHand = FindBone(transforms, "LeftHand");
        _rightHand = FindBone(transforms, "RightHand");
        _leftForeArm = FindBone(transforms, "LeftForeArm", "LeftForearm");
        _rightForeArm = FindBone(transforms, "RightForeArm", "RightForearm");
    }

    private static Transform FindBone(IList<Transform> transforms, params string[] nameFragments)
    {
        foreach (var fragment in nameFragments)
        {
            foreach (var transform in transforms)
            {
                if (transform.name.IndexOf(fragment, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return transform;
            }
        }

        return null;
    }
}
