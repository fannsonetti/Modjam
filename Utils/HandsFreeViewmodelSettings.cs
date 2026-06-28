using UnityEngine;

namespace MoreWeapons.Utils;

/// <summary>
/// First-person pose shared with the Thor Hammer mod — no avatar hands, model floats in front of the camera.
/// </summary>
internal static class HandsFreeViewmodelSettings
{
    internal static readonly Vector3 EquippableLocalPosition = new(0.28f, -0.11f, 0.36f);
    internal static readonly Vector3 EquippableLocalEulerAngles = Vector3.zero;

    /// <summary>Raised pose while the nuke map overlay is open — device held up toward the screen.</summary>
    internal static readonly Vector3 EquippableMapLocalPosition = new(0.06f, 0.08f, 0.72f);
    internal static readonly Vector3 EquippableMapLocalEulerAngles = new(350f, 230f, 0f);

    internal static readonly Vector3 ModelLocalPosition = new(0.02f, -0.065f, 0.05f);
    internal static readonly Vector3 ModelLocalEulerAngles = new(0f, 90f, 0f);
    internal const float ModelUniformScale = 1.5f;

    internal const float MapPoseLerpSpeed = 5f;
}
