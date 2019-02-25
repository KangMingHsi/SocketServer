using System;
using System.Threading;
using System.Collections.Generic;

using Serilog;

namespace GameServer
{
	class RoomManager
	{
		private List<GameRoom> _gamingRooms;
		private List<GameRoom> _availableRooms;

		public RoomManager()
		{
			_gamingRooms = new List<GameRoom>();
			_availableRooms = new List<GameRoom>();
		}

		public void Init(Server server)
		{
			for (int room = 0; room < Constant.MaxGameRoom; ++room)
			{
				_availableRooms.Add(new GameRoom(server, room));
			}
		}

		public void Update(List<ClientPlayer> pendingPlayers)
		{
			CheckGameIsOver();
			MatchPendingPlayers(pendingPlayers);
		}

		public void Clear()
		{
			_availableRooms.Clear();
			_gamingRooms.Clear();
		}

		private void CheckGameIsOver()
		{
			for (int i = _gamingRooms.Count - 1; i >= 0; --i)
			{
				if (_gamingRooms[i].IsOver)
				{
					Log.Information("回收房間");
					_availableRooms.Add(_gamingRooms[i]);
					_gamingRooms.RemoveAt(i);
				}
			}
		}

		private void MatchPendingPlayers(List<ClientPlayer> pendingPlayers)
		{
			if (_availableRooms.Count > 0 && pendingPlayers.Count > 1)
			{
				//Log.Information("配對玩家");
				var matching = new PlayerMatching();
				pendingPlayers.Sort(matching);

				int idxOffset = 0;

				do
				{
					if (Math.Abs(pendingPlayers[idxOffset].Account.Score - pendingPlayers[idxOffset + 1].Account.Score) >= 10)
					{
						++idxOffset;
					}
					else
					{
						Log.Information("配對一組成功");
						GameRoom room = _availableRooms[0];
						
						room.AddPlayer(pendingPlayers[idxOffset]);
						pendingPlayers.RemoveAt(idxOffset);

						room.AddPlayer(pendingPlayers[idxOffset]);
						pendingPlayers.RemoveAt(idxOffset);

						ThreadPool.QueueUserWorkItem(StartGame, room);

						_gamingRooms.Add(room);
						_availableRooms.RemoveAt(0);
					}

					Thread.Sleep(1);
				}
				while (_availableRooms.Count > 0 && (pendingPlayers.Count - idxOffset) > 1);
			}

			//pendingPlayers.Clear();
		}

		private void StartGame(object room)
		{
			GameRoom gameRoom = room as GameRoom;
			gameRoom.GameStart();
		}
	}
}
