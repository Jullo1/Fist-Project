using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Economy;
using Unity.Services.Economy.Model;
using UnityEngine.UI;
using System.Collections;

public class EconomySystem : MonoBehaviour
{
    [SerializeField] string coinID = "FIST_COIN";
    CurrencyDefinition fistCoinDefinition;
    public bool ready;
    public static long balance;

    MainMenu menu;

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

        //load player balance
        fistCoinDefinition = EconomyService.Instance.Configuration.GetCurrency(coinID);
        PlayerBalance fistCoinBalance = await fistCoinDefinition.GetPlayerBalanceAsync();
        balance = fistCoinBalance.Balance;
        StartCoroutine(menu.UpdateCoinsUI(balance));
        ready = true;
    }

    public async void AddCoins(long lootedCoins)
    {
        PlayerBalance fistCoinBalance = await fistCoinDefinition.GetPlayerBalanceAsync();
        if (lootedCoins != 0) fistCoinBalance = await EconomyService.Instance.PlayerBalances.SetBalanceAsync(coinID, fistCoinBalance.Balance + lootedCoins);
        balance = fistCoinBalance.Balance;
        //StartCoroutine(menu.UpdateCoinsUI(balance));
    }
}