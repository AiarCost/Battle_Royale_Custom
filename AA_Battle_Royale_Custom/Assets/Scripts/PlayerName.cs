using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerName : MonoBehaviour
{
    public TextMeshPro PlayerNameText;

    private void Awake()
    {
        PlayerNameText.text = GameManager.instance.GetPlayer(gameObject).photonPlayer.NickName;
    }


}
