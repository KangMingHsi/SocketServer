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
			_connectionString = GetConnectionConfig(config);
		}

		~DatabaseConnector()
		{
			Close();
		}

		public void Close()
		{
			Log.Warning("DB is closed!");
		}

		public bool Login(string usr, string pwd)
		{
			try
			{
				using (var conn = new NpgsqlConnection(_connectionString))
				{
					conn.Open();
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = conn;
						cmd.CommandText = string.Format("UPDATE player SET status=true WHERE username='{0}' and password='{1}' and status=false;", usr, pwd);

						if ((cmd.ExecuteNonQuery()) > 0)
						{
							return true;
						}
						else
						{
							return false;
						}

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

		public void Logout(string usr, string pwd)
		{
			try
			{
				using (var conn = new NpgsqlConnection(_connectionString))
				{
					conn.Open();
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = conn;
						cmd.CommandText = string.Format("UPDATE player SET status=false WHERE username='{0}' and password='{1}' and status=true;", usr, pwd);
						cmd.ExecuteNonQuery();
					}
				}
			}
			catch (Exception e)
			{
				Log.Error(e.Message);
				Close();
			}

		}

		private string GetConnectionConfig(string[] config)
		{
			return string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};", config[0], config[1], config[2], config[3], config[4]);
		}
	}
}
