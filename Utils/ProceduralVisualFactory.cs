using UnityEngine;
using UnityEngine.Rendering;
#if IL2CPP
using Il2CppScheduleOne.DevUtilities;
#else
using ScheduleOne.DevUtilities;
#endif

namespace MoreWeapons.Utils;

internal static class ProceduralVisualFactory
{
    private static Mesh _ellipsoidMesh;
    private static Mesh _wingMesh;
    private static Mesh _finMesh;

    internal static GameObject CreateEllipsoid(
        string name,
        Transform parent,
        Vector3 localPosition,
        Quaternion localRotation,
        Vector3 localScale,
        Color color,
        int layer)
    {
        return CreateMeshObject(name, parent, localPosition, localRotation, localScale, GetEllipsoidMesh(), color, layer);
    }

    internal static GameObject CreateWing(
        string name,
        Transform parent,
        Vector3 localPosition,
        Quaternion localRotation,
        Vector3 localScale,
        Color color,
        int layer)
    {
        return CreateMeshObject(name, parent, localPosition, localRotation, localScale, GetWingMesh(), color, layer);
    }

    internal static GameObject CreateTailFin(
        string name,
        Transform parent,
        Vector3 localPosition,
        Quaternion localRotation,
        Vector3 localScale,
        Color color,
        int layer)
    {
        return CreateMeshObject(name, parent, localPosition, localRotation, localScale, GetFinMesh(), color, layer);
    }

    private static GameObject CreateMeshObject(
        string name,
        Transform parent,
        Vector3 localPosition,
        Quaternion localRotation,
        Vector3 localScale,
        Mesh mesh,
        Color color,
        int layer)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = localRotation;
        go.transform.localScale = localScale;

        var filter = go.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        var renderer = go.AddComponent<MeshRenderer>();
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Unlit/Color");
        renderer.material = CreateDoubleSidedMaterial(shader, color);
        renderer.shadowCastingMode = ShadowCastingMode.Off;

        LayerUtility.SetLayerRecursively(go, layer);
        return go;
    }

    private static Mesh GetEllipsoidMesh()
    {
        if (_ellipsoidMesh != null)
            return _ellipsoidMesh;

        const int longitudeSegments = 20;
        const int latitudeSegments = 10;
        var vertices = new Vector3[(latitudeSegments + 1) * (longitudeSegments + 1)];
        var triangles = new int[latitudeSegments * longitudeSegments * 6];

        var vertex = 0;
        for (var lat = 0; lat <= latitudeSegments; lat++)
        {
            var v = lat / (float)latitudeSegments;
            var phi = Mathf.PI * v;
            var y = Mathf.Cos(phi) * 0.5f;
            var ringRadius = Mathf.Sin(phi) * 0.5f;

            for (var lon = 0; lon <= longitudeSegments; lon++)
            {
                var u = lon / (float)longitudeSegments;
                var theta = Mathf.PI * 2f * u;
                vertices[vertex++] = new Vector3(Mathf.Cos(theta) * ringRadius, y, Mathf.Sin(theta) * ringRadius);
            }
        }

        var tri = 0;
        for (var lat = 0; lat < latitudeSegments; lat++)
        {
            for (var lon = 0; lon < longitudeSegments; lon++)
            {
                var current = lat * (longitudeSegments + 1) + lon;
                var next = current + longitudeSegments + 1;
                triangles[tri++] = current;
                triangles[tri++] = current + 1;
                triangles[tri++] = next;
                triangles[tri++] = current + 1;
                triangles[tri++] = next + 1;
                triangles[tri++] = next;
            }
        }

        _ellipsoidMesh = new Mesh
        {
            name = "MoreWeapons_ProceduralEllipsoid",
            vertices = vertices,
            triangles = triangles
        };
        _ellipsoidMesh.RecalculateNormals();
        _ellipsoidMesh.RecalculateBounds();
        return _ellipsoidMesh;
    }

    private static Mesh GetWingMesh()
    {
        if (_wingMesh != null)
            return _wingMesh;

        var vertices = new[]
        {
            new Vector3(-0.45f, 0.04f, -0.5f),
            new Vector3(0.35f, 0.04f, -0.5f),
            new Vector3(0.2f, 0.04f, 0.5f),
            new Vector3(-0.22f, 0.04f, 0.5f),
            new Vector3(-0.45f, -0.04f, -0.5f),
            new Vector3(0.35f, -0.04f, -0.5f),
            new Vector3(0.2f, -0.04f, 0.5f),
            new Vector3(-0.22f, -0.04f, 0.5f),
        };
        var triangles = new[]
        {
            0, 1, 2, 0, 2, 3,
            5, 4, 7, 5, 7, 6,
            4, 0, 3, 4, 3, 7,
            1, 5, 6, 1, 6, 2,
            3, 2, 6, 3, 6, 7,
            4, 5, 1, 4, 1, 0
        };

        _wingMesh = new Mesh
        {
            name = "MoreWeapons_ProceduralTaperedWing",
            vertices = vertices,
            triangles = triangles
        };
        _wingMesh.RecalculateNormals();
        _wingMesh.RecalculateBounds();
        return _wingMesh;
    }

    private static Mesh GetFinMesh()
    {
        if (_finMesh != null)
            return _finMesh;

        var vertices = new[]
        {
            new Vector3(-0.5f, -0.04f, -0.25f),
            new Vector3(0.5f, -0.04f, -0.25f),
            new Vector3(0.1f, -0.04f, 0.45f),
            new Vector3(-0.5f, 0.04f, -0.25f),
            new Vector3(0.5f, 0.04f, -0.25f),
            new Vector3(0.1f, 0.04f, 0.45f),
        };
        var triangles = new[]
        {
            0, 1, 2,
            4, 3, 5,
            3, 0, 2, 3, 2, 5,
            1, 4, 5, 1, 5, 2,
            3, 4, 1, 3, 1, 0
        };

        _finMesh = new Mesh
        {
            name = "MoreWeapons_ProceduralTailFin",
            vertices = vertices,
            triangles = triangles
        };
        _finMesh.RecalculateNormals();
        _finMesh.RecalculateBounds();
        return _finMesh;
    }

    private static Material CreateDoubleSidedMaterial(Shader shader, Color color)
    {
        var material = new Material(shader) { color = color };
        MeshRenderHelper.ConfigureDoubleSided(material);
        return material;
    }
}
