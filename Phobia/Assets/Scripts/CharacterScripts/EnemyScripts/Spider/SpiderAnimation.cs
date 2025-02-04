﻿using UnityEngine;
using System.Collections;

/// <summary>
/// Purpose: A spider exclusive class for controlling the animations.<para/>
/// Authors:            <para/>
/// Issues: Always moves no matter what.
/// </summary>
public class SpiderAnimation : MonoBehaviour
{

    // Defining the different animations
    public Animation animations;
    public string attack = "attack1";
    public string move = "walk";
    public string death = "death2";

    // Setup animations
    void Start()
    {
        // Set all animations to loop
        animations.wrapMode = WrapMode.Loop;
        // except attacking and death
        animations[attack].wrapMode = WrapMode.Once;
        animations[death].wrapMode = WrapMode.Once;

        // Attacking takes priority over moving. Dying takes highest priority.
        animations[attack].layer = 1;
        animations[death].layer = 2;
    }

    // Update is called once per frame
    void Update()
    {
        animations.CrossFade(move);
    }

    // Execute attack animation
    public void attackAnim()
    {
        animations.CrossFade(attack);
    }

    // Kill the spider
    public void spiderKilled()
    {
        animations.Play(death);
        // Stop moving
        GetComponent<AIPath>().speed = 0;
        // Destroy after death animation has finished
        Destroy(gameObject, animations[death].length);
    }
}
