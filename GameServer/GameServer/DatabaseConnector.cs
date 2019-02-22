using System;
using System.IO;

using Serilog;
using Npgsql;
using GameNetwork;

namespace GameServer
{
	class DatabaseConnector
	{
		private string _connectionString;
		private string _key;
		// config sequence: ip, port, id, password, db
		public DatabaseConnector(string[] config)
		{
			_connectionString = DatabaseCmd.GetConnectionConfig(config);

			using (FileStream file = new FileStream("PrivateKey.xml", FileMode.Open, FileAccess.Read))
			{
				byte[] bytes = new byte[1024];
				file.Read(bytes, 0, 1024);
				_key = System.Text.Encoding.ASCII.GetString(bytes);
			}
		}

		public void Close()
		{
			Log.Warning("DB is closed!");
		}

		public void Login(ref ClientAccount account)
		{
			try
			{
				using (var conn = new NpgsqlConnection(_connectionString))
				{
					Log.Information("帳號登入中");
					conn.Open();
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = conn;
						cmd.CommandText = DatabaseCmd.GetSelectPasswordCmd(account.Username);

						string storedPassword;

						using (var reader = cmd.ExecuteReader())
						{
							if (reader.Read())
							{
								storedPassword = reader.GetString(0);
							}
							else
							{
								return;
							}
						}

						if (Verification(storedPassword, account.Password))
						{
							Log.Information("驗證通過");
							cmd.CommandText = DatabaseCmd.GetLoginCmd(account.Username);
							account.IsOnline = (cmd.ExecuteNonQuery()) > 0;

							cmd.CommandText = DatabaseCmd.GetSelectScoreCmd(account.Username);
							using (var reader = cmd.ExecuteReader())
							{
								reader.Read();
								account.Score = reader.GetInt32(0);
							}
						}

						conn.Close();
					}

				}
			}
			catch (Exception e)
			{
				Log.Error(e.Message);
				Close();
			}
		}

		public void Logout(ref ClientAccount account)
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

						cmd.CommandText = DatabaseCmd.GetLogoutCmd(account.Username);
						cmd.ExecuteNonQuery();

						account.IsOnline = false;
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

		private bool Verification(string storedPassword, string accountPassword)
		{
			Log.Information("驗證密碼中");
			return RSAHelper.Decrypt(_key, storedPassword).Equals(RSAHelper.Decrypt(_key, accountPassword));
		}
	}
}
