using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Economy;
using Unity.Services.Economy.Model;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class EconomySystem : MonoBehaviour
{
    [SerializeField] string coinID = "FIST_COIN";
    CurrencyDefinition fistCoinDefinition;
    public bool ready;
    public static long balance;

    MainMenu menu;
    Market market;

    void Awake()
    {
        DontDestroyOnLoad(this);
        menu = FindObjectOfType<MainMenu>();
    }

    public async void Start()
    {
        //start economy system
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        await EconomyService.Instance.Configuration.SyncConfigurationAsync();
        fistCoinDefinition = EconomyService.Instance.Configuration.GetCurrency(coinID);
        PlayerBalance fistCoinBalance = await fistCoinDefinition.GetPlayerBalanceAsync();
        balance = fistCoinBalance.Balance;
        StartCoroutine(menu.UpdateCoinsUI(balance));

        LoadInventory();
        ready = true;
        //AddCoins(1000);
    }

    public async void AddCoins(long lootedCoins)
    {
        PlayerBalance fistCoinBalance = await fistCoinDefinition.GetPlayerBalanceAsync();
        if (lootedCoins != 0) fistCoinBalance = await EconomyService.Instance.PlayerBalances.SetBalanceAsync(coinID, fistCoinBalance.Balance + lootedCoins);
        balance = fistCoinBalance.Balance;
        //StartCoroutine(menu.UpdateCoinsUI(balance));
    }

    public async void LoadInventory()
    {
        market = FindObjectOfType<Market>();
        GetInventoryResult playerInventory = await EconomyService.Instance.PlayerInventory.GetInventoryAsync();

        market.SetUpgradeStep(0, 0);
        market.SetUpgradeStep(1, 0);
        market.SetUpgradeStep(2, 0);
        market.SetUpgradeStep(3, 0);

        PlayerPrefs.SetInt("STRENGTH", 0);
        PlayerPrefs.SetInt("ATTACKSPEED", 0);
        PlayerPrefs.SetInt("SPECIAL", 0);
        PlayerPrefs.SetInt("MOVEMENT", 0);

        //adjust market inventory to match player's progress
        for (int i = 0; i < playerInventory.PlayersInventoryItems.Count; i++)
        {
            Debug.Log(playerInventory.PlayersInventoryItems[i].InventoryItemId);

            switch (playerInventory.PlayersInventoryItems[i].InventoryItemId)
            {
                case "STRENGTH1":
                    market.SetUpgradeStep(0, 1);
                    PlayerPrefs.SetInt("STRENGTH", 1);
                    break;
                case "STRENGTH2":
                    market.SetUpgradeStep(0, 2);
                    PlayerPrefs.SetInt("STRENGTH", 2);
                    break;
                case "STRENGTH3":
                    market.SetUpgradeStep(0, 3);
                    PlayerPrefs.SetInt("STRENGTH", 3);
                    break;
                case "STRENGTH4":
                    market.SetUpgradeStep(0, 4);
                    PlayerPrefs.SetInt("STRENGTH", 4);
                    break;

                case "ATTACKSPEED1":
                    market.SetUpgradeStep(1, 1);
                    PlayerPrefs.SetInt("ATTACKSPEED", 1);
                    break;
                case "ATTACKSPEED2":
                    market.SetUpgradeStep(1, 2);
                    PlayerPrefs.SetInt("ATTACKSPEED", 2);
                    break;
                case "ATTACKSPEED3":
                    market.SetUpgradeStep(1, 3);
                    PlayerPrefs.SetInt("ATTACKSPEED", 3);
                    break;
                case "ATTACKSPEED4":
                    market.SetUpgradeStep(1, 4);
                    PlayerPrefs.SetInt("ATTACKSPEED", 4);
                    break;

                case "SPECIAL1":
                    market.SetUpgradeStep(2, 1);
                    PlayerPrefs.SetInt("SPECIAL", 1);
                    break;
                case "SPECIAL2":
                    market.SetUpgradeStep(2, 2);
                    PlayerPrefs.SetInt("SPECIAL", 2);
                    break;
                case "SPECIAL3":
                    market.SetUpgradeStep(2, 3);
                    PlayerPrefs.SetInt("SPECIAL", 3);
                    break;
                case "SPECIAL4":
                    market.SetUpgradeStep(2, 4);
                    PlayerPrefs.SetInt("SPECIAL", 4);
                    break;

                case "MOVEMENT1":
                    market.SetUpgradeStep(3, 1);
                    PlayerPrefs.SetInt("MOVEMENT", 1);
                    break;
                case "MOVEMENT2":
                    market.SetUpgradeStep(3, 2);
                    PlayerPrefs.SetInt("MOVEMENT", 2);
                    break;
                case "MOVEMENT3":
                    market.SetUpgradeStep(3, 3);
                    PlayerPrefs.SetInt("MOVEMENT", 3);
                    break;
                case "MOVEMENT4":
                    market.SetUpgradeStep(3, 4);
                    PlayerPrefs.SetInt("MOVEMENT", 4);
                    break;
            }
        }
        market.Ready();
    }
}