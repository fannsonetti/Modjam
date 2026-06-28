using System.Reflection;
using UnityEngine;

namespace MoreWeapons.Utils;

internal static class PlayerControlHelper
{
    internal static bool GetCanMove(object movement)
    {
        if (movement == null)
            return true;

        var type = movement.GetType();
        var field = type.GetField("canMove", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null && field.FieldType == typeof(bool))
            return (bool)field.GetValue(movement);

        var property = type.GetProperty("CanMove", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (property != null && property.PropertyType == typeof(bool) && property.CanRead)
            return (bool)property.GetValue(movement);

        return true;
    }

    internal static void SetCanMove(object movement, bool canMove)
    {
        if (movement == null)
            return;

        var type = movement.GetType();
        var field = type.GetField("canMove", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null && field.FieldType == typeof(bool))
        {
            field.SetValue(movement, canMove);
            return;
        }

        var property = type.GetProperty("CanMove", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (property != null && property.PropertyType == typeof(bool) && property.CanWrite)
            property.SetValue(movement, canMove);
    }

    internal static bool GetCanLook(object camera)
    {
        if (camera == null)
            return true;

        var type = camera.GetType();
        var property = type.GetProperty("CanLook", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (property != null && property.PropertyType == typeof(bool) && property.CanRead)
            return (bool)property.GetValue(camera);

        var field = type.GetField("canLook", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null && field.FieldType == typeof(bool))
            return (bool)field.GetValue(camera);

        return true;
    }

    internal static void SetCanLook(object camera, bool canLook)
    {
        if (camera == null)
            return;

        var type = camera.GetType();
        var method = type.GetMethod("SetCanLook", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(bool) }, null);
        if (method != null)
        {
            method.Invoke(camera, new object[] { canLook });
            return;
        }

        var property = type.GetProperty("CanLook", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (property != null && property.PropertyType == typeof(bool) && property.CanWrite)
        {
            property.SetValue(camera, canLook);
            return;
        }

        var field = type.GetField("canLook", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null && field.FieldType == typeof(bool))
            field.SetValue(camera, canLook);
    }

    internal static void FreeMouse(object camera)
    {
        InvokeCameraMethod(camera, "FreeMouse");
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    internal static void LockMouse(object camera)
    {
        InvokeCameraMethod(camera, "LockMouse");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private static void InvokeCameraMethod(object camera, string methodName)
    {
        if (camera == null)
            return;

        camera.GetType()
            .GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            ?.Invoke(camera, null);
    }
}
