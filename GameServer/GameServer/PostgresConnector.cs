using System;

using Serilog;
using Npgsql;
using GameNetwork;
using System.Collections.Generic;

namespace GameServer
{
	class PostgresConnector
	{
		private string _connectionString;
		private RSAServerProvider _rsa;
		// config sequence: ip, port, id, password, db
		public PostgresConnector(string[] config)
		{
			_connectionString = DatabaseCmd.GetPostgresConfig(config);
			_rsa = new RSAServerProvider();
		}

		public void Close()
		{
			
		}

		public void Login(ref ClientAccount account)
		{
			try
			{
				using (var conn = new NpgsqlConnection(_connectionString))
				{
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

						if (_rsa.Verification(storedPassword, account.Password))
						{
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
						cmd.CommandText = DatabaseCmd.GetLogoutCmd(account.Username);
						cmd.ExecuteNonQuery();

						account.IsOnline = false;
						account.IsMatch = false;
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

		public void UpdateScore(string username, string score)
		{
			try
			{
				using (var conn = new NpgsqlConnection(_connectionString))
				{
					conn.Open();
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = conn;
						cmd.CommandText = DatabaseCmd.GetUpdateScoreCmd(username, score);
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

		public Tuple<string, string>[] GetScorePairs()
		{
			List<Tuple<string, string>> tuple = new List<Tuple<string, string>>();

			try
			{
				using (var conn = new NpgsqlConnection(_connectionString))
				{
					conn.Open();
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = conn;
						cmd.CommandText = DatabaseCmd.GetSelectScoresCmd();
						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								tuple.Add(new Tuple<string, string>(reader.GetString(0), reader.GetInt32(1).ToString()));
							}
						}
					}
					conn.Close();
				}
			}
			catch (Exception e)
			{
				Log.Error(e.Message);
				Close();
			}
		
			return tuple.ToArray();
		}
	}
}
