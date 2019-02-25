using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
	class Constant
	{
		public const int WinScore = 5;
		public const int LoseSocre = -3;
		public const int MaxGameRoom = 50;
		public const int MaxClient = 100;

		public const double DBUpdateInterval = 60.0;
		public const double ClientNetworkUpdateInterval = 5.0;
	}
}
