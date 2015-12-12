﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : SingletonBehaviour<GameManager> 
{
	// Game state manager
	GameStateManager _gameStateManager = null;
	public static GameStateManager GSM
	{ get { return instance._gameStateManager; } }

	void Awake()
	{
		_gameStateManager = GetComponent<GameStateManager>();
	}

	void Update()
	{
		if(Input.GetKey( KeyCode.R ) && Input.GetKey( KeyCode.LeftAlt ))
			ResetLevel();
	}

	public void ResetLevel()
	{
		Application.LoadLevel( Application.loadedLevel );
		Destroy( gameObject );
	}
}
