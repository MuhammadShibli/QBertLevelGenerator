using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RedGemController : MonoBehaviour
{
    public int numOfStepsToRewind = 3;

    public Block block;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Destroy(this.gameObject,0.1f);
        }
    }
}
