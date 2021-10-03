using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class GameUI : MonoBehaviour
{
    public Slider healthBar;
    public TextMeshProUGUI playerInfoText;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI winText;
    public TextMeshProUGUI huntText;
    public Image winBackground;

    private PlayerController player;
    public string HuntedPlayerName;

    //instance
    public static GameUI instance;

    void Awake()
    {
        instance = this;
    }

    public void Initialize(PlayerController localPlayer)
    {
        player = localPlayer;
        healthBar.maxValue = player.maxHp;
        healthBar.value = player.curHp;

        UpdatePlayerInfoText();
        UpdateAmmoText();
        Invoke("HuntedText" , 2f);
    }

    public void UpdateHealthBar()
    {
        healthBar.value = player.curHp;
    }

    public void UpdatePlayerInfoText()
    {
        playerInfoText.text = "<b> Alive: </b>" + GameManager.instance.alivePlayers + "\n<b>Kills: </b> " + player.kills;
    }

    public void UpdateAmmoText()
    {
        ammoText.text = player.weapon.curAmmo + " / " + player.weapon.maxAmmo;
    }

    public void SetWinText(string winnerName, int killAmount)
    {
        winBackground.gameObject.SetActive(true);
        winText.text = winnerName + " wins with "+killAmount + " kills!";
    }


    [PunRPC]
    public void HuntedText()
    {
        //if the player does not have a hunt, give one (only should be on initialize)

        int HuntingPlayerId = -1;
        int count = 0;

        if (GameManager.instance.alivePlayersList.Count > 1)
        {

            while ( HuntingPlayerId == -1 || GameManager.instance.alivePlayersList[HuntingPlayerId] == PhotonNetwork.LocalPlayer.NickName)
            {
                Debug.Log(GameManager.instance.alivePlayersList.Count);
                //Grabbing the ID of someone to place as hunter List starts at 0...
                HuntingPlayerId = Random.Range(0, GameManager.instance.alivePlayersList.Count);
                

                //if the person grabbed is alive && not the local player themselves... then assign them.
                if (GameManager.instance.alivePlayersList[HuntingPlayerId] != PhotonNetwork.LocalPlayer.NickName)
                {
                    huntText.text = "Hunt: " + GameManager.instance.alivePlayersList[HuntingPlayerId];
                   
                    HuntedPlayerName = GameManager.instance.alivePlayersList[HuntingPlayerId];
                }
                else
                {
                    HuntingPlayerId = -1;
                    count++;
                }

                if (count >= 100)
                {
                    Debug.LogError("We cannot find a person to hunt. Please try again...if it persists, check the code...");
                    break;
                }

            }
        }

        else
        {
            Debug.Log("I'm sorry you cannot hunt yourself... find some friends :)");
            huntText.text = "No player to hunt.";

        }
    }

}
