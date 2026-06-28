using S1API.Money;

namespace MoreWeapons.Utils;

internal static class NukeStrikeBilling
{
    internal static float StrikeCost => Core.NukeStrikeCost;

    internal static float GetOnlineBalance()
    {
        try
        {
            return Money.GetOnlineBalance();
        }
        catch
        {
            return 0f;
        }
    }

    internal static bool CanAfford() => GetOnlineBalance() >= StrikeCost;

    internal static bool TryCharge(out string failureReason)
    {
        failureReason = string.Empty;
        var balance = GetOnlineBalance();
        if (balance < StrikeCost)
        {
            failureReason = $"Insufficient credit card balance (${balance:0} / ${StrikeCost:0}).";
            return false;
        }

        try
        {
            Money.CreateOnlineTransaction(
                "Tactical Nuke Strike",
                -StrikeCost,
                1f,
                "Remote detonation unit ordnance");
            return true;
        }
        catch (System.Exception ex)
        {
            failureReason = $"Payment failed: {ex.Message}";
            return false;
        }
    }
}
