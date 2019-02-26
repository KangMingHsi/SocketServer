using System;
using System.Collections.Generic;
using System.Text;

namespace BotClient
{
	struct LocalMessagePackage
	{
		public bool IsLocal;
		public string LocalMessage;
		public byte[] NetWorkMessage;

		public LocalMessagePackage(string msg)
		{
			IsLocal = true;
			LocalMessage = msg;
			NetWorkMessage = null;
		}

		public LocalMessagePackage(byte[] msg)
		{
			IsLocal = false;
			LocalMessage = null;
			NetWorkMessage = msg;
		}
	}
}
