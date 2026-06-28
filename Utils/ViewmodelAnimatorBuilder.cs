using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace MoreWeapons.Utils;

internal static class ViewmodelAnimatorBuilder
{
    internal static AnimationClip CreateClipFromKeyframes(string clipName, float duration, IList<ViewmodelAnimKeyframe> keyframes)
    {
        var clip = new AnimationClip { name = clipName, legacy = false };
        clip.frameRate = 60f;

        if (keyframes == null || keyframes.Count == 0)
            return clip;

        var sorted = new List<ViewmodelAnimKeyframe>(keyframes);
        sorted.Sort((a, b) => a.Time.CompareTo(b.Time));

        AddVector3Curves(clip, "mixamorig:LeftHand", "localPosition", sorted, duration, k => k.LeftHandOffset, k => k.LeftHandOffset, k => k.LeftHandOffset);
        AddVector3Curves(clip, "mixamorig:LeftHand", "localEulerAngles", sorted, duration, k => k.LeftHandEuler, k => k.LeftHandEuler, k => k.LeftHandEuler);
        AddVector3Curves(clip, "mixamorig:RightHand", "localPosition", sorted, duration, k => k.RightHandOffset, k => k.RightHandOffset, k => k.RightHandOffset);
        AddVector3Curves(clip, "mixamorig:RightHand", "localEulerAngles", sorted, duration, k => k.RightHandEuler, k => k.RightHandEuler, k => k.RightHandEuler);
        AddVector3Curves(clip, "mixamorig:LeftForeArm", "localPosition", sorted, duration, k => k.LeftForeArmOffset, k => k.LeftForeArmOffset, k => k.LeftForeArmOffset);
        AddVector3Curves(clip, "mixamorig:LeftForeArm", "localEulerAngles", sorted, duration, k => k.LeftForeArmEuler, k => k.LeftForeArmEuler, k => k.LeftForeArmEuler);
        AddVector3Curves(clip, "mixamorig:RightForeArm", "localPosition", sorted, duration, k => k.RightForeArmOffset, k => k.RightForeArmOffset, k => k.RightForeArmOffset);
        AddVector3Curves(clip, "mixamorig:RightForeArm", "localEulerAngles", sorted, duration, k => k.RightForeArmEuler, k => k.RightForeArmEuler, k => k.RightForeArmEuler);

        return clip;
    }

    internal static string ToCurveExportSnippet(string controllerName, ViewmodelAnimClip clip)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"// Duplicate template controller as {controllerName}, then bake these keyframes into clips.");
        sb.AppendLine($"// Clip: {clip.Name}, Duration: {clip.Duration:0.###}s");
        foreach (var keyframe in clip.Keyframes)
        {
            sb.AppendLine($"// t={keyframe.Time:0.###} LeftHand={Format(keyframe.LeftHandOffset)} euler={Format(keyframe.LeftHandEuler)}");
            sb.AppendLine($"// t={keyframe.Time:0.###} RightHand={Format(keyframe.RightHandOffset)} euler={Format(keyframe.RightHandEuler)}");
            sb.AppendLine($"// t={keyframe.Time:0.###} LeftForeArm={Format(keyframe.LeftForeArmOffset)} euler={Format(keyframe.LeftForeArmEuler)}");
            sb.AppendLine($"// t={keyframe.Time:0.###} RightForeArm={Format(keyframe.RightForeArmOffset)} euler={Format(keyframe.RightForeArmEuler)}");
        }

        return sb.ToString();
    }

    private static void AddVector3Curves(
        AnimationClip clip,
        string path,
        string propertyPrefix,
        List<ViewmodelAnimKeyframe> keyframes,
        float duration,
        System.Func<ViewmodelAnimKeyframe, Vector3> selector,
        System.Func<ViewmodelAnimKeyframe, Vector3> _,
        System.Func<ViewmodelAnimKeyframe, Vector3> __)
    {
        AddCurve(clip, path, propertyPrefix + ".x", keyframes, duration, k => selector(k).x);
        AddCurve(clip, path, propertyPrefix + ".y", keyframes, duration, k => selector(k).y);
        AddCurve(clip, path, propertyPrefix + ".z", keyframes, duration, k => selector(k).z);
    }

    private static void AddCurve(
        AnimationClip clip,
        string path,
        string property,
        List<ViewmodelAnimKeyframe> keyframes,
        float duration,
        System.Func<ViewmodelAnimKeyframe, float> selector)
    {
        var curve = new AnimationCurve();
        foreach (var keyframe in keyframes)
        {
            var time = keyframe.Time * duration;
            curve.AddKey(time, selector(keyframe));
        }

        clip.SetCurve(path, typeof(Transform), property, curve);
    }

    private static string Format(Vector3 v) =>
        $"({v.x.ToString("0.####", CultureInfo.InvariantCulture)}, {v.y.ToString("0.####", CultureInfo.InvariantCulture)}, {v.z.ToString("0.####", CultureInfo.InvariantCulture)})";
}
