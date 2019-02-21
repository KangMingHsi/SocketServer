using System;
using System.Collections.Generic;
using System.Text;

namespace GameNetwork
{
	class Message
	{
		public const int Disconnect = 88;
		public const int SignIn = 91;
		public const int SignInSuccess = 1;
		public const int SignInFail = 0;


		public const int Error = -1;
		public const int NoMeaning = 777;
	}
}
