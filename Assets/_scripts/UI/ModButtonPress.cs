using Assets._scripts.Mods;
using System.Collections.Generic;
using UnityEngine;


public enum ClickableModname
{
    None,
    OrderRerollNormal,
    VIPReroll,
    FireSale
}

public class ModButtonPress : MonoBehaviour
{
    [SerializeField]
    private ClickableModname buttonModKey;
    private void Start()
    {
        ModManager.Instance.OnModAdded += OnModPurchased;
        ModManager.Instance.OnClickableModExpired += OnModExpired;

        ModManager.Instance.ResetClickableModsAction += disableButton;

    }

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void OnModPurchased(Mod selectedmod)
    {
        if (selectedmod.GetType() == typeof(OrderRerollNormal) && buttonModKey == ClickableModname.OrderRerollNormal)
        {
            gameObject.SetActive(true);

        } else if (selectedmod.GetType() == typeof(VipReroll) && buttonModKey == ClickableModname.VIPReroll)
        {
            gameObject.SetActive(true);

        }
        else if (selectedmod.GetType() == typeof(FireSale) && buttonModKey == ClickableModname.FireSale)
        {
            gameObject.SetActive(true);

        }
    }
    private void OnModExpired(Mod selectedmod)
    {
        if (selectedmod.GetType() == typeof(OrderRerollNormal) && buttonModKey == ClickableModname.OrderRerollNormal)
        {
            gameObject.SetActive(false);

        }
        else if (selectedmod.GetType() == typeof(VipReroll) && buttonModKey == ClickableModname.VIPReroll)
        {
            gameObject.SetActive(false);

        }
        else if (selectedmod.GetType() == typeof(FireSale) && buttonModKey == ClickableModname.FireSale)
        {
            gameObject.SetActive(false);

        }
    }

    public void disableButton()
    {
        gameObject.SetActive(false);
    }

    public void OnRerollModButtonClick()
    {

        OrderRerollNormal activeMod = ModManager.Instance.GetMod <OrderRerollNormal>();
        if (activeMod != null)
        {
            if (!activeMod.onModClick())
            {
                disableButton();
            }
            ;
        }
        else {
            Debug.LogWarning("FAILED TO RUN MOD CLICK"); 
        }
    }

    public void OnVIPRerollModButtonClick()
    {

        VipReroll activeMod = ModManager.Instance.GetMod<VipReroll>();
        if (activeMod != null)
        {
            if (!activeMod.onModClick())
            {
                disableButton();

            }
            ;
        }
        else
        {
            Debug.LogWarning("FAILED TO RUN MOD CLICK");
        }
    }
    public void OnFireSaleModButtonClick()
    {

        FireSale activeMod = ModManager.Instance.GetMod<FireSale>();
        if (activeMod != null)
        {
            if (!activeMod.onModClick())
            {
                disableButton();
            }
            ;
        }
        else
        {
            Debug.LogWarning("FAILED TO RUN MOD CLICK");
        }
    }
}