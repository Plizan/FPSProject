using System.Collections.Generic;
using System.Net;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;
using Google.Protobuf;
using ProtoPacket;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class NetClient : MonoBehaviour, INetEventListener
{
	public static NetClient Get { get; private set; }

	[SerializeField]
	private string _host = "127.0.0.1";
	[SerializeField]
	private int _port = 1234;
	[SerializeField]
	private float _sendHz = 30f;
	[SerializeField]
	private GameObject _remotePrefab;

	private NetManager _client;
	private NetPeer _netPeer;
	private readonly NetDataWriter _writer = new();

	private long _tick;
	private float _acc;
	private int _id = -1;

	private PlayerCtrl localPlayerCtrl => _Ctrls[_id];

	private readonly Dictionary<int, PlayerCtrl> _Ctrls = new();
	private readonly Dictionary<int, (Vector3, Quaternion)> _targets = new();
	private readonly Dictionary<int, Vector3> _velocities = new();

	private void Awake()
	{
		Application.runInBackground = true;
		Get = this;
	}

	private void Start()
	{
		_client = new(this);

#if UNITY_EDITOR
		_client.DisconnectTimeout = int.MaxValue;
#else
		_client.PingInterval = 1000;
		_client.DisconnectTimeout = 10000;
#endif

		_client.Start();
		_client.Connect(_host, _port, "9yQLpB9nxdyanQBZ1zqwWLinUXfOFoN0aljyDMPiZIeJS80ZiE");
	}

	private void Update()
	{
		_client.PollEvents();
		_acc += Time.deltaTime;

		var step = 1f / _sendHz;
		while (_acc >= step)
		{
			_acc -= step;
			_tick++;
			SendSnap();
		}

		foreach (var (id, ctrl) in _Ctrls)
		{
			if (id == _id)
				continue;

			if (_targets.TryGetValue(id, out var target) == false)
				continue;

			if (_velocities.TryGetValue(id, out var v) == false)
				v = Vector3.zero;

			ctrl.transform.position = Vector3.SmoothDamp(ctrl.transform.position, target.Item1, ref v, 0.05f);
			ctrl.transform.rotation = target.Item2;
			_velocities[id] = v;
		}
	}

	public void SendSnap()
	{
		if (_netPeer == null || _id == -1)
			return;

		_writer.Reset();
		_writer.Put((byte)PacketId.PacketPlayer);

		var bytes = new PlayerState
		{
			Tick = _tick,
			Pos = localPlayerCtrl.transform.position.GetVector3(),
			Rotation = localPlayerCtrl.transform.rotation.GetQuaternion(),
			Hp = localPlayerCtrl.GetCtrlCom<StateCtrlCom>().HP
		}.ToByteArray();
		_writer.Put(bytes);
		_netPeer.Send(_writer, DeliveryMethod.ReliableSequenced);
	}

	private void OnSanity(Sanity sanity)
	{
		if (sanity.Ok == false)
			transform.position = sanity.Pos.GetVector3();
	}

	private void OnRemoteSnap(PlayerState playerState)
	{
		if (playerState.State == PlayerState.Types.State.Out)
		{
			RemoveRemotePlayerObj(playerState.Id);
			return;
		}

		if (playerState.Id == _id)
			return;

		TryGeneratePlayerObj(playerState.Id, false);
		_Ctrls[playerState.Id].GetCtrlCom<StateCtrlCom>().SetHP(playerState.Hp);
		_targets[playerState.Id] = (playerState.Pos.GetVector3(), playerState.Rotation.GetQuaternion());
	}

	public void TakeDamage(TakeDamage takeDamage)
	{
		_writer.Reset();
		_writer.Put((byte)PacketId.PacketDamage);
		_writer.Put(takeDamage.ToByteArray());
		_netPeer.Send(_writer, DeliveryMethod.ReliableOrdered);
	}

	private void TryGeneratePlayerObj(int id, bool isLocal)
	{
		if (_Ctrls.ContainsKey(id))
			return;

		var traSpawn = GameManager.Get.spawnPoints[id % GameManager.Get.spawnPoints.Length];
		var obj = Instantiate(_remotePrefab, traSpawn.position, traSpawn.rotation);
		obj.name = isLocal ? $"Local_{id}" : $"Remote_{id}";

		var playerCtrl = obj.GetComponent<PlayerCtrl>();
		playerCtrl.Initialize(id, isLocal);
		_Ctrls[id] = playerCtrl;
		_velocities[id] = Vector3.zero;
	}

	public void OnPeerConnected(NetPeer peer)
	{
		_netPeer = peer;
	}

	public void OnPeerDisconnected(NetPeer peer, DisconnectInfo info)
	{
		_netPeer = null;
		RemoveRemotePlayerObj(_id);
	}

	private void RemoveRemotePlayerObj(int id)
	{
		if (_Ctrls.TryGetValue(id, out var remote) == false)
			return;

		Destroy(remote.gameObject);
		_Ctrls.Remove(id);
		_targets.Remove(id);
		_velocities.Remove(id);
	}

	public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod method)
	{
		var pid = (PacketId)reader.GetByte();
		var payload = reader.GetRemainingBytes();
		switch (pid)
		{
			case PacketId.PacketLocalPlayer:
			{
				var data = PlayerState.Parser.ParseFrom(payload);
				if (_id == -1)
				{
					_id = data.Id;
					TryGeneratePlayerObj(_id, true);
				}
				else
				{
					localPlayerCtrl.GetCtrlCom<StateCtrlCom>().SetHP(data.Hp);
				}
				break;
			}
			case PacketId.PacketPlayer:
			{
				OnRemoteSnap(PlayerState.Parser.ParseFrom(payload));
				break;
			}
			case PacketId.PacketSanity:
			{
				OnSanity(Sanity.Parser.ParseFrom(payload));
				break;
			}
		}
		reader.Recycle();
	}

	public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
	{
	}

	public void OnConnectionRequest(ConnectionRequest request)
	{
	}

	public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
	{
	}

	public void OnNetworkError(IPEndPoint ep, System.Net.Sockets.SocketError error)
	{
	}

	private void OnDestroy()
	{
		_netPeer?.Disconnect();
		_client?.Stop();
	}
}