using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Economy;
using UnityEngine;
using UnityEngine.UI;

public class Market : MonoBehaviour
{
    EconomySystem economy;
    int[] ownedItemStep = new int[4];
    string[,] upgradeNames = new string[4, 4] { { "Apple", "Pizza", "ChocoBar", "Sundae" }, { "Mineral Water", "Soda", "Energy Drink", "Thunder Aid" }, { "Hood", "Helmet", "Wizard Hat", "Headband" }, { "Boots", "Ultra Lightweight Shoes", "High End Sneakers", "Feathertech Experimental Footwear" } };

    [SerializeField] Sprite[] upgradeSprites;
    int[] costs = new int[4] {4,16,32,64};

    [SerializeField] Button marketButton;
    [SerializeField] List<Button> upgradeButtons;
    [SerializeField] List<TextMeshProUGUI> upgradeTexts;
    [SerializeField] List<TextMeshProUGUI> upgradeCosts;
    [SerializeField] List<Image> upgradeImages;

    public async void MarketPurchase(string purchaseType)
    {
        MarketButtonsActive(false);
        switch (purchaseType)
        {
            case "STRENGTH":
                if (ownedItemStep[0] >= 4) break;
                if ((int)EconomySystem.balance >= costs[ownedItemStep[0]])
                {
                    await EconomyService.Instance.Purchases.MakeVirtualPurchaseAsync("BUYSTRENGTH" + (ownedItemStep[0] + 1).ToString());
                    SetUpgradeStep(0, ownedItemStep[0] + 1);
                    PlayerPrefs.SetInt("STRENGTH", PlayerPrefs.GetInt("STRENGTH") + 1);
                }
                break;
            case "ATTACKSPEED":
                if (ownedItemStep[1] >= 4) break;
                if ((int)EconomySystem.balance >= costs[ownedItemStep[1]])
                {
                    await EconomyService.Instance.Purchases.MakeVirtualPurchaseAsync("BUYATTACKSPEED" + (ownedItemStep[1] + 1).ToString());
                    SetUpgradeStep(1, ownedItemStep[1] + 1);
                    PlayerPrefs.SetInt("ATTACKSPEED", PlayerPrefs.GetInt("ATTACKSPEED") + 1);
                }
                break;
            case "SPECIAL":
                if (ownedItemStep[2] >= 4) break;
                if ((int)EconomySystem.balance >= costs[ownedItemStep[2]])
                {
                    await EconomyService.Instance.Purchases.MakeVirtualPurchaseAsync("BUYSPECIAL" + (ownedItemStep[2] + 1).ToString());
                    SetUpgradeStep(2, ownedItemStep[2] + 1);
                    PlayerPrefs.SetInt("SPECIAL", PlayerPrefs.GetInt("SPECIAL") + 1);
                }
                break;
            case "MOVEMENT":
                if (ownedItemStep[3] >= 4) break;
                if ((int)EconomySystem.balance >= costs[ownedItemStep[3]])
                {
                    await EconomyService.Instance.Purchases.MakeVirtualPurchaseAsync("BUYMOVEMENT" + (ownedItemStep[3] + 1).ToString());
                    SetUpgradeStep(3, ownedItemStep[3] + 1);
                    PlayerPrefs.SetInt("MOVEMENT", PlayerPrefs.GetInt("MOVEMENT") + 1);
                }
                break;
        }
        economy.ReloadCurrency();
        MarketButtonsActive(true);
    }

    public void SetUpgradeStep(int type, int step)
    {
        //Debug.Log(type.ToString() + " " + step.ToString());
        ownedItemStep[type] = step;
        if (step >= 4)
        {
            upgradeCosts[type].text = "MAX";
            upgradeTexts[type].text = upgradeNames[type, 3];
            upgradeImages[type].sprite = upgradeSprites[(type * 4) + 3];
        }
        else
        {
            upgradeCosts[type].text = costs[step].ToString();
            upgradeTexts[type].text = upgradeNames[type, step]; //setup icons and names
            upgradeImages[type].sprite = upgradeSprites[(type * 4) + step];
        }
    }

    public void Ready()
    {
        marketButton.interactable = true;
        Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
        economy = FindObjectOfType<EconomySystem>();
    }

    void MarketButtonsActive(bool enabled)
    {
        foreach (Button button in upgradeButtons)
            button.enabled = enabled;
    }
}
