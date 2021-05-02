using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkController;
using NetworkController.Interfaces;

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



		INetworkController networkController;
		public int Port {
			get; private set;
		}

		public P2PComm (int port) {
			Port = port;
            //server = new TrackerServer();
            var udpEndPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), Port);
			networkController = new NetworkManagerFactory().Create();
			networkController.StartListening(Port);

        }

        

        public event EventHandler<P2PCommEventArgs> UDPManagerBound;
		public event EventHandler<P2PCommEventArgs> DataCancelled;
		public event EventHandler<P2PCommEventArgs> DataReceived;
		public event EventHandler<P2PCommEventArgs> DataDelivered;
		public event EventHandler<P2PCommEventArgs> DataRetried;
		public event EventHandler<P2PCommEventArgs> DataSent;

		public void Send (Channel channel, object data, string address, int port) {
			//udpm.Send (Enum.GetName<Channel>(channel), data, address, port);
			
		}

		public void Close () {

			//clientEngine.StopAllAsync();


		}

		//private void UDPManagerBoundHandler (UDPManagerEvent e) {
		//	UDPManagerBound?.Invoke (this, (P2PCommEventArgs)e);
		//}

		//private void DataReceivedHandler (UDPManagerEvent e) {
		//	DataReceived?.Invoke (this, (P2PCommEventArgs)e);
		//}

		//private void DataDeliveredHandler (UDPManagerEvent e) {
		//	DataDelivered?.Invoke (this, (P2PCommEventArgs)e);
		//}

		//private void DataCancelledHandler (UDPManagerEvent e) {
		//	DataCancelled?.Invoke (this, (P2PCommEventArgs)e);
		//}

		//private void DataRetriedHandler (UDPManagerEvent e) {
		//	DataRetried?.Invoke (this, (P2PCommEventArgs)e);
		//}

		//private void DataSentHandler (UDPManagerEvent e) {
		//	DataSent?.Invoke (this, (P2PCommEventArgs)e);
		//}

	}
}
