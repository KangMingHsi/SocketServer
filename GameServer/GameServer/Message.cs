using System;
using System.Collections.Generic;
using System.Text;

namespace GameNetwork
{
	public enum Message : int
	{
		Error = -1,
		Disconnect,

		SignIn,
		SignInSuccess,
		SignInFail,

		MatchGame,
		GameAction,


		NoMeaning
	}
}
