using UnityEngine;

namespace MoreWeapons.Utils;

internal static class ImGuiSkinHelper
{
    private static bool _initialized;
    private static GUIStyle _panelStyle;
    private static GUIStyle _labelStyle;
    private static GUIStyle _buttonStyle;
    private static GUIStyle _textFieldStyle;
    private static GUIStyle _boxStyle;
    private static GUIStyle _horizontalSliderStyle;
    private static GUIStyle _horizontalSliderThumbStyle;
    private static Texture2D _panelTexture;
    private static Texture2D _buttonTexture;
    private static Texture2D _overlayTexture;

    internal static void EnsureInitialized()
    {
        if (_initialized)
            return;

        _panelTexture = MakeTexture(new Color(0.08f, 0.09f, 0.11f, 0.96f));
        _buttonTexture = MakeTexture(new Color(0.18f, 0.2f, 0.24f, 1f));
        _overlayTexture = MakeTexture(new Color(0f, 0f, 0f, 0.45f));

        _panelStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = _panelTexture, textColor = Color.white },
            onNormal = { background = _panelTexture, textColor = Color.white },
            padding = new RectOffset(10, 10, 10, 10),
        };

        _boxStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = MakeTexture(new Color(0.12f, 0.13f, 0.16f, 1f)), textColor = Color.white },
            padding = new RectOffset(6, 6, 6, 6),
        };

        _labelStyle = new GUIStyle(GUI.skin.label)
        {
            normal = { textColor = Color.white },
            fontSize = 12,
        };

        _buttonStyle = new GUIStyle(GUI.skin.button)
        {
            normal = { background = _buttonTexture, textColor = Color.white },
            hover = { background = MakeTexture(new Color(0.24f, 0.27f, 0.32f, 1f)), textColor = Color.white },
            active = { background = MakeTexture(new Color(0.14f, 0.16f, 0.2f, 1f)), textColor = Color.white },
            padding = new RectOffset(8, 8, 4, 4),
        };

        _textFieldStyle = new GUIStyle(GUI.skin.textField)
        {
            normal = { background = MakeTexture(new Color(0.05f, 0.05f, 0.07f, 1f)), textColor = Color.white },
            focused = { background = MakeTexture(new Color(0.07f, 0.08f, 0.1f, 1f)), textColor = Color.white },
            padding = new RectOffset(4, 4, 3, 3),
        };

        _horizontalSliderStyle = new GUIStyle(GUI.skin.horizontalSlider)
        {
            normal = { background = MakeTexture(new Color(0.2f, 0.22f, 0.26f, 1f)) },
        };

        _horizontalSliderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb)
        {
            normal = { background = MakeTexture(new Color(0.75f, 0.78f, 0.85f, 1f)) },
        };

        _initialized = true;
    }

    internal static void DrawOverlay()
    {
        EnsureInitialized();
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _overlayTexture, ScaleMode.StretchToFill);
    }

    internal static GUIStyle PanelStyle
    {
        get
        {
            EnsureInitialized();
            return _panelStyle;
        }
    }

    internal static GUIStyle LabelStyle
    {
        get
        {
            EnsureInitialized();
            return _labelStyle;
        }
    }

    internal static GUIStyle ButtonStyle
    {
        get
        {
            EnsureInitialized();
            return _buttonStyle;
        }
    }

    internal static GUIStyle TextFieldStyle
    {
        get
        {
            EnsureInitialized();
            return _textFieldStyle;
        }
    }

    internal static GUIStyle BoxStyle
    {
        get
        {
            EnsureInitialized();
            return _boxStyle;
        }
    }

    internal static GUIStyle HorizontalSliderStyle
    {
        get
        {
            EnsureInitialized();
            return _horizontalSliderStyle;
        }
    }

    internal static GUIStyle HorizontalSliderThumbStyle
    {
        get
        {
            EnsureInitialized();
            return _horizontalSliderThumbStyle;
        }
    }

    private static Texture2D MakeTexture(Color color)
    {
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point,
        };
        texture.SetPixel(0, 0, color);
        texture.Apply(false, true);
        return texture;
    }
}
