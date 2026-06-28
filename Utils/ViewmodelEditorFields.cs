using UnityEngine;

namespace MoreWeapons.Utils;

internal static class ViewmodelEditorFields
{
    private const float LabelWidth = 140f;
    private const float ValueWidth = 72f;
    private const float SliderWidth = 380f;

    internal static float FloatField(string label, float value, float width = ValueWidth)
    {
        GUILayout.BeginHorizontal();
        if (!string.IsNullOrEmpty(label))
            GUILayout.Label(label, ImGuiSkinHelper.LabelStyle, GUILayout.Width(LabelWidth));
        var text = GUILayout.TextField(value.ToString("0.####"), ImGuiSkinHelper.TextFieldStyle, GUILayout.Width(width));
        if (float.TryParse(text, out var parsed) && !Mathf.Approximately(parsed, value))
            value = parsed;
        GUILayout.EndHorizontal();
        return value;
    }

    internal static float Slider(string label, float value, float min, float max)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, ImGuiSkinHelper.LabelStyle, GUILayout.Width(LabelWidth));
        value = GUILayout.HorizontalSlider(
            value,
            min,
            max,
            ImGuiSkinHelper.HorizontalSliderStyle,
            ImGuiSkinHelper.HorizontalSliderThumbStyle,
            GUILayout.Width(SliderWidth));
        var text = GUILayout.TextField(value.ToString("0.####"), ImGuiSkinHelper.TextFieldStyle, GUILayout.Width(ValueWidth));
        if (float.TryParse(text, out var parsed) && !Mathf.Approximately(parsed, value))
            value = parsed;
        GUILayout.EndHorizontal();
        return value;
    }

    internal static Vector3 Vector3Field(string label, Vector3 value, float min = -1f, float max = 1f, bool useSliders = true)
    {
        GUILayout.Label(label, ImGuiSkinHelper.LabelStyle);
        if (useSliders)
        {
            value.x = Slider("  X", value.x, min, max);
            value.y = Slider("  Y", value.y, min, max);
            value.z = Slider("  Z", value.z, min, max);
        }
        else
        {
            value.x = FloatField("  X", value.x);
            value.y = FloatField("  Y", value.y);
            value.z = FloatField("  Z", value.z);
        }

        return value;
    }
}
