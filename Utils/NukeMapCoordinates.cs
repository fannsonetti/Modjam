using UnityEngine;



namespace MoreWeapons.Utils;



internal static class NukeMapCoordinates

{

    internal const float MinWorldX = -193.5f;

    internal const float MaxWorldX = 214f;

    internal const float MaxWorldZ = 200f;

    internal const float MinWorldZ = -207f;



    internal static Vector3 ScreenPointToWorld(Vector2 guiPoint, Rect mapRect)

    {

        var u = Mathf.Clamp01((guiPoint.x - mapRect.xMin) / mapRect.width);

        var v = Mathf.Clamp01((guiPoint.y - mapRect.yMin) / mapRect.height);

        var x = Mathf.Lerp(MinWorldX, MaxWorldX, u);

        var z = Mathf.Lerp(MaxWorldZ, MinWorldZ, v);

        return new Vector3(x, 0f, z);

    }



    internal static Vector2 WorldPointToGuiPoint(Vector3 worldPoint, Rect mapRect)

    {

        var u = Mathf.InverseLerp(MinWorldX, MaxWorldX, worldPoint.x);

        var v = Mathf.InverseLerp(MaxWorldZ, MinWorldZ, worldPoint.z);

        return new Vector2(mapRect.xMin + u * mapRect.width, mapRect.yMin + v * mapRect.height);

    }

}

