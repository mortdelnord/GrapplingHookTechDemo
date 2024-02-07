using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManage : MonoBehaviour
{


    public GameObject grappleRet;
    public GameObject noGrap;


    public Player player;



    private void Update()
    {
        if (player.canGrapple)
        {
            grappleRet.SetActive(true);
            noGrap.SetActive(false);
        }else
        {
            grappleRet.SetActive(false);
            noGrap.SetActive(true);
        }
    }





}
