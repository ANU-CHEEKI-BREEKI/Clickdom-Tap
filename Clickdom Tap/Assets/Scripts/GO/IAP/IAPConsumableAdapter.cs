using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class IAPConsumableAdapter : MonoBehaviour
{
    [SerializeField] private GIAP.IAPConsumableId id;
    [Space]
    [SerializeField] private UnityEvent onPurchaseSuccess;
    [SerializeField] private UnityEvent onPurchaseFailed;

    public void BuyProduct()
    {
        GIAP.Instance.BuyConsumable(id, (success, args) =>
        {
            if (success)
                onPurchaseSuccess?.Invoke();
            else
                onPurchaseFailed?.Invoke();
        });
    }
}
