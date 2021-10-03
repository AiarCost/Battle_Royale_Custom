using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;



public enum PickupType
{
    Health,
    Ammo
}

public class Pickup : MonoBehaviourPun
{

    public PickupType type;
    public int value;


    private void OnTriggerEnter(Collider col)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (col.CompareTag("Player"))
        {
            //Get the player
            PlayerController player = GameManager.instance.GetPlayer(col.gameObject);

            if (type == PickupType.Health)
                player.photonView.RPC("Heal", player.photonPlayer, value);
            else if (type == PickupType.Ammo)
                player.photonView.RPC("GiveAmmo", player.photonPlayer, value);

            // destroy the object
            photonView.RPC("DestroyPickup", RpcTarget.AllBuffered);  
        }
    }


    [PunRPC]
    public void DestroyPickup()
    {
        Destroy(gameObject);
    }
}



