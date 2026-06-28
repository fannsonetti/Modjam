using UnityEngine;

namespace MoreWeapons.Utils;

public sealed class ViewmodelEditorHost : MonoBehaviour
{
#if IL2CPP
    public ViewmodelEditorHost(System.IntPtr ptr) : base(ptr) { }
#endif

    private static ViewmodelEditorHost _instance;

    internal static void Ensure()
    {
        if (_instance != null)
            return;

        var go = new GameObject("MoreWeaponsViewmodelEditorHost");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<ViewmodelEditorHost>();
    }

    private void OnGUI()
    {
        ViewmodelEditor.DrawGui();
    }

    private void LateUpdate()
    {
        ViewmodelEditor.LateUpdatePreview();
    }
}
