using System;
using UnityEngine;

namespace ScheduleOne.Levelling;

[Serializable]
public struct FullRank(ERank rank, int tier)
{
	public const int TIER_COUNT = 5;

	public ERank Rank = rank;

	[Range(1f, 5f)]
	public int Tier = tier;

	public override string ToString()
	{
		return GetString(this);
	}

	public FullRank NextRank()
	{
		if (Rank == ERank.Kingpin)
		{
			return new FullRank(ERank.Kingpin, Tier + 1);
		}
		if (Tier < 5)
		{
			return new FullRank(Rank, Tier + 1);
		}
		return new FullRank(Rank + 1, 1);
	}

	public float ToFloat()
	{
		return (float)Rank + (float)Tier / 5f;
	}

	public int GetRankIndex()
	{
		return (int)Rank * 5 + (Tier - 1);
	}

	public static string GetString(FullRank rank)
	{
		string text = rank.Rank.ToString();
		text = text.Replace("_", " ");
		return rank.Tier switch
		{
			1 => text + " I", 
			2 => text + " II", 
			3 => text + " III", 
			4 => text + " IV", 
			5 => text + " V", 
			_ => text + " " + rank.Tier, 
		};
	}

	public static bool operator >(FullRank a, FullRank b)
	{
		if (a.Rank > b.Rank)
		{
			return true;
		}
		if (a.Rank == b.Rank)
		{
			return a.Tier > b.Tier;
		}
		return false;
	}

	public static bool operator <(FullRank a, FullRank b)
	{
		if (a.Rank < b.Rank)
		{
			return true;
		}
		if (a.Rank == b.Rank)
		{
			return a.Tier < b.Tier;
		}
		return false;
	}

	public static bool operator <=(FullRank a, FullRank b)
	{
		if (!(a < b))
		{
			return a == b;
		}
		return true;
	}

	public static bool operator >=(FullRank a, FullRank b)
	{
		if (!(a > b))
		{
			return a == b;
		}
		return true;
	}

	public static bool operator ==(FullRank a, FullRank b)
	{
		if (a.Rank == b.Rank)
		{
			return a.Tier == b.Tier;
		}
		return false;
	}

	public static bool operator !=(FullRank a, FullRank b)
	{
		if (a.Rank == b.Rank)
		{
			return a.Tier != b.Tier;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj is FullRank)
		{
			return this == (FullRank)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public int CompareTo(FullRank other)
	{
		if (this > other)
		{
			return 1;
		}
		if (this < other)
		{
			return -1;
		}
		return 0;
	}
}
