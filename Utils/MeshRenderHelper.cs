using UnityEngine;
using UnityEngine.Rendering;

namespace MoreWeapons.Utils;

internal static class MeshRenderHelper
{
    internal static void ConfigureDoubleSided(Material material)
    {
        if (material == null)
            return;

        material.SetInt("_Cull", (int)CullMode.Off);
        if (material.HasProperty("_CullMode"))
            material.SetInt("_CullMode", (int)CullMode.Off);
        if (material.HasProperty("_DoubleSidedEnable"))
            material.SetFloat("_DoubleSidedEnable", 1f);
    }

    internal static void ConfigureDoubleSidedRenderers(GameObject root)
    {
        if (root == null)
            return;

        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            var materials = renderer.materials;
            for (var i = 0; i < materials.Length; i++)
                ConfigureDoubleSided(materials[i]);
            renderer.materials = materials;
        }
    }
}
