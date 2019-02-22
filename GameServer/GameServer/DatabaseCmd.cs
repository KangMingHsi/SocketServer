using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
	class DatabaseCmd
	{
		public static string GetConnectionConfig(string[] config)
		{
			return string.Format(@"Server={0};
									Port={1};
									User Id={2};
									Password={3};
									Database={4};
									Pooling=true;
									MaxPoolSize=150;", config[0], config[1], config[2], config[3], config[4]);
		}

		public static string GetLoginCmd(string usr, string pwd)
		{
			return string.Format("UPDATE player SET status=true WHERE username='{0}' and password='{1}' and status=false;", usr, pwd);
		}

		public static string GetLogoutCmd(string usr, string pwd)
		{
			return string.Format("UPDATE player SET status=false WHERE username='{0}' and password='{1}' and status=true;", usr, pwd);
		}

		public static string GetUpdateScoreCmd(string usr, int score)
		{
			return string.Format("UPDATE player SET score={1} WHERE username='{0}';", usr, score.ToString());
		}
	}
}
