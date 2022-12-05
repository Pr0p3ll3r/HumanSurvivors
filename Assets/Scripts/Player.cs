using UnityEngine;
using System.Collections;
using FishNet.Object;
using TMPro;
using FishNet.Object.Synchronizing;

public class Player : NetworkBehaviour
{
    public TextMeshPro playerNickname;
    public bool invincible;

    void Update()
    {      

    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner) return;

        Camera.main.GetComponent<CameraFollow>().SetPlayer(transform);  
    }
}
