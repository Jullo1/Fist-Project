using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Economy;
using UnityEngine;
using UnityEngine.UI;

public class Market : MonoBehaviour
{
    int[] ownedItemStep = new int[4];
    string[] upgradeNames = new string[16];
    [SerializeField] Button marketButton;
    [SerializeField] List<Button> upgradeButtons;
    [SerializeField] List<TextMeshProUGUI> upgradeTexts;
    [SerializeField] List<TextMeshProUGUI> upgradeCosts;

    public async void MarketPurchase(string purchaseType)
    {
        MarketButtonsActive(false);
        switch (purchaseType)
        {
            case "STRENGTH":
                if (ownedItemStep[0] >= 4) break;
                await EconomyService.Instance.Purchases.MakeVirtualPurchaseAsync("BUYSTRENGTH" + (ownedItemStep[0] + 1).ToString());
                SetUpgradeStep(0, ownedItemStep[0] + 1);
                break;
            case "ATTACKSPEED":
                if (ownedItemStep[1] >= 4) break;
                await EconomyService.Instance.Purchases.MakeVirtualPurchaseAsync("BUYATTACKSPEED" + (ownedItemStep[1] + 1).ToString());
                SetUpgradeStep(1, ownedItemStep[1] + 1);
                break;
            case "SPECIAL":
                if (ownedItemStep[2] >= 4) break;
                await EconomyService.Instance.Purchases.MakeVirtualPurchaseAsync("BUYSPECIAL" + (ownedItemStep[2] + 1).ToString());
                SetUpgradeStep(2, ownedItemStep[2] + 1);
                break;
            case "MOVEMENT":
                if (ownedItemStep[3] >= 4) break;
                await EconomyService.Instance.Purchases.MakeVirtualPurchaseAsync("BUYMOVEMENT" + (ownedItemStep[3] + 1).ToString());
                SetUpgradeStep(3, ownedItemStep[3] + 1);
                break;
        }
        MarketButtonsActive(true);
    }

    public void SetUpgradeStep(int type, int step)
    {
        ownedItemStep[type] = step;
        if (step >= 4) upgradeCosts[type].text = "MAX";
        else if (step == 0) upgradeCosts[type].text = "$4";
        else upgradeCosts[type].text = "$" + (Mathf.Pow(4, step + 1)).ToString();
        //upgradeTexts[type].text = 
    }

    public void Ready()
    {
        marketButton.interactable = true;
        Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
    }

    void MarketButtonsActive(bool enabled)
    {
        foreach (Button button in upgradeButtons)
            button.enabled = enabled;
    }
}
