using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
	class DatabaseCmd
	{
		public static string GetPostgresConfig(string[] config)
		{
			return string.Format(@"Server={0};
									Port={1};
									User Id={2};
									Password={3};
									Database={4};", config[0], config[1], config[2], config[3], config[4]);
		}

		public static string GetLoginCmd(string usr)
		{
			return string.Format("UPDATE player SET status=true WHERE username=E'{0}' and status=false;", usr);
		}

		public static string GetLogoutCmd(string usr)
		{
			return string.Format("UPDATE player SET status=false WHERE username=E'{0}' and status=true;", usr);
		}

		public static string GetUpdateScoreCmd(string usr, int score)
		{
			return string.Format("UPDATE player SET score={1} WHERE username=E'{0}';", usr, score.ToString());
		}

		public static string GetUpdateScoreCmd(string usr, string score)
		{
			return string.Format("UPDATE player SET score={1} WHERE username=E'{0}';", usr, score);
		}

		public static string GetSelectScoresCmd()
		{
			return string.Format("SELECT username, score FROM player;");
		}

		public static string GetSelectScoreCmd(string usr)
		{
			return string.Format("SELECT score FROM player WHERE username=E'{0}';", usr);
		}

		public static string GetSelectPasswordCmd(string usr)
		{
			return string.Format("SELECT password FROM player WHERE username=E'{0}';", usr);
		}


		// Redis Command Below
		public static string GetRedisConfig(string[] config)
		{
			return string.Format("{0}:{1}", config[0], config[1]);
		}
	}
}
