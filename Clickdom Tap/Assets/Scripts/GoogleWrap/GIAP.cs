using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;

public class GIAP : IStoreListener
{
    public enum IAPConsumableId
    {
        giap_silver_pack,
        giap_double_silver_gain,
        giap_double_damage
    }

    private string[] consumablesIdsCache;
    private Dictionary<string, Action<bool, PurchaseEventArgs>> onConsumablePurcheseActions;

    private Action<bool> onInit;

    private IStoreController controller;
    private IExtensionProvider extensions;

    public bool IsInitiated => controller != null && extensions != null;

    private static GIAP instance;
    public static GIAP Instance
    {
        get
        {
            if (instance == null)
                instance = new GIAP();
            return instance;
        }
    }
    private GIAP() { }

    public void Init(Action<bool> onInit)
    {
        if (IsInitiated)
            return;

        this.onInit = onInit;
        onConsumablePurcheseActions = new Dictionary<string, Action<bool, PurchaseEventArgs>>();

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        consumablesIdsCache = Enum.GetNames(typeof(IAPConsumableId));
        foreach (var id in consumablesIdsCache)
            builder.AddProduct(id, ProductType.Consumable);
        UnityPurchasing.Initialize(this, builder);
    }

    public void BuyConsumable(IAPConsumableId id, Action<bool, PurchaseEventArgs> onSuccess)
    {
        BuyProduct(id.ToString(), onSuccess);
    }

    private void BuyProduct(string productId, Action<bool, PurchaseEventArgs> onPurchase)
    {
        if (!IsInitiated)
            return;

        var product = controller.products.WithID(productId);
        if (product != null && product.availableToPurchase)
        {
            controller.InitiatePurchase(product);
            if (!onConsumablePurcheseActions.ContainsKey(productId))
                onConsumablePurcheseActions.Add(productId, onPurchase);
            else
                onConsumablePurcheseActions[productId] = onPurchase;
        }
        else
        {
            OnPurchaseFailed(product, PurchaseFailureReason.ProductUnavailable);
        }
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log($"InitializeFailed: {error}");
        onInit?.Invoke(false);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        var id = e.purchasedProduct.definition.id;
        if (onConsumablePurcheseActions.ContainsKey(id))
        {
            var act = onConsumablePurcheseActions[id];
            act?.Invoke(true, e);
            onConsumablePurcheseActions.Remove(id);
        }
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
    {
        try
        {
            Debug.Log($"PurchaseFailed: {i} - {p}");
            if(i != null && onConsumablePurcheseActions.ContainsKey(i.definition.id))
            {
                var act = onConsumablePurcheseActions[i.definition.id];
                act?.Invoke(false, null);
                onConsumablePurcheseActions.Remove(i.definition.id);
            }
        }
        catch { }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        this.controller = controller;
        this.extensions = extensions;

        onInit?.Invoke(IsInitiated);
    }
}
