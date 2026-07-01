using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace MoreWeapons.Utils;

internal static class ViewmodelAnimatorBuilder
{
    internal static AnimationClip BuildOverrideClip(AnimationClip sourceClip, ViewmodelAnimClip animClip, GameObject sampleRoot)
    {
        if (sourceClip == null || animClip == null || sampleRoot == null || animClip.Keyframes.Count == 0)
            return null;

        var duration = animClip.Duration > 0f ? animClip.Duration : sourceClip.length;
        if (duration <= 0f)
            duration = 0.5f;

        var clip = new AnimationClip
        {
            name = sourceClip.name + "_MW",
            legacy = false,
            frameRate = 60f,
        };

        var keyframes = new List<ViewmodelAnimKeyframe>(animClip.Keyframes);
        keyframes.Sort((a, b) => a.Time.CompareTo(b.Time));

        var savedPosition = sampleRoot.transform.localPosition;
        var savedRotation = sampleRoot.transform.localRotation;
        var curveBuilder = new CurveBuilder();

        foreach (var keyframe in keyframes)
        {
            var time = Mathf.Clamp(keyframe.Time, 0f, 1f) * duration;
            sourceClip.SampleAnimation(sampleRoot, time);
            ViewmodelAvatarBoneHelper.ApplyKeyframeOffsets(keyframe);
            RecordHandForearmPose(curveBuilder, sampleRoot.transform, time);
        }

        sampleRoot.transform.SetLocalPositionAndRotation(savedPosition, savedRotation);
        curveBuilder.ApplyTo(clip);
        return clip;
    }

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

    private static void RecordHandForearmPose(CurveBuilder curveBuilder, Transform sampleRoot, float time)
    {
        RecordBoneCurves(curveBuilder, sampleRoot, time, "mixamorig:LeftHand");
        RecordBoneCurves(curveBuilder, sampleRoot, time, "mixamorig:RightHand");
        RecordBoneCurves(curveBuilder, sampleRoot, time, "mixamorig:LeftForeArm");
        RecordBoneCurves(curveBuilder, sampleRoot, time, "mixamorig:RightForeArm");
    }

    private static void RecordBoneCurves(CurveBuilder curveBuilder, Transform sampleRoot, float time, string boneName)
    {
        var bone = FindBoneTransform(sampleRoot, boneName);
        if (bone == null)
            return;

        var path = GetRelativePath(bone, sampleRoot);
        curveBuilder.AddKey(path, "localPosition.x", time, bone.localPosition.x);
        curveBuilder.AddKey(path, "localPosition.y", time, bone.localPosition.y);
        curveBuilder.AddKey(path, "localPosition.z", time, bone.localPosition.z);
        curveBuilder.AddKey(path, "localEulerAngles.x", time, bone.localEulerAngles.x);
        curveBuilder.AddKey(path, "localEulerAngles.y", time, bone.localEulerAngles.y);
        curveBuilder.AddKey(path, "localEulerAngles.z", time, bone.localEulerAngles.z);
    }

    private static Transform FindBoneTransform(Transform root, string exactName)
    {
        var transforms = root.GetComponentsInChildren<Transform>(true);
        foreach (var transform in transforms)
        {
            if (transform.name == exactName)
                return transform;
        }

        return null;
    }

    private static string GetRelativePath(Transform bone, Transform root)
    {
        if (bone == null || root == null || bone == root)
            return string.Empty;

        var segments = new Stack<string>();
        var current = bone;
        while (current != null && current != root)
        {
            segments.Push(current.name);
            current = current.parent;
        }

        return string.Join("/", segments);
    }

    private sealed class CurveBuilder
    {
        private readonly Dictionary<string, AnimationCurve> _curves = new();

        internal void AddKey(string path, string property, float time, float value)
        {
            var key = path + "\0" + property;
            if (!_curves.TryGetValue(key, out var curve))
            {
                curve = new AnimationCurve();
                _curves[key] = curve;
            }

            curve.AddKey(time, value);
        }

        internal void ApplyTo(AnimationClip clip)
        {
            foreach (var pair in _curves)
            {
                var separator = pair.Key.IndexOf('\0');
                var path = pair.Key.Substring(0, separator);
                var property = pair.Key.Substring(separator + 1);
                clip.SetCurve(path, typeof(Transform), property, pair.Value);
            }
        }
    }
}
