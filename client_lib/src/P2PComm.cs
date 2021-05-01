﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MonoTorrent; 
using MonoTorrent.Client;

namespace BombPeliLib
{
	public enum Channel : byte
	{
		UNKNOWN = 0x00,
		DEFAULT = 0x01,
		MANAGEMENT = 0x02,
		GAME = 0x03
	};

	public class P2PComm {

		private ClientEngine clientEngine;

		public int Port {
			get; private set;
		}

		public P2PComm (int port) {
			Port = port;
			var settings = new EngineSettingsBuilder { ListenPort = port };
			clientEngine = new ClientEngine(settings.ToSettings());
			
			
		}

		public event EventHandler<P2PCommEventArgs> UDPManagerBound;
		public event EventHandler<P2PCommEventArgs> DataCancelled;
		public event EventHandler<P2PCommEventArgs> DataReceived;
		public event EventHandler<P2PCommEventArgs> DataDelivered;
		public event EventHandler<P2PCommEventArgs> DataRetried;
		public event EventHandler<P2PCommEventArgs> DataSent;

		public void Send (Channel channel, object data, string address, int port) {
			//udpm.Send (Enum.GetName<Channel>(channel), data, address, port);
			clientEngine.
		}

		public void Close () {

			clientEngine.StopAllAsync();


		}

		private void UDPManagerBoundHandler (UDPManagerEvent e) {
			UDPManagerBound?.Invoke (this, (P2PCommEventArgs)e);
		}

		private void DataReceivedHandler (UDPManagerEvent e) {
			DataReceived?.Invoke (this, (P2PCommEventArgs)e);
		}

		private void DataDeliveredHandler (UDPManagerEvent e) {
			DataDelivered?.Invoke (this, (P2PCommEventArgs)e);
		}

		private void DataCancelledHandler (UDPManagerEvent e) {
			DataCancelled?.Invoke (this, (P2PCommEventArgs)e);
		}

		private void DataRetriedHandler (UDPManagerEvent e) {
			DataRetried?.Invoke (this, (P2PCommEventArgs)e);
		}

		private void DataSentHandler (UDPManagerEvent e) {
			DataSent?.Invoke (this, (P2PCommEventArgs)e);
		}

	}
}
