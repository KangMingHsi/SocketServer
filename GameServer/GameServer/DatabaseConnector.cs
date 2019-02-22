using System;
using System.Threading.Tasks;

using Serilog;
using Npgsql;

namespace GameServer
{
	class DatabaseConnector
	{
		private string _connectionString;
		// config sequence: ip, port, id, password, db
		public DatabaseConnector(string[] config)
		{
			_connectionString = DatabaseCmd.GetConnectionConfig(config);
		}

		public void Close()
		{
			Log.Warning("DB is closed!");
		}

		public bool Login(ClientAccount account)
		{
			try
			{
				using (var conn = new NpgsqlConnection(_connectionString))
				{
					conn.Open();
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = conn;
						cmd.CommandText = DatabaseCmd.GetLoginCmd(account.Username, account.Password);

						bool success = (cmd.ExecuteNonQuery()) > 0;
						conn.Close();

						return success;
					}

				}
			}
			catch (Exception e)
			{
				Log.Error(e.Message);
				Close();
			}

			return false;
		}

		public void Logout(ClientAccount account)
		{
			try
			{
				using (var conn = new NpgsqlConnection(_connectionString))
				{
					conn.Open();
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = conn;
						cmd.CommandText = DatabaseCmd.GetLogoutCmd(account.Username, account.Password);
						cmd.ExecuteNonQuery();
					}
					conn.Close();
				}
			}
			catch (Exception e)
			{
				Log.Error(e.Message);
				Close();
			}

		}

		public void UpdateScore(ClientAccount account)
		{
			try
			{
				using (var conn = new NpgsqlConnection(_connectionString))
				{
					conn.Open();
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = conn;
						cmd.CommandText = DatabaseCmd.GetUpdateScoreCmd(account.Username, account.Score);
						cmd.ExecuteNonQuery();
					}
					conn.Close();
				}
			}
			catch (Exception e)
			{
				Log.Error(e.Message);
				Close();
			}
		}


	}
}
