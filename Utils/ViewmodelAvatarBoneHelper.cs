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
    private static readonly string[] LeftHandNames = { "mixamorig:LeftHand", "LeftHand" };
    private static readonly string[] RightHandNames = { "mixamorig:RightHand", "RightHand" };
    private static readonly string[] LeftForeArmNames = { "mixamorig:LeftForeArm", "LeftForeArm", "LeftForearm" };
    private static readonly string[] RightForeArmNames = { "mixamorig:RightForeArm", "RightForeArm", "RightForearm" };

    private static Animator _animator;
    private static GameObject _sampleRoot;
    private static bool _sessionActive;
    private static float _savedAnimatorSpeed = 1f;
    private static AnimatorUpdateMode _savedUpdateMode = AnimatorUpdateMode.Normal;

    private static Transform _leftHand;
    private static Transform _rightHand;
    private static Transform _leftForeArm;
    private static Transform _rightForeArm;

    internal static bool HasSession => _sessionActive;
    internal static Animator Animator => _animator;
    internal static string ResolvedBoneStatus { get; private set; } = "Bones not resolved";

    internal static GameObject GetSampleRoot()
    {
        if (_sampleRoot != null)
            return _sampleRoot;

        var avatar = Singleton<ViewmodelAvatar>.Instance;
        if (avatar == null)
            return null;

        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var type = avatar.GetType();
        var body = type.GetProperty("BodyContainer", flags)?.GetValue(avatar) as Transform
            ?? type.GetField("BodyContainer", flags)?.GetValue(avatar) as Transform;

        _sampleRoot = body != null ? body.gameObject : avatar.gameObject;
        return _sampleRoot;
    }

    internal static void BeginEditorSession()
    {
        EndEditorSession();

        var avatar = Singleton<ViewmodelAvatar>.Instance;
        if (avatar == null)
            return;

        _animator = GetAnimator(avatar);
        if (_animator == null)
            return;

        ResolveBones(_animator);
        GetSampleRoot();

        _savedAnimatorSpeed = _animator.speed;
        _savedUpdateMode = _animator.updateMode;
        _animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        _animator.speed = 0f;
        _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        _sessionActive = true;
    }

    internal static void EndEditorSession()
    {
        if (_animator != null)
        {
            _animator.speed = _savedAnimatorSpeed;
            _animator.updateMode = _savedUpdateMode;
        }

        _sessionActive = false;
        _animator = null;
        _sampleRoot = null;
        _leftHand = null;
        _rightHand = null;
        _leftForeArm = null;
        _rightForeArm = null;
        ResolvedBoneStatus = "Bones not resolved";
    }

    internal static void EvaluatePose(float aimAmount)
    {
        if (_animator == null)
            return;

        _animator.SetFloat("Aim", aimAmount);
        _animator.Update(0f);
    }

    internal static void SampleClip(AnimationClip clip, float timeSeconds)
    {
        if (clip == null)
            return;

        var root = GetSampleRoot();
        if (root == null)
            return;

        clip.SampleAnimation(root, timeSeconds);
    }

    internal static void ApplyBoneOverrides(ViewmodelProfile profile)
    {
        if (!EnsureBonesResolved())
            return;

        ApplyOffset(_leftHand, profile.LeftHandOffset, profile.LeftHandEuler);
        ApplyOffset(_rightHand, profile.RightHandOffset, profile.RightHandEuler);
        ApplyOffset(_leftForeArm, profile.LeftForeArmOffset, profile.LeftForeArmEuler);
        ApplyOffset(_rightForeArm, profile.RightForeArmOffset, profile.RightForeArmEuler);
    }

    internal static void ApplyKeyframeOffsets(ViewmodelAnimKeyframe keyframe)
    {
        if (!EnsureBonesResolved())
            return;

        ApplyOffset(_leftHand, keyframe.LeftHandOffset, keyframe.LeftHandEuler);
        ApplyOffset(_rightHand, keyframe.RightHandOffset, keyframe.RightHandEuler);
        ApplyOffset(_leftForeArm, keyframe.LeftForeArmOffset, keyframe.LeftForeArmEuler);
        ApplyOffset(_rightForeArm, keyframe.RightForeArmOffset, keyframe.RightForeArmEuler);
    }

    internal static void PrepareRuntimeSampling()
    {
        var avatar = Singleton<ViewmodelAvatar>.Instance;
        if (avatar == null)
            return;

        _animator = GetAnimator(avatar);
        if (_animator != null)
            ResolveBones(_animator);

        GetSampleRoot();
    }

    internal static void CaptureIntoProfile(ref ViewmodelProfile profile)
    {
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

        var clone = UnityEngine.Object.Instantiate(source);
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

    private static bool EnsureBonesResolved()
    {
        if (_leftHand != null || _rightHand != null || _leftForeArm != null || _rightForeArm != null)
            return true;

        var avatar = Singleton<ViewmodelAvatar>.Instance;
        if (avatar == null)
            return false;

        var animator = GetAnimator(avatar);
        if (animator == null)
            return false;

        ResolveBones(animator);
        return _leftHand != null || _rightHand != null || _leftForeArm != null || _rightForeArm != null;
    }

    private static void ResolveBones(Animator animator)
    {
        _leftHand = GetHumanoidOrNamedBone(animator, HumanBodyBones.LeftHand, LeftHandNames);
        _rightHand = GetHumanoidOrNamedBone(animator, HumanBodyBones.RightHand, RightHandNames);
        _leftForeArm = GetHumanoidOrNamedBone(animator, HumanBodyBones.LeftLowerArm, LeftForeArmNames);
        _rightForeArm = GetHumanoidOrNamedBone(animator, HumanBodyBones.RightLowerArm, RightForeArmNames);

        ResolvedBoneStatus =
            $"LHand={NameOf(_leftHand)} RHand={NameOf(_rightHand)} " +
            $"LFore={NameOf(_leftForeArm)} RFore={NameOf(_rightForeArm)}";
    }

    private static Transform GetHumanoidOrNamedBone(Animator animator, HumanBodyBones humanBone, string[] names)
    {
        if (animator.isHuman)
        {
            var human = animator.GetBoneTransform(humanBone);
            if (human != null)
                return human;
        }

        var root = animator.transform;
        var transforms = root.GetComponentsInChildren<Transform>(true);
        foreach (var exact in names)
        {
            foreach (var transform in transforms)
            {
                if (transform.name == exact)
                    return transform;
            }
        }

        foreach (var fragment in names)
        {
            foreach (var transform in transforms)
            {
                var name = transform.name;
                if (name.IndexOf("Container", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;
                if (name.IndexOf("Alignment", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;
                if (name.IndexOf(fragment, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return transform;
            }
        }

        return null;
    }

    private static string NameOf(Transform bone) => bone != null ? bone.name : "missing";
}
