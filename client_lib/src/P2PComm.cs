using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using kevincastejon;

namespace BombPeliLib
{
	public enum Channel : byte
	{
		UNKNOWN = 0x00,
		DEFAULT = 0x01,
		MANAGEMENT = 0x02,
		GAME = 0x03
	};

	public class P2PComm
	{

		private readonly UDPManager udpm;
		public int Port {
			get; private set;
		}

		public P2PComm (int port) {
			Port = port;
			udpm = new UDPManager (port);

			udpm.On<UDPManagerEvent> (UDPManagerEvent.Names.BOUND, UDPManagerBoundHandler);
			udpm.On<UDPManagerEvent> (UDPManagerEvent.Names.DATA_CANCELED, DataCancelledHandler);
			udpm.On<UDPManagerEvent> (UDPManagerEvent.Names.DATA_DELIVERED, DataDeliveredHandler);
			udpm.On<UDPManagerEvent> (UDPManagerEvent.Names.DATA_RECEIVED, DataReceivedHandler);
			udpm.On<UDPManagerEvent> (UDPManagerEvent.Names.DATA_RETRIED, DataRetriedHandler);
			udpm.On<UDPManagerEvent> (UDPManagerEvent.Names.DATA_SENT, DataSentHandler);

			udpm.AddChannel(Channel.UNKNOWN.ToString(), true, true, 50, 1000);
			udpm.AddChannel(Channel.DEFAULT.ToString(), true, true, 50, 1000);
			udpm.AddChannel(Channel.MANAGEMENT.ToString(), true, true, 50, 1000);
			udpm.AddChannel(Channel.GAME.ToString(), true, true, 50, 1000);
		}

		public event EventHandler<P2PCommEventArgs> UDPManagerBound;
		public event EventHandler<P2PCommEventArgs> DataCancelled;
		public event EventHandler<P2PCommEventArgs> DataReceived;
		public event EventHandler<P2PCommEventArgs> DataDelivered;
		public event EventHandler<P2PCommEventArgs> DataRetried;
		public event EventHandler<P2PCommEventArgs> DataSent;

		public void Send (Channel channel, object data, string address, int port) {
			udpm.Send (channel.ToString (), data, address, port);
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
