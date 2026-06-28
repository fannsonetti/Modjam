using System.Collections.Generic;
using UnityEngine;

namespace MoreWeapons.Utils;

internal static class ViewmodelTimelineGui
{
    private const float KeyframeSize = 10f;
    private static bool _draggingPlayhead;
    private static int _draggingKeyframe = -1;

    internal static float Draw(
        Rect rect,
        float duration,
        float normalizedTime,
        IList<float> keyframeTimes,
        ref int selectedKeyframe)
    {
        var evt = Event.current;
        var trackRect = new Rect(rect.x, rect.y + 18f, rect.width, rect.height - 18f);

        DrawRuler(new Rect(rect.x, rect.y, rect.width, 16f), duration);
        GUI.Box(trackRect, string.Empty, ImGuiSkinHelper.BoxStyle);

        var playheadX = trackRect.x + normalizedTime * trackRect.width;
        DrawPlayhead(playheadX, trackRect.y, trackRect.height);

        for (var i = 0; i < keyframeTimes.Count; i++)
        {
            var keyX = trackRect.x + keyframeTimes[i] * trackRect.width;
            var keyRect = new Rect(keyX - KeyframeSize * 0.5f, trackRect.y + trackRect.height * 0.5f - KeyframeSize * 0.5f, KeyframeSize, KeyframeSize);
            var selected = selectedKeyframe == i;
            GUI.color = selected ? new Color(1f, 0.85f, 0.2f, 1f) : new Color(0.85f, 0.9f, 1f, 1f);
            GUI.DrawTexture(keyRect, GetDiamondTexture(), ScaleMode.StretchToFill);
            GUI.color = Color.white;

            if (evt.type == EventType.MouseDown && evt.button == 0 && keyRect.Contains(evt.mousePosition))
            {
                selectedKeyframe = i;
                _draggingKeyframe = i;
                normalizedTime = keyframeTimes[i];
                evt.Use();
            }
        }

        if (evt.type == EventType.MouseDown && evt.button == 0 && trackRect.Contains(evt.mousePosition))
        {
            normalizedTime = Mathf.Clamp01((evt.mousePosition.x - trackRect.x) / trackRect.width);
            _draggingPlayhead = true;
            selectedKeyframe = FindNearestKeyframe(normalizedTime, keyframeTimes);
            evt.Use();
        }

        if (evt.type == EventType.MouseDrag && _draggingPlayhead)
        {
            normalizedTime = Mathf.Clamp01((evt.mousePosition.x - trackRect.x) / trackRect.width);
            evt.Use();
        }

        if (evt.type == EventType.MouseDrag && _draggingKeyframe >= 0)
        {
            normalizedTime = Mathf.Clamp01((evt.mousePosition.x - trackRect.x) / trackRect.width);
            evt.Use();
        }

        if (evt.type == EventType.MouseUp)
        {
            _draggingPlayhead = false;
            _draggingKeyframe = -1;
        }

        var timeLabel = duration > 0f ? $"{normalizedTime * duration:0.###}s / {duration:0.###}s" : $"{normalizedTime:0.###}";
        GUI.Label(new Rect(rect.x, rect.yMax + 2f, rect.width, 18f), timeLabel, ImGuiSkinHelper.LabelStyle);

        return normalizedTime;
    }

    private static void DrawRuler(Rect rect, float duration)
    {
        GUI.Box(rect, string.Empty, ImGuiSkinHelper.BoxStyle);
        const int ticks = 10;
        for (var i = 0; i <= ticks; i++)
        {
            var t = i / (float)ticks;
            var x = rect.x + t * rect.width;
            GUI.DrawTexture(new Rect(x, rect.yMax - 6f, 1f, 6f), Texture2D.whiteTexture);
            var label = duration > 0f ? $"{duration * t:0.##}s" : t.ToString("0.##");
            GUI.Label(new Rect(x - 16f, rect.y, 32f, 14f), label, ImGuiSkinHelper.LabelStyle);
        }
    }

    private static void DrawPlayhead(float x, float y, float height)
    {
        GUI.color = new Color(1f, 0.35f, 0.25f, 0.95f);
        GUI.DrawTexture(new Rect(x - 1f, y, 2f, height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(x - 5f, y, 10f, 8f), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    private static int FindNearestKeyframe(float time, IList<float> keyframeTimes)
    {
        var best = -1;
        var bestDist = float.MaxValue;
        for (var i = 0; i < keyframeTimes.Count; i++)
        {
            var dist = Mathf.Abs(keyframeTimes[i] - time);
            if (dist >= bestDist)
                continue;
            bestDist = dist;
            best = i;
        }

        return bestDist <= 0.04f ? best : -1;
    }

    private static Texture2D _diamondTexture;

    private static Texture2D GetDiamondTexture()
    {
        if (_diamondTexture != null)
            return _diamondTexture;

        const int size = 16;
        _diamondTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
        };

        var center = size * 0.5f;
        var pixels = new Color[size * size];
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var dx = Mathf.Abs(x - center);
                var dy = Mathf.Abs(y - center);
                var inside = dx + dy <= center;
                pixels[y * size + x] = inside ? Color.white : Color.clear;
            }
        }

        _diamondTexture.SetPixels(pixels);
        _diamondTexture.Apply(false, true);
        return _diamondTexture;
    }
}
