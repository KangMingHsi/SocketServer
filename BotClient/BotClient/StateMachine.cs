using System;
using System.Collections.Generic;
using System.Text;

namespace BotClient
{
	class StateMachine<T> where T : class
	{
		private T _owner;
		private State<T> _currentState;
		private State<T> _previousState;
		private State<T> _globalState;

		public StateMachine(T owner)
		{
			_owner = owner;
			_currentState = null;
			_previousState = null;
			_globalState = null;
		}

		public void SetCurrentState(State<T> s) { _currentState = s; }
		public void SetGlobalState(State<T> s) { _globalState = s; }
		public void SetPreviousState(State<T> s) { _previousState = s; }

		public void Update()
		{
			if(_globalState != null) _globalState.Execute(_owner);
			if (_currentState != null) _currentState.Execute(_owner);
		}

		public bool HandleMessage(LocalMessagePackage msg)
		{
		  //first see if the current state is valid and that it can handle
		  //the message
		  if (_currentState != null && _currentState.HandleMessage(_owner, msg))
		  {
		    return true;
		  }

		  //if not, and if a global state has been implemented, send 
		  //the message to the global state
		  if (_globalState != null && _globalState.HandleMessage(_owner, msg))
		  {
		    return true;
		  }
		  return false;
		}

		//change to a new state
		public void ChangeState(State<T> newState)
		{
			_previousState = _currentState;

			_currentState.Exit(_owner);
			_currentState = newState;
			_currentState.Enter(_owner);
		}

		//change state back to the previous state
		public void RevertToPreviousState()
		{
			ChangeState(_previousState);
		}

		//returns true if the current state's type is equal to the type of the
		//class passed as a parameter. 
		public bool IsInState(State<T> st)
		{
			if (st.GetHashCode().Equals(_currentState.GetHashCode())) return true;
			return false;
		}

		public State<T> CurrentState()  {return _currentState;}
		public State<T> GlobalState()   {return _globalState;}
		public State<T> PreviousState() {return _previousState;}

	};
}

