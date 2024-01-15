using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIHandler : MonoBehaviour
{
    Player player;

    //UI Elements
    public List<Image> hitpointsUI = new List<Image>();
    public List<Image> attackCDUI = new List<Image>();

    [SerializeField] Text comboUI;
    [SerializeField] Image comboProgressBar;

    [SerializeField] Image specialUI;
    [SerializeField] Image specialTriggerUI1;
    [SerializeField] Image specialTriggerUI2;

    [SerializeField] List<Outline> attackCDOutlines = new List<Outline>();

    [SerializeField] Tutorial tutorial;

    void Awake()
    {
        player = GetComponent<Player>();
        tutorial = FindObjectOfType<Tutorial>();
    }

    void Update()
    {

        for (int i = 0; i < attackCDUI.Count; i++)
            attackCDUI[i].fillAmount = player.attackTimer[i] / player.attackCD[i];

        specialUI.fillAmount = player.specialTimer / player.specialCD;

        if (player.specialChannel > 0.20f) //small delay before feedback, in case player intended to tap
        {
            specialTriggerUI1.fillAmount = (player.specialChannel - 0.20f) / 0.20f;
            specialTriggerUI2.fillAmount = (player.specialChannel - 0.20f) / 0.20f;
        }
        else
        {
            specialTriggerUI1.fillAmount = 0;
            specialTriggerUI2.fillAmount = 0;
        }

        if (player.comboAmount > 1)
        {

            comboProgressBar.transform.parent.gameObject.SetActive(true);
            comboProgressBar.fillAmount = player.comboAmount / player.maxComboCDBoost;
            comboUI.text = "x" + player.comboAmount.ToString();
            if (player.comboAmount >= player.maxComboCDBoost) //max combo boost achieved
            {
                comboUI.color = new Color32(250, 100, 0, 255);
                comboUI.fontSize = 40;
            }
            else if (player.comboAmount >= player.maxComboCDBoost / 2)
            {
                comboUI.color = new Color32(240, 160, 0, 255);
            }
            else if (player.comboAmount >= player.maxComboCDBoost / 4)
            {
                comboUI.color = new Color32(255, 255, 50, 255);
                comboUI.fontSize = 36;
            }
            else
            {
                comboProgressBar.transform.parent.gameObject.SetActive(false);
                comboUI.color = new Color32(255, 255, 255, 255);
            }
        }
        else //if it's 0, hide combo count and bar
        {
            comboUI.text = "";
            comboUI.fontSize = 34;
            comboUI.color = new Color32(255, 255, 255, 255);
            comboProgressBar.transform.parent.gameObject.SetActive(false);
        }
        comboProgressBar.color = comboUI.color;

        foreach (Outline outline in attackCDOutlines)
            outline.effectColor = comboUI.color;

        UpdateComboStats();
    }

    public void SendTutorial()
    {
        tutorial.gameObject.SetActive(true);
        tutorial.SendTutorial();
    }


    void UpdateComboStats() //comboAmount makes attackTimer floats increase faster (does not affect the cooldown number)
    {
        if (player.comboAmount == 0)
        {
            player.comboCDBoost = 1f;
            return;
        }
        if (player.comboAmount < player.maxComboCDBoost) player.comboCDBoost = 1 + ((float)player.comboAmount / 100); //every x1 makes attack charge 1% faster, up to maxComboCDBoost (starts at 20 on new game)
        else player.comboCDBoost = 1 + ((float)player.maxComboCDBoost / 100);
    }

    public void CheckHitpoints()
    {
        foreach (Image bar in hitpointsUI)
            bar.gameObject.SetActive(true);

        for (int i = hitpointsUI.Count; i > player.hitpoints; i--)
            hitpointsUI[i - 1].gameObject.SetActive(false);
    }
}
