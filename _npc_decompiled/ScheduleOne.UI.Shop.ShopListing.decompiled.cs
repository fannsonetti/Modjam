using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence;
using ScheduleOne.Variables;
using UnityEngine;

namespace ScheduleOne.UI.Shop;

[Serializable]
public class ShopListing
{
	[Serializable]
	public class CategoryInstance
	{
		public EShopCategory Category;
	}

	public enum ERestockRate
	{
		Daily,
		Weekly,
		Never
	}

	public string name;

	public StorableItemDefinition Item;

	[Header("Pricing")]
	[SerializeField]
	protected bool OverridePrice;

	[SerializeField]
	protected float OverriddenPrice = 10f;

	[Header("Stock")]
	public bool LimitedStock;

	public int DefaultStock = -1;

	public ERestockRate RestockRate;

	public bool TieStockToNumberVariable;

	public string StockVariableName = "";

	[Header("Purchase Tracking")]
	public bool TrackPurchases;

	public string PurchasedQuantityVariableName = "";

	[Header("Settings")]
	public bool EnforceMinimumGameCreationVersion;

	public float MinimumGameCreationVersion = 27f;

	public bool CanBeDelivered;

	[Header("Color")]
	public bool UseIconTint;

	public Color IconTint = Color.white;

	[Header("Visibility")]
	public bool ConditionalVisibility;

	public string ConditionalVisibilityVariableName = "";

	public Action onStockChanged;

	private NumberVariable stockVariable;

	private NumberVariable purchasedQuantityVariable;

	private BoolVariable conditionalVisibilityVariable;

	public bool IsInStock => true;

	public float Price
	{
		get
		{
			if (!OverridePrice)
			{
				return Item.BasePurchasePrice;
			}
			return OverriddenPrice;
		}
	}

	public bool IsUnlimitedStock => !LimitedStock;

	public ShopInterface Shop { get; private set; }

	public int CurrentStock { get; protected set; }

	public int QuantityInCart { get; private set; }

	public int CurrentStockMinusCart => CurrentStock - QuantityInCart;

	public void Initialize(ShopInterface shop)
	{
		Shop = shop;
		if (TieStockToNumberVariable)
		{
			stockVariable = NetworkSingleton<VariableDatabase>.Instance.GetVariable(StockVariableName) as NumberVariable;
			if (stockVariable != null)
			{
				stockVariable.OnValueChanged.AddListener(StockVariableChanged);
			}
			else
			{
				Console.LogError("Failed to find stock variable " + StockVariableName + " for shop listing " + name);
				TieStockToNumberVariable = false;
			}
		}
		if (TrackPurchases)
		{
			purchasedQuantityVariable = NetworkSingleton<VariableDatabase>.Instance.GetVariable(PurchasedQuantityVariableName) as NumberVariable;
			if (purchasedQuantityVariable == null)
			{
				Console.LogError("Failed to find purchased quantity variable " + PurchasedQuantityVariableName + " for shop listing " + name);
				TrackPurchases = false;
			}
		}
		if (ConditionalVisibility)
		{
			conditionalVisibilityVariable = NetworkSingleton<VariableDatabase>.Instance.GetVariable(ConditionalVisibilityVariableName) as BoolVariable;
			if (conditionalVisibilityVariable == null)
			{
				Console.LogError("Failed to find conditional visibility variable " + ConditionalVisibilityVariableName + " for shop listing " + name);
				ConditionalVisibility = false;
			}
		}
	}

	public void Restock(bool network)
	{
		SetStock(DefaultStock);
	}

	public void RemoveStock(int quantity)
	{
		if (TrackPurchases)
		{
			purchasedQuantityVariable.SetValue(purchasedQuantityVariable.Value + (float)quantity, replicate: true);
		}
		SetStock(CurrentStock - quantity);
	}

	public void SetStock(int quantity, bool network = true)
	{
		if (!IsUnlimitedStock)
		{
			if (network && NetworkSingleton<ShopManager>.InstanceExists && Shop != null)
			{
				NetworkSingleton<ShopManager>.Instance.SendStock(Shop.ShopCode, Item.ID, quantity);
			}
			if (TieStockToNumberVariable)
			{
				quantity = Mathf.RoundToInt(stockVariable.Value);
			}
			CurrentStock = quantity;
			if (CurrentStock < 0)
			{
				CurrentStock = 0;
			}
			if (onStockChanged != null)
			{
				onStockChanged();
			}
		}
	}

	public void PullStockFromVariable()
	{
		if (stockVariable != null)
		{
			CurrentStock = Mathf.RoundToInt(stockVariable.Value);
			if (onStockChanged != null)
			{
				onStockChanged();
			}
		}
		else
		{
			Console.LogError("Stock variable is null for shop listing " + name);
		}
	}

	private void StockVariableChanged(float newValue)
	{
		SetStock(Mathf.RoundToInt(stockVariable.Value), network: false);
	}

	public virtual bool ShouldShow()
	{
		if (EnforceMinimumGameCreationVersion && SaveManager.GetVersionNumber(Singleton<MetadataManager>.Instance.CreationVersion) < MinimumGameCreationVersion)
		{
			return false;
		}
		if (ConditionalVisibility && conditionalVisibilityVariable != null && !conditionalVisibilityVariable.Value)
		{
			return false;
		}
		return true;
	}

	public virtual bool DoesListingMatchCategoryFilter(EShopCategory category)
	{
		if (category != EShopCategory.All)
		{
			return Item.ShopCategories.Find((CategoryInstance x) => x.Category == category) != null;
		}
		return true;
	}

	public virtual bool DoesListingMatchSearchTerm(string searchTerm)
	{
		return Item.Name.ToLower().Contains(searchTerm.ToLower());
	}

	public void SetQuantityInCart(int quantity)
	{
		QuantityInCart = quantity;
		if (onStockChanged != null)
		{
			onStockChanged();
		}
	}
}
