using MelonLoader;
using S1API.Entities;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using MoreWeapons.Utils;

namespace MoreWeapons.NPCs;

public sealed class NukeStrikeContact : NPC
{
    public static NukeStrikeContact Instance { get; private set; }

    public override bool IsPhysical => false;

    protected override void ConfigurePrefab(NPCPrefabBuilder builder)
    {
        var icon = ModAssetLoader.LoadSprite(
            "MoreWeapons.Assets.Icons.unknown_contact.png",
            "Assets/Icons/unknown_contact.png");
        var prefab = builder.WithIdentity("wp_nuke_strike_contact", "Unknown", string.Empty);
        if (icon != null)
            prefab.WithIcon(icon);
    }

    protected override void OnCreated()
    {
        base.OnCreated();
        Instance = this;
        ConversationCanBeHidden = true;
        MelonCoroutines.Start(ApplyContactStyleOnce());
    }

    internal void SendStrikeEta(float etaSeconds)
    {
        var minutes = Mathf.Max(1, Mathf.CeilToInt(etaSeconds / 60f));
        var etaLine = minutes == 1
            ? "about a minute"
            : $"about {minutes} minutes";

        SendTextMessage(
            $"Strike's locked in. Touchdown {etaLine}. " +
            "Stay clear of the target zone until it's done.");
    }

    private IEnumerator ApplyContactStyleOnce()
    {
        yield return new WaitForSeconds(2f);
        try
        {
            ClearConversationCategoriesViaReflection();
            var icon = ModAssetLoader.LoadSprite(
                "MoreWeapons.Assets.Icons.unknown_contact.png",
                "Assets/Icons/unknown_contact.png");
            if (icon != null)
            {
                Icon = icon;
                RefreshMessagingIcons();
            }
        }
        catch (System.Exception ex)
        {
            MelonLogger.Warning($"MoreWeapons: NukeStrikeContact styling failed — {ex.Message}");
        }
    }

    private void ClearConversationCategoriesViaReflection()
    {
        var s1NpcField = typeof(NPC).GetField("S1NPC", BindingFlags.NonPublic | BindingFlags.Instance);
        var s1Npc = s1NpcField?.GetValue(this);
        if (s1Npc == null)
            return;

        var catsField = s1Npc.GetType().GetField("ConversationCategories", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (catsField?.GetValue(s1Npc) is IList categories)
            categories.Clear();

        var msgConvField = s1Npc.GetType().GetField("MSGConversation", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var msgConv = msgConvField?.GetValue(s1Npc);
        if (msgConv == null)
            return;

        var convType = msgConv.GetType();
        var convCats = convType.GetProperty("Categories", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(msgConv) as IList
            ?? convType.GetField("Categories", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(msgConv) as IList;
        convCats?.Clear();
    }
}
