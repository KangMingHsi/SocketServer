using GameNetwork;
using StackExchange.Redis;

namespace GameServer
{
	class RedisConnector
	{
		private ConnectionMultiplexer _conn;
		private IDatabase _db;

		public RedisConnector(string[] config)
		{
			string connectOptions = DatabaseCmd.GetRedisConfig(config);

			_conn = ConnectionMultiplexer.Connect(connectOptions);
			_db = _conn.GetDatabase();
		}

		// TODO check wait is necessary or not
		public void UpdateScoreTable(ClientAccount account)
		{
			var setTask = _db.HashSetAsync("UserScoreTable", account.Username, account.Score.ToString());
			_db.Wait(setTask);
		}

		public HashEntry[] GetAllScores()
		{
			var getTask = _db.HashGetAllAsync("UserScoreTable");
			var table = _db.Wait(getTask);

			return table;
		}

		public void Close()
		{
			_db = null;
			_conn.Close();
		}

	}
}
