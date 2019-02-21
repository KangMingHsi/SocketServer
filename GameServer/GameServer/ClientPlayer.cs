using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
	class ClientPlayer
	{
		private ClientNetwork _network;
		private List<int> _actions;

		public ClientPlayer()
		{
			_actions = new List<int>();
		}

		public bool HasInput()
		{
			return _actions.Count > 0;
		}

		public int GetAction()
		{
			int action = _actions[0];
			_actions.Clear();
			return action;
		}

		public void HandleMessage()
		{

		}
	}
}
