﻿using UnityEngine;
using System.Collections;

//needs test to see if gems can be reset on restart
public class GemSelection : MonoBehaviour // : PersistentSingleton<GemSelection>
{

	public Gem gemOne;
	public Gem gemTwo;

	void awake ()
	{
		DontDestroyOnLoad (this);
	}
	// Use this for initialization
	public void selectGems (Gem gemOne, Gem gemTwo)
	{
		this.gemOne = gemOne;
		this.gemTwo = gemTwo;
	}

}
