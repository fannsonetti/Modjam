using System.Globalization;
using System.Text;
using UnityEngine;

namespace MoreWeapons.Utils;

internal struct ViewmodelProfile
{
    public string Label;
    public ViewmodelPresentationMode PresentationMode;

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
    public Vector3 LeftForeArmOffset;
    public Vector3 LeftForeArmEuler;
    public Vector3 RightForeArmOffset;
    public Vector3 RightForeArmEuler;

    internal bool IsFloating => PresentationMode == ViewmodelPresentationMode.Floating;

    internal static ViewmodelProfile DefaultPlaceholderBar => new()
    {
        Label = "PlaceholderBar",
        PresentationMode = ViewmodelPresentationMode.AvatarHands,
        HipEuler = new Vector3(0f, 90f, 0f),
        UniformScale = 1f,
    };

    internal static ViewmodelProfile SharedGlbRifle => new()
    {
        Label = "SharedGlbRifle",
        PresentationMode = ViewmodelPresentationMode.AvatarHands,
        HipPosition = new Vector3(0f, 0f, 0.12f),
        HipEuler = new Vector3(0f, 180f, 0f),
        UniformScale = 1f,
        AimEuler = new Vector3(-9f, 18f, 0f),
        AimPositionOffset = new Vector3(0f, 0f, 0.05f),
        ThirdPersonPosition = new Vector3(0f, 0f, 0.24f),
        ThirdPersonWorldScale = 0.9f,
    };

    internal static ViewmodelProfile CreateFloating(
        string label,
        float modelScale = HandsFreeViewmodelSettings.ModelUniformScale,
        bool includeMapRaise = false)
    {
        var profile = new ViewmodelProfile
        {
            Label = label,
            PresentationMode = ViewmodelPresentationMode.Floating,
            EquippableLocalPosition = HandsFreeViewmodelSettings.EquippableLocalPosition,
            EquippableLocalEuler = HandsFreeViewmodelSettings.EquippableLocalEulerAngles,
            ModelLocalPosition = HandsFreeViewmodelSettings.ModelLocalPosition,
            ModelLocalEuler = HandsFreeViewmodelSettings.ModelLocalEulerAngles,
            ModelUniformScale = modelScale,
            ThirdPersonPosition = new Vector3(0f, 0f, 0.06f),
            ThirdPersonWorldScale = 0.4f,
        };

        if (includeMapRaise)
        {
            profile.MapRaiseEquippablePosition = HandsFreeViewmodelSettings.EquippableMapLocalPosition;
            profile.MapRaiseEquippableEuler = HandsFreeViewmodelSettings.EquippableMapLocalEulerAngles;
        }

        return profile;
    }

    public Quaternion GetAimRotation(float aim)
    {
        var euler = HipEuler + AimEuler * aim;
        return Quaternion.Euler(euler);
    }

    public Vector3 GetAimPositionOffset(float aim) => AimPositionOffset * aim;

    internal void ApplyFloatingModelTransform(Transform modelTransform)
    {
        modelTransform.localPosition = ModelLocalPosition;
        modelTransform.localRotation = Quaternion.Euler(ModelLocalEuler);
        modelTransform.localScale = Vector3.one * ModelUniformScale;
    }

    internal void GetFloatingEquippablePose(float mapRaiseBlend, out Vector3 position, out Quaternion rotation)
    {
        position = Vector3.Lerp(EquippableLocalPosition, MapRaiseEquippablePosition, mapRaiseBlend);
        rotation = Quaternion.Slerp(
            Quaternion.Euler(EquippableLocalEuler),
            Quaternion.Euler(MapRaiseEquippableEuler),
            mapRaiseBlend);
    }

    internal ViewmodelProfile Clone() => new()
    {
        Label = Label,
        PresentationMode = PresentationMode,
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
        LeftForeArmOffset = LeftForeArmOffset,
        LeftForeArmEuler = LeftForeArmEuler,
        RightForeArmOffset = RightForeArmOffset,
        RightForeArmEuler = RightForeArmEuler,
    };

    internal string ToCSharpSnippet(string fieldName)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"internal static ViewmodelProfile {fieldName} => new()");
        sb.AppendLine("{");
        sb.AppendLine($"    Label = \"{Label}\",");
        sb.AppendLine($"    PresentationMode = ViewmodelPresentationMode.{PresentationMode},");

        if (IsFloating)
        {
            AppendVector(sb, "EquippableLocalPosition", EquippableLocalPosition);
            AppendVector(sb, "EquippableLocalEuler", EquippableLocalEuler);
            AppendVector(sb, "ModelLocalPosition", ModelLocalPosition);
            AppendVector(sb, "ModelLocalEuler", ModelLocalEuler);
            AppendFloat(sb, "ModelUniformScale", ModelUniformScale);
            if (MapRaiseEquippablePosition != Vector3.zero || MapRaiseEquippableEuler != Vector3.zero)
            {
                AppendVector(sb, "MapRaiseEquippablePosition", MapRaiseEquippablePosition);
                AppendVector(sb, "MapRaiseEquippableEuler", MapRaiseEquippableEuler);
            }
        }
        else
        {
            AppendVector(sb, "HipPosition", HipPosition);
            AppendVector(sb, "HipEuler", HipEuler);
            AppendFloat(sb, "UniformScale", UniformScale);
            AppendVector(sb, "AimEuler", AimEuler);
            AppendVector(sb, "AimPositionOffset", AimPositionOffset);
            if (LeftHandOffset != Vector3.zero) AppendVector(sb, "LeftHandOffset", LeftHandOffset);
            if (LeftHandEuler != Vector3.zero) AppendVector(sb, "LeftHandEuler", LeftHandEuler);
            if (RightHandOffset != Vector3.zero) AppendVector(sb, "RightHandOffset", RightHandOffset);
            if (RightHandEuler != Vector3.zero) AppendVector(sb, "RightHandEuler", RightHandEuler);
            if (LeftForeArmOffset != Vector3.zero) AppendVector(sb, "LeftForeArmOffset", LeftForeArmOffset);
            if (LeftForeArmEuler != Vector3.zero) AppendVector(sb, "LeftForeArmEuler", LeftForeArmEuler);
            if (RightForeArmOffset != Vector3.zero) AppendVector(sb, "RightForeArmOffset", RightForeArmOffset);
            if (RightForeArmEuler != Vector3.zero) AppendVector(sb, "RightForeArmEuler", RightForeArmEuler);
        }

        AppendVector(sb, "ThirdPersonPosition", ThirdPersonPosition);
        if (ThirdPersonEuler != Vector3.zero) AppendVector(sb, "ThirdPersonEuler", ThirdPersonEuler);
        AppendFloat(sb, "ThirdPersonWorldScale", ThirdPersonWorldScale);
        sb.AppendLine("};");
        return sb.ToString();
    }

    private static void AppendVector(StringBuilder sb, string name, Vector3 value)
    {
        sb.AppendLine($"    {name} = new Vector3({Format(value.x)}f, {Format(value.y)}f, {Format(value.z)}f),");
    }

    private static void AppendFloat(StringBuilder sb, string name, float value)
    {
        sb.AppendLine($"    {name} = {Format(value)}f,");
    }

    private static string Format(float value) =>
        value.ToString("0.####", CultureInfo.InvariantCulture);
}
