namespace GameNetwork
{
	struct ClientAccount
	{
		public string Username;
		public string Password;
		public bool IsOnline;
		public int Score;
		public bool IsMatch;

		public static byte[] ToBytes(ClientAccount account)
		{
			RSAClientProvider rsa = new RSAClientProvider();

			MessageBuffer messageBuffer = new MessageBuffer(new byte[2048]);

			messageBuffer.WriteInt((int)Message.SignIn);
			messageBuffer.WriteString(account.Username);
			messageBuffer.WriteString(rsa.Encrypt(account.Password));

			return messageBuffer.Buffer;
		}
	}
}
