﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityOSC;
using System.Reflection;
using System.Net;

public class OscManager : MonoBehaviour {
	[Serializable]
	public struct DistantOsc {
		public string name;
		public string ip;
		public short port;
	}

	public short localPort = 6006;
	public DistantOsc[] servers;

	private OSCServer localServer;
	private List<OSCClient> localClients = new List<OSCClient>();

	public DistantOsc getOscDistantServer(string name) {
		return Array.Find<DistantOsc> (this.servers, s => s.name == name);
	}

	public void sendOscMessage(OSCClient client, string topic, params object[] values) {
		Debug.LogFormat ("sending message on {0}", topic);
		OSCMessage msg = new OSCMessage (topic);
		foreach (object val in values) {
			Type val_type = val.GetType ();
			MethodInfo method = typeof(OSCMessage).GetMethod ("Append");
			MethodInfo generic = method.MakeGenericMethod (val_type);

			object[] valWrapped = new object[1];
			valWrapped [0] = val;
			generic.Invoke (msg, valWrapped);
		}
		client.Send (msg);
	}

	public OSCClient createClient(string server) {
		OscManager.DistantOsc oscDestination = this.getOscDistantServer (server);

		IPAddress ip = IPAddress.Parse(oscDestination.ip);
		OSCClient c = new UnityOSC.OSCClient (ip, (int)(oscDestination.port));
		this.localClients.Add (c);
		return c;
	}

	private void OnPacketReceived(OSCServer server, OSCPacket packet) {
		Debug.LogFormat ("OSC Message received on {0} : {1}",
			packet.Address.ToString(),
			packet.Data.ToString());
	}

	void Awake() {
		this.localServer = new OSCServer (this.localPort);
		Debug.LogFormat ("Server Listening on {0}", this.localPort.ToString ());
		this.localServer.PacketReceivedEvent += this.OnPacketReceived;
	}

	void OnApplicationQuit() 
	{
		this.localServer.Close ();

		foreach(OSCClient c in this.localClients)
		{
			c.Close ();
		}
	}

}
