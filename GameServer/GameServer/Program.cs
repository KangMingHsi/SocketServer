using System;
using Npgsql;
using Serilog;

namespace GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
			Log.Logger = new LoggerConfiguration()
						.WriteTo.Console()
						.CreateLogger();

			Server server = new Server("127.0.0.1", 36000);
			server.Run();

			//String connectionString = "Server=127.0.0.1;Port=5432;User Id=sean_kang;Password=jfigames;Database=train;";

			//using (var conn = new NpgsqlConnection(connectionString))
			//{
			//	conn.Open();

			//	// Insert some data
			//	using (var cmd = new NpgsqlCommand())
			//	{
			//		for (int i = 0; i < 100; ++i)
			//		{
			//			cmd.Connection = conn;
			//			cmd.CommandText = "INSERT INTO player (status, password, username) VALUES(false, '123456', 'player" + i.ToString() + "');";
			//			cmd.Parameters.AddWithValue("p", "Hello world");
			//			cmd.ExecuteNonQuery();
			//		}
			//	}

			//	// Retrieve all rows
			//	//using (var cmd = new NpgsqlCommand("SELECT some_field FROM data", conn))
			//	//using (var reader = cmd.ExecuteReader())
			//	//	while (reader.Read())
			//	//		Log.Information(reader.GetString(0));
			//}
		}

	}
	
}
