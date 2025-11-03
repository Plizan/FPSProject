using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Utils;
using Google.Protobuf;
using ProtoPacket;

public class Player
{
	public NetPeer peer;
	public PlayerState lastPlayerState;
}

public class Server : INetEventListener
{
	private NetManager _server;
	private readonly Dictionary<int, Player> _players = new();
	private readonly Lock _sync = new();
	private readonly NetDataWriter _sharedWriter = new();
	public const int port = 1234;
	public const float pollDelay = .01f;

	public void Run()
	{
		_server = new(this);
		_server.Start(port);
		Console.WriteLine("Server started\n" +
		                  $"Server listening port on {_server.LocalPort}");

		while (true)
		{
			_server.PollEvents();
			Thread.Sleep((int)(pollDelay * 1000));
		}
	}

	private void OnPlayer(PlayerState playerState, NetPeer peer)
	{
		lock (_sync)
		{
			if (_players.TryGetValue(peer.Id, out var player) == false)
				return;

			playerState.Id = peer.Id;
			player.lastPlayerState = playerState;

			SendToAll(PacketId.PacketPlayer, playerState, peer.Id);
			SendPacket(peer, PacketId.PacketLocalPlayer, playerState, DeliveryMethod.ReliableSequenced);
		}
	}

	public void OnPeerConnected(NetPeer peer)
	{
		lock (_sync)
		{
			_players[peer.Id] = new() { peer = peer };
			SendPacket(peer, PacketId.PacketLocalPlayer, new()
			{
				Id = peer.Id,
			}, DeliveryMethod.ReliableOrdered);
			Console.WriteLine($"Peer {peer.Id} connected.");
		}
	}

	private void SendPacket(NetPeer peer, PacketId id, PlayerState playerState, DeliveryMethod method)
	{
		lock (_sync)
		{
			_sharedWriter.Reset();
			_sharedWriter.Put((byte)id);
			_sharedWriter.Put(playerState.ToByteArray());
			peer.Send(_sharedWriter, method);
		}
	}

	private void SendToAll(PacketId packetID, PlayerState state, int excludeID)
	{
		lock (_sync)
		{
			foreach (var (id, player) in _players)
			{
				if (id == excludeID)
					continue;

				SendPacket(player.peer, packetID, state, DeliveryMethod.ReliableSequenced);
			}
		}
	}

	public void OnPeerDisconnected(NetPeer peer, DisconnectInfo info)
	{
		lock (_sync)
		{
			SendToAll(PacketId.PacketPlayer, new()
			{
				Id = peer.Id,
				State = PlayerState.Types.State.Out
			}, peer.Id);
			_players.Remove(peer.Id);

			Console.WriteLine($"Peer {peer.Id} disconnected: {info.Reason}");
		}
	}

	public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod method)
	{
		lock (_sync)
		{
			var pid = (PacketId)reader.GetByte();
			var payload = reader.GetRemainingBytes();
			switch (pid)
			{
				case PacketId.PacketPlayer:
				{
					var playerState = PlayerState.Parser.ParseFrom(payload);
					OnPlayer(playerState, peer);
					break;
				}
				case PacketId.PacketSanity:
				{
					break;
				}
				case PacketId.PacketDamage:
				{
					var takeDamage = TakeDamage.Parser.ParseFrom(payload);
					var player = _players.GetValueOrDefault(takeDamage.TargetID);

					if (player == null)
						break;

					var playerState = player.lastPlayerState;
					playerState.Hp -= takeDamage.Damage;
					OnPlayer(playerState, player.peer);
					break;
				}
			}
			reader.Recycle();
		}
	}

	public void OnConnectionRequest(ConnectionRequest request) => request.AcceptIfKey("9yQLpB9nxdyanQBZ1zqwWLinUXfOFoN0aljyDMPiZIeJS80ZiE");

	public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
	{
	}

	public void OnNetworkError(IPEndPoint ep, System.Net.Sockets.SocketError error)
	{
	}

	public void OnNetworkReceiveUnconnected(IPEndPoint ep, NetPacketReader r, UnconnectedMessageType t)
	{
	}
}