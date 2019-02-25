using Serilog;

using GameNetwork;

namespace GameServer
{
	class DatabaseHelper
	{
		private double _lastUpdateTime;
		private double _updateInterval;
		private double _currentTime;
		private PostgresConnector _postgresConnector;
		private RedisConnector _redisConnector;

		public DatabaseHelper(string[] postgresConfig, string[] redisConfig)
		{
			_postgresConnector = new PostgresConnector(postgresConfig);
			_redisConnector = new RedisConnector(redisConfig);

			_lastUpdateTime = 0.0;
			_currentTime = 0.0;
			_updateInterval = 60.0;

			ResetRedis();
		}

		public void SynchronizeDatabase(double deltaTime)
		{
			_currentTime += deltaTime;

			if ((_currentTime - _lastUpdateTime) > _updateInterval)
			{
				Log.Warning("同步資料");
				_lastUpdateTime = _currentTime;

				var scoreTable = _redisConnector.GetAllScores();

				for (int i = 0; i < scoreTable.Length; ++i)
				{
					_postgresConnector.UpdateScore(scoreTable[i].Name, scoreTable[i].Value);
				}
			}
		}

		public void Login(ref ClientAccount account)
		{
			Log.Information("帳號登入中");
			_postgresConnector.Login(ref account);

			if (account.IsOnline)
			{
				Log.Information("驗證通過並初始化資料");
				_redisConnector.UpdateScoreTable(account);
			}
		}

		public void Logout(ref ClientAccount account)
		{
			Log.Information("帳號登出");
			_postgresConnector.Logout(ref account);
		}

		public void UpdateScore(ClientAccount account)
		{
			_redisConnector.UpdateScoreTable(account);
		}

		public void Close()
		{
			Log.Information("關閉資料庫");
			_postgresConnector.Close();
			_redisConnector.Close();
		}

		// TODO postgres to redis
		private void ResetRedis()
		{
			var scorePairs = _postgresConnector.GetScorePairs();

			for(int i = 0; i < scorePairs.Length; ++i)
			{
				_redisConnector.UpdateScoreTable(scorePairs[i].Item1, scorePairs[i].Item2);
			}
		}
	}
}
