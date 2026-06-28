using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace MoreWeapons.Utils;

internal struct ViewmodelAnimKeyframe
{
    public float Time;
    public float Aim;

    public Vector3 HipPosition;
    public Vector3 HipEuler;
    public float UniformScale;
    public Vector3 AimEuler;
    public Vector3 AimPositionOffset;

    public Vector3 EquippableLocalPosition;
    public Vector3 EquippableLocalEuler;
    public Vector3 ModelLocalPosition;
    public Vector3 ModelLocalEuler;
    public float ModelUniformScale;
    public Vector3 MapRaiseEquippablePosition;
    public Vector3 MapRaiseEquippableEuler;

    public Vector3 ThirdPersonPosition;
    public Vector3 ThirdPersonEuler;
    public float ThirdPersonWorldScale;
    public Vector3 LeftHandOffset;
    public Vector3 LeftHandEuler;
    public Vector3 RightHandOffset;
    public Vector3 RightHandEuler;

    internal static ViewmodelAnimKeyframe FromProfile(ViewmodelProfile profile, float time, float aim)
    {
        return new ViewmodelAnimKeyframe
        {
            Time = time,
            Aim = aim,
            HipPosition = profile.HipPosition,
            HipEuler = profile.HipEuler,
            UniformScale = profile.UniformScale,
            AimEuler = profile.AimEuler,
            AimPositionOffset = profile.AimPositionOffset,
            EquippableLocalPosition = profile.EquippableLocalPosition,
            EquippableLocalEuler = profile.EquippableLocalEuler,
            ModelLocalPosition = profile.ModelLocalPosition,
            ModelLocalEuler = profile.ModelLocalEuler,
            ModelUniformScale = profile.ModelUniformScale,
            MapRaiseEquippablePosition = profile.MapRaiseEquippablePosition,
            MapRaiseEquippableEuler = profile.MapRaiseEquippableEuler,
            ThirdPersonPosition = profile.ThirdPersonPosition,
            ThirdPersonEuler = profile.ThirdPersonEuler,
            ThirdPersonWorldScale = profile.ThirdPersonWorldScale,
            LeftHandOffset = profile.LeftHandOffset,
            LeftHandEuler = profile.LeftHandEuler,
            RightHandOffset = profile.RightHandOffset,
            RightHandEuler = profile.RightHandEuler,
        };
    }

    internal ViewmodelProfile ToProfile(ViewmodelPresentationMode mode, string label)
    {
        return new ViewmodelProfile
        {
            Label = label,
            PresentationMode = mode,
            HipPosition = HipPosition,
            HipEuler = HipEuler,
            UniformScale = UniformScale,
            AimEuler = AimEuler,
            AimPositionOffset = AimPositionOffset,
            EquippableLocalPosition = EquippableLocalPosition,
            EquippableLocalEuler = EquippableLocalEuler,
            ModelLocalPosition = ModelLocalPosition,
            ModelLocalEuler = ModelLocalEuler,
            ModelUniformScale = ModelUniformScale,
            MapRaiseEquippablePosition = MapRaiseEquippablePosition,
            MapRaiseEquippableEuler = MapRaiseEquippableEuler,
            ThirdPersonPosition = ThirdPersonPosition,
            ThirdPersonEuler = ThirdPersonEuler,
            ThirdPersonWorldScale = ThirdPersonWorldScale,
            LeftHandOffset = LeftHandOffset,
            LeftHandEuler = LeftHandEuler,
            RightHandOffset = RightHandOffset,
            RightHandEuler = RightHandEuler,
        };
    }

    internal static ViewmodelAnimKeyframe Lerp(ViewmodelAnimKeyframe a, ViewmodelAnimKeyframe b, float t)
    {
        return new ViewmodelAnimKeyframe
        {
            Time = Mathf.Lerp(a.Time, b.Time, t),
            Aim = Mathf.Lerp(a.Aim, b.Aim, t),
            HipPosition = Vector3.Lerp(a.HipPosition, b.HipPosition, t),
            HipEuler = Vector3.Lerp(a.HipEuler, b.HipEuler, t),
            UniformScale = Mathf.Lerp(a.UniformScale, b.UniformScale, t),
            AimEuler = Vector3.Lerp(a.AimEuler, b.AimEuler, t),
            AimPositionOffset = Vector3.Lerp(a.AimPositionOffset, b.AimPositionOffset, t),
            EquippableLocalPosition = Vector3.Lerp(a.EquippableLocalPosition, b.EquippableLocalPosition, t),
            EquippableLocalEuler = Vector3.Lerp(a.EquippableLocalEuler, b.EquippableLocalEuler, t),
            ModelLocalPosition = Vector3.Lerp(a.ModelLocalPosition, b.ModelLocalPosition, t),
            ModelLocalEuler = Vector3.Lerp(a.ModelLocalEuler, b.ModelLocalEuler, t),
            ModelUniformScale = Mathf.Lerp(a.ModelUniformScale, b.ModelUniformScale, t),
            MapRaiseEquippablePosition = Vector3.Lerp(a.MapRaiseEquippablePosition, b.MapRaiseEquippablePosition, t),
            MapRaiseEquippableEuler = Vector3.Lerp(a.MapRaiseEquippableEuler, b.MapRaiseEquippableEuler, t),
            ThirdPersonPosition = Vector3.Lerp(a.ThirdPersonPosition, b.ThirdPersonPosition, t),
            ThirdPersonEuler = Vector3.Lerp(a.ThirdPersonEuler, b.ThirdPersonEuler, t),
            ThirdPersonWorldScale = Mathf.Lerp(a.ThirdPersonWorldScale, b.ThirdPersonWorldScale, t),
            LeftHandOffset = Vector3.Lerp(a.LeftHandOffset, b.LeftHandOffset, t),
            LeftHandEuler = Vector3.Lerp(a.LeftHandEuler, b.LeftHandEuler, t),
            RightHandOffset = Vector3.Lerp(a.RightHandOffset, b.RightHandOffset, t),
            RightHandEuler = Vector3.Lerp(a.RightHandEuler, b.RightHandEuler, t),
        };
    }
}

internal sealed class ViewmodelAnimClip
{
    internal string Name = "Custom";
    internal float Duration = 0.5f;
    internal readonly List<ViewmodelAnimKeyframe> Keyframes = new();

    internal bool TrySample(float normalizedTime, out ViewmodelAnimKeyframe sample)
    {
        sample = default;
        if (Keyframes.Count == 0)
            return false;

        normalizedTime = Mathf.Clamp01(normalizedTime);
        Keyframes.Sort((a, b) => a.Time.CompareTo(b.Time));

        if (Keyframes.Count == 1)
        {
            sample = Keyframes[0];
            return true;
        }

        if (normalizedTime <= Keyframes[0].Time)
        {
            sample = Keyframes[0];
            return true;
        }

        var last = Keyframes[Keyframes.Count - 1];
        if (normalizedTime >= last.Time)
        {
            sample = last;
            return true;
        }

        for (var i = 0; i < Keyframes.Count - 1; i++)
        {
            var a = Keyframes[i];
            var b = Keyframes[i + 1];
            if (normalizedTime < a.Time || normalizedTime > b.Time)
                continue;

            var span = Mathf.Max(0.0001f, b.Time - a.Time);
            var t = (normalizedTime - a.Time) / span;
            sample = ViewmodelAnimKeyframe.Lerp(a, b, t);
            return true;
        }

        sample = last;
        return true;
    }

    internal string ToCSharpSnippet(string fieldName)
    {
        Keyframes.Sort((a, b) => a.Time.CompareTo(b.Time));
        var sb = new StringBuilder();
        sb.AppendLine($"internal static ViewmodelAnimClip {fieldName} => new()");
        sb.AppendLine("{");
        sb.AppendLine($"    Name = \"{Name}\",");
        sb.AppendLine($"    Duration = {Format(Duration)}f,");
        sb.AppendLine("    Keyframes = new List<ViewmodelAnimKeyframe>");
        sb.AppendLine("    {");

        foreach (var keyframe in Keyframes)
            sb.AppendLine("        " + FormatKeyframe(keyframe) + ",");

        sb.AppendLine("    },");
        sb.AppendLine("};");
        return sb.ToString();
    }

    private static string FormatKeyframe(ViewmodelAnimKeyframe k)
    {
        return "new() { " +
               $"Time = {Format(k.Time)}f, Aim = {Format(k.Aim)}f, " +
               $"HipPosition = {FormatVector(k.HipPosition)}, HipEuler = {FormatVector(k.HipEuler)}, UniformScale = {Format(k.UniformScale)}f, " +
               $"AimEuler = {FormatVector(k.AimEuler)}, AimPositionOffset = {FormatVector(k.AimPositionOffset)}, " +
               $"EquippableLocalPosition = {FormatVector(k.EquippableLocalPosition)}, EquippableLocalEuler = {FormatVector(k.EquippableLocalEuler)}, " +
               $"ModelLocalPosition = {FormatVector(k.ModelLocalPosition)}, ModelLocalEuler = {FormatVector(k.ModelLocalEuler)}, ModelUniformScale = {Format(k.ModelUniformScale)}f, " +
               $"MapRaiseEquippablePosition = {FormatVector(k.MapRaiseEquippablePosition)}, MapRaiseEquippableEuler = {FormatVector(k.MapRaiseEquippableEuler)}, " +
               $"ThirdPersonPosition = {FormatVector(k.ThirdPersonPosition)}, ThirdPersonEuler = {FormatVector(k.ThirdPersonEuler)}, ThirdPersonWorldScale = {Format(k.ThirdPersonWorldScale)}f, " +
               $"LeftHandOffset = {FormatVector(k.LeftHandOffset)}, LeftHandEuler = {FormatVector(k.LeftHandEuler)}, " +
               $"RightHandOffset = {FormatVector(k.RightHandOffset)}, RightHandEuler = {FormatVector(k.RightHandEuler)} " +
               "}";
    }

    private static string FormatVector(Vector3 v) =>
        $"new Vector3({Format(v.x)}f, {Format(v.y)}f, {Format(v.z)}f)";

    private static string Format(float value) =>
        value.ToString("0.####", CultureInfo.InvariantCulture);
}
