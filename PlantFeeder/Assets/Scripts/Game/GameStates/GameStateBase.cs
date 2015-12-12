using UnityEngine;
using System.Collections;

public abstract class GameStateBase : WadeBehaviour 
{
	// Called when a state is first added to the game state stack
	public virtual void OnEnterState(){}

	// Called when a state is on top of the stack every update loop
	public virtual void OnUpdateState(){}

	// Called when a state is removed from the game state stack
	public virtual void OnExitState(){}

	// Called when a state a state is placed on top of the state stack above this state
	public virtual void OnPauseState(){}

	// Called when the top state of the stack is removed, making this state the top of the stack
	public virtual void OnUnpauseState(){}
}
