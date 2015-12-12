using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameStateManager : MonoBehaviour
{
	// Map of game state types to a corresponding game type object
	Dictionary<GameStateTypes, GameStateBase> _stateTypeToStateMap = new Dictionary<GameStateTypes, GameStateBase>();

	Stack<GameStateBase> _activeGameStates = new Stack<GameStateBase>();

	[SerializeField] GameStateTypes _initGameStateType = 0;

	Transform _gameModeParent = null;

	void Awake()
	{
		_gameModeParent = new GameObject("GameModes").transform;
		_gameModeParent.SetParent(transform);

		InitializeGameStateTypes();

		if(_initGameStateType != GameStateTypes.None)
			PushGameState(_initGameStateType);
	}

	public void Reinitialize(GameStateTypes gameStateType)
	{
		_stateTypeToStateMap.Clear();

		InitializeGameStateTypes();
		PushGameState(gameStateType);
	}

	void Update()
	{
		if(_activeGameStates.Count > 0)
			_activeGameStates.Peek().OnUpdateState();
	}

	void InitializeGameStateTypes()
	{
		// Add game states here with their associated class
		// TODO: Move this to use reflection or some associated type system like in BCB
		// these game mode object may eventually take up a non-trivial amount of memory

		// Menus
//		_stateTypeToStateMap.Add(GameStateTypes.TeamSelect, SpawnGameModeGO<GameStateTeamSelect>());
	}

	T SpawnGameModeGO<T>() where T : GameStateBase
	{
		T tObj = new GameObject(typeof(T).ToString(), typeof(T)).GetComponent<T>();
		tObj.transform.SetParent(_gameModeParent);

		return tObj;
	}

	public void PushGameState(GameStateTypes gameStateType)
	{
		if(_activeGameStates.Count > 0)
			_activeGameStates.Peek().OnPauseState();

		_activeGameStates.Push(_stateTypeToStateMap[gameStateType]);
		_stateTypeToStateMap[gameStateType].OnEnterState();
	}

	public void PopGameState()
	{
		if(_activeGameStates.Count > 0)
		{
			_activeGameStates.Peek().OnExitState();
			Destroy(_activeGameStates.Pop().gameObject);
		}

		if(_activeGameStates.Count > 0)
			_activeGameStates.Peek().OnUnpauseState();
	}

	public void ClearGameStates()
	{
		while(_activeGameStates.Count > 0)
		{
			_activeGameStates.Peek().OnExitState();
			Destroy(_activeGameStates.Pop().gameObject);
		}
	}

	public bool IsInState(GameStateTypes type)
	{
		return _activeGameStates.Peek() == _stateTypeToStateMap[type];
	}
}
