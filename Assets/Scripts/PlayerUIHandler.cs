using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIHandler : MonoBehaviour
{
    Player player;

    //UI Elements
    public GameObject[] hitpointsUI = new GameObject[3];
    public List<Image> attackCDUI = new List<Image>();
    public List<Outline> attackCDOutlines = new List<Outline>();

    [SerializeField] Text comboUI;
    [SerializeField] Image comboProgressBar;

    [SerializeField] Image specialUI;
    [SerializeField] Image specialTriggerUI1;
    [SerializeField] Image specialTriggerUI2;
    Outline specialOutline;

    Tutorial tutorial;

    void Awake()
    {
        player = GetComponent<Player>();
        tutorial = FindObjectOfType<Tutorial>();
        hitpointsUI = GameObject.FindGameObjectsWithTag("PlayerHealthBar");
        specialOutline = specialUI.gameObject.transform.parent.GetComponentInParent<Outline>();
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

        if (player.comboAmount == 0) { comboProgressBar.transform.parent.gameObject.SetActive(false); comboUI.text = ""; comboUI.color = new Color32(255, 255, 255, 255); }
        else
        {
            comboUI.color = new Color32(225, (byte)Mathf.Lerp(160, 80, player.comboAmount / player.maxComboCDBoost), 10, 255);
            comboProgressBar.transform.parent.gameObject.SetActive(true);
            comboUI.text = "x" + player.comboAmount.ToString();
            comboProgressBar.color = comboUI.color;
            comboProgressBar.fillAmount = player.comboAmount / player.maxComboCDBoost;
        }
        if (player.comboAmount >= player.maxComboCDBoost) comboUI.fontSize = 38;
        else comboUI.fontSize = 34;

        foreach (Image circle in attackCDUI)
            circle.color = comboUI.color;

        UpdateComboStats();
    }

    public void SendTutorial()
    {
        tutorial.gameObject.SetActive(true);
        tutorial.SendTutorial();
    }

    public void SpecialReady(bool status)
    {
        if (status) specialOutline.enabled = true;
        else specialOutline.enabled = false;
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
        foreach (GameObject bar in hitpointsUI)
            bar.SetActive(true);

        for (int i = hitpointsUI.Length; i > player.hitpoints; i--)
            hitpointsUI[i - 1].gameObject.SetActive(false);
    }
}
