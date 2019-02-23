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
				return _deltaTime;
			}
		}

		private DateTime _lastUpdateTime;
		private double _deltaTime;

		public void Start()
		{
			_lastUpdateTime = DateTime.Now;
		}

		public void Update()
		{
			_deltaTime = (DateTime.Now - _lastUpdateTime).TotalSeconds;
			_lastUpdateTime = DateTime.Now;
		}

	}
}
