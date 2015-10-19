﻿using UnityEngine;
using System.Collections;

/// <summary>
/// Purpose:???
/// </summary>
public class EnemyDeathSpawn : MonoBehaviour
{

    bool isShuttingDown = false;
    public GameObject onDeathCreate;

    void OnDestroy()
    {
        if (!isShuttingDown)
        {
            Vector3 off = new Vector3(2, 0, 0);
            Vector3 notoff = new Vector3(-2, 0, 0);
            GameObject make = (GameObject)GameObject.Instantiate(onDeathCreate, this.gameObject.transform.position + off, this.gameObject.transform.rotation);
            make.GetComponent<AIPath>().target = GameObject.FindWithTag("Player").transform;
            make = (GameObject)GameObject.Instantiate(onDeathCreate, this.gameObject.transform.position + notoff, this.gameObject.transform.rotation);
            make.GetComponent<AIPath>().target = GameObject.FindWithTag("Player").transform;
        }
    }

    void OnApplicationQuit()
    {
        isShuttingDown = true;
    }
}
