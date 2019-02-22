using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
	class Timer
	{
		public double DeltaTime
		{
			get
			{
				var value = (DateTime.Now - _lastUpdateTime).TotalSeconds;
				_lastUpdateTime = DateTime.Now;
				return value;
			}
		}

		private DateTime _lastUpdateTime;

		public void Start()
		{
			_lastUpdateTime = DateTime.Now;
		}

	}
}
