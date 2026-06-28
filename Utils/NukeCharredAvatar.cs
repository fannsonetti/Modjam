#if IL2CPP

using Il2CppScheduleOne.AvatarFramework;

using Il2CppScheduleOne.NPCs;

using GameAvatar = Il2CppScheduleOne.AvatarFramework.Avatar;

#else

using ScheduleOne.AvatarFramework;

using ScheduleOne.NPCs;

using GameAvatar = ScheduleOne.AvatarFramework.Avatar;

#endif



using System.Collections;

using System.Collections.Generic;

using MelonLoader;

using UnityEngine;



namespace MoreWeapons.Utils;



internal static class NukeCharredAvatar

{

    private static readonly Color Charred = Color.black;

    private const int NpcsPerFrame = 6;



    internal static void ApplyInBlastZone(Vector3 origin, float damageRadius)

    {

        var avatars = CollectAvatars(origin, damageRadius);

        if (avatars.Count == 0)

            return;



        MelonCoroutines.Start(ApplyAvatarsRoutine(avatars));

    }



    private static List<GameAvatar> CollectAvatars(Vector3 origin, float damageRadius)

    {

        var avatars = new List<GameAvatar>(32);

        var radiusSq = damageRadius * damageRadius;



        foreach (var npc in NPCManager.NPCRegistry)

        {

            if (npc?.Avatar == null)

                continue;



            var target = GetTargetPoint(npc);

            if ((target - origin).sqrMagnitude > radiusSq)

                continue;



            avatars.Add(npc.Avatar);

        }



        return avatars;

    }



    private static IEnumerator ApplyAvatarsRoutine(List<GameAvatar> avatars)

    {

        for (var i = 0; i < avatars.Count; i++)

        {

            Apply(avatars[i]);

            if ((i + 1) % NpcsPerFrame == 0)

                yield return null;

        }

    }



    internal static void Apply(GameAvatar avatar)

    {

        if (avatar == null)

            return;



        avatar.SetSkinColor(Charred);

        avatar.OverrideHairColor(Charred);

        CharFace(avatar);

        CharBodyLayers(avatar);

        CharAccessories(avatar);

    }



    private static void CharFace(GameAvatar avatar)

    {

        if (avatar.FaceMesh == null)

            return;



        var mat = avatar.FaceMesh.material;

        var faceTex = mat.GetTexture("_Layer_1_Texture") as Texture2D;

        avatar.SetFaceTexture(faceTex, Charred);

    }



    private static void CharBodyLayers(GameAvatar avatar)

    {

        var meshes = avatar.BodyMeshes;

        if (meshes == null)

            return;



        foreach (var mesh in meshes)

        {

            if (mesh == null)

                continue;



            var mat = mesh.material;

            if (mat.HasProperty("_SkinColor"))

                mat.SetColor("_SkinColor", Charred);



            for (var layer = 1; layer <= 6; layer++)

            {

                var texProperty = "_Layer_" + layer + "_Texture";

                var colorProperty = "_Layer_" + layer + "_Color";

                if (!mat.HasProperty(texProperty) || !mat.HasProperty(colorProperty))

                    continue;



                if (mat.GetTexture(texProperty) == null)

                    continue;



                mat.SetColor(colorProperty, Charred);

            }

        }

    }



    private static void CharAccessories(GameAvatar avatar)

    {

        if (avatar.BodyContainer == null)

            return;



        foreach (var accessory in avatar.BodyContainer.GetComponentsInChildren<Accessory>(true))

            accessory.ApplyColor(Charred);

    }



    private static Vector3 GetTargetPoint(NPC npc)

    {

        if (npc.Avatar != null)

            return npc.Avatar.transform.position + Vector3.up * 0.9f;



        return npc.transform.position + Vector3.up * 1f;

    }

}


