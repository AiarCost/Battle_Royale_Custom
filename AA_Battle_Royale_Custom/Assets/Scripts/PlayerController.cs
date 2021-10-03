using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;


public class PlayerController : MonoBehaviourPun
{
    [Header("Stats")]
    public float moveSpeed;
    public float jumpForce;

    [Header("Components")]
    public Rigidbody rig;

    public int id;
    public Player photonPlayer;
    public string PlayerName;

    private int curAttackId;

    public int curHp;
    public int maxHp;
    public int kills;
    public bool dead;
    private bool flashingDamage;
    public MeshRenderer mr;

    public PlayerWeapon weapon;

    public TextMeshPro PlayerNameText;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine || dead)
            return;

        Move();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryJump();
        }

        if (Input.GetMouseButtonDown(0))
            weapon.TryShoot();
    }


    private void Move()
    {
        // get the input axis
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // calculate a direction relative to where we're facing
        Vector3 dir = (transform.forward * z + transform.right * x) * moveSpeed;
        dir.y = rig.velocity.y;

        // set that as our velocity
        rig.velocity = dir;

    }

    void TryJump()
    {
        // create a ray facing down
        Ray ray = new Ray(transform.position, Vector3.down);

        // shoot the raycast
        if (Physics.Raycast(ray, 1.5f))
            rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

    }

    [PunRPC]
    public void Initialize(Player player)
    {
        id = player.ActorNumber;
        photonPlayer = player;

        GameManager.instance.players[id - 1] = this;

        // is this not our local player?
        if (!photonView.IsMine)
        {
            GetComponentInChildren<Camera>().gameObject.SetActive(false);
            rig.isKinematic = true;
        }
        else
        {
            GameUI.instance.Initialize(this);
        }

        PlayerNameText.text = player.NickName;
        GameManager.instance.alivePlayersList.Add(player.NickName);
    }


    [PunRPC]
    public void TakeDamage(int attackerId, int damage)
    {
        if (dead)
            return;

        curHp -= damage;
        curAttackId = attackerId;

        // flash teh player red
        photonView.RPC("DamageFlash", RpcTarget.Others);

        //Update the health bar UI
        GameUI.instance.UpdateHealthBar();

        // die if no health left 
        if (curHp <= 0)
        {
            photonView.RPC("Die", RpcTarget.All, "TakeDamage");
            Debug.Log("Take Damage has killed:" + photonPlayer.NickName);
        }
    }

    [PunRPC]
    void DamageFlash()
    {
        if (flashingDamage)
            return;

        StartCoroutine(DamageFlashCoRoutine());

        IEnumerator DamageFlashCoRoutine()
        {
            flashingDamage = true;

            Color defaultColor = mr.material.color;
            mr.material.color = Color.red;

            yield return new WaitForSeconds(0.05f);

            mr.material.color = defaultColor;
            flashingDamage = false;
        }

        

    }


    [PunRPC]
    void Die(string FunctionCall)
    {
        curHp = 0;
        dead = true;
      

        GameManager.instance.alivePlayers--;

        //host will check win condition
        if (PhotonNetwork.IsMasterClient)
            GameManager.instance.CheckWinCondition();

        GameManager.instance.alivePlayersList.Remove(photonPlayer.NickName);

        // is this our local player?
        if (photonView.IsMine)
        {
            if (curAttackId != 0)
                GameManager.instance.GetPlayer(curAttackId).photonView.RPC("AddKill", RpcTarget.All, photonPlayer.NickName);

            //set the cam to spectator
            GetComponentInChildren<CameraController>().SetAsSpectator();

            //disable the physics and hide the player
            rig.isKinematic = true;
            transform.position = new Vector3(0, -50, 0);
        }

    }


    [PunRPC]
    public void AddKill(string KilledPlayerName)
    {

        //A player has died, if this was the hunted, then new target, else wrong person kills the local player
        Debug.Log(GameUI.instance.HuntedPlayerName + " " + KilledPlayerName + " The if statement for kills");
        if(KilledPlayerName == GameUI.instance.HuntedPlayerName)
        {
            kills++;
            GameUI.instance.UpdatePlayerInfoText();
            GameUI.instance.HuntedText();

        }
        else
        {
            GameUI.instance.UpdatePlayerInfoText();
            GameUI.instance.HuntedText();
        }


    }

    [PunRPC]
    public void Heal(int amountToHeal)
    {
        curHp = Mathf.Clamp(curHp + amountToHeal, 0, maxHp);

        //Update the health bar UI
        GameUI.instance.UpdateHealthBar();
    }

}
