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

internal static class ViewmodelAvatarHandHelper
{
    private static Transform _leftHand;
    private static Transform _rightHand;
    private static Vector3 _leftBasePosition;
    private static Vector3 _rightBasePosition;
    private static Vector3 _leftBaseEuler;
    private static Vector3 _rightBaseEuler;
    private static bool _captured;

    internal static void CaptureBaseline()
    {
        var avatar = Singleton<ViewmodelAvatar>.Instance;
        if (avatar == null)
            return;

        _leftHand = GetHandTransform(avatar, "LeftHandContainer");
        _rightHand = GetHandTransform(avatar, "RightHandContainer");

        if (_leftHand != null)
        {
            _leftBasePosition = _leftHand.localPosition;
            _leftBaseEuler = _leftHand.localEulerAngles;
        }

        if (_rightHand != null)
        {
            _rightBasePosition = _rightHand.localPosition;
            _rightBaseEuler = _rightHand.localEulerAngles;
        }

        _captured = _leftHand != null || _rightHand != null;
    }

    internal static void ApplyOffsets(ViewmodelProfile profile)
    {
        if (!_captured)
            CaptureBaseline();

        if (_leftHand != null)
        {
            _leftHand.localPosition = _leftBasePosition + profile.LeftHandOffset;
            _leftHand.localEulerAngles = _leftBaseEuler + profile.LeftHandEuler;
        }

        if (_rightHand != null)
        {
            _rightHand.localPosition = _rightBasePosition + profile.RightHandOffset;
            _rightHand.localEulerAngles = _rightBaseEuler + profile.RightHandEuler;
        }
    }

    internal static void RestoreBaseline()
    {
        if (!_captured)
            return;

        if (_leftHand != null)
        {
            _leftHand.localPosition = _leftBasePosition;
            _leftHand.localEulerAngles = _leftBaseEuler;
        }

        if (_rightHand != null)
        {
            _rightHand.localPosition = _rightBasePosition;
            _rightHand.localEulerAngles = _rightBaseEuler;
        }
    }

    internal static void Reset()
    {
        RestoreBaseline();
        _leftHand = null;
        _rightHand = null;
        _captured = false;
    }

    private static Transform GetHandTransform(object avatar, string memberName)
    {
        var type = avatar.GetType();
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        if (type.GetField(memberName, flags)?.GetValue(avatar) is Transform fieldTransform)
            return fieldTransform;

        if (type.GetProperty(memberName, flags)?.GetValue(avatar) is Transform propertyTransform)
            return propertyTransform;

        return null;
    }
}
