using System;
using System.Collections.Generic;
using System.Text;

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
		}

		public void SynchronizeDatabase(double deltaTime)
		{
			_currentTime += deltaTime;

			if (_currentTime - _lastUpdateTime > _updateInterval)
			{
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
			_postgresConnector.Login(ref account);

			if (account.IsOnline)
			{
				_redisConnector.UpdateScoreTable(account);
			}
		}

		public void Logout(ref ClientAccount account)
		{
			_postgresConnector.Logout(ref account);
		}

		public void UpdateScore(ClientAccount account)
		{
			//_postgresConnector.UpdateScore(account);
			_redisConnector.UpdateScoreTable(account);
		}

		public void Close()
		{
			_postgresConnector.Close();
			_redisConnector.Close();
		}
	}
}
