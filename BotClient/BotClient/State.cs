using System;
using System.Collections.Generic;
using System.Text;

namespace BotClient
{
	interface State<T> where T : class
	{
		void Enter(T t);
		void Execute(T t);
		void Exit(T t);
		bool HandleMessage(T t, string msg);
	}
}
