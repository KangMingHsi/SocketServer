using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
	class PlayerMatching : IComparer<ClientPlayer>
	{
		public int Compare(ClientPlayer first, ClientPlayer second)
		{
			return first.Account.Score.CompareTo(second.Account.Score);
		}
	}
}
