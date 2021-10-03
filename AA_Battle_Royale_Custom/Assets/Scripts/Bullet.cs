using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    private int damage;
    private int attackerId;
    private bool IsMine;

    public Rigidbody rig;


    public void Initialize(int damage, int attackerId, bool IsMine)
    {
        this.damage = damage;
        this.attackerId = attackerId;
        this.IsMine = IsMine;

        Destroy(gameObject, 5.0f);
    }

    private void OnTriggerEnter(Collider col)
    {
        // did we hit a player?
        // if this is the local player's bullet, damage the hit player
        //we're using client side hit detection

        if(col.CompareTag("Player") && IsMine)
        {
            PlayerController player = GameManager.instance.GetPlayer(col.gameObject);

            if(player.id != attackerId)
            {
                player.photonView.RPC("TakeDamage", player.photonPlayer, attackerId, damage);
            }

            Destroy(gameObject);
        }


    }
}
