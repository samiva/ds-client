using System.Net;

namespace SimpleP2P
{
	internal class Message
	{
		
		readonly public IPEndPoint peer;
		readonly public long       time;
		readonly public RawMessage msg;

		public Message (IPEndPoint peer, long time, RawMessage msg) {
			this.peer = peer;
			this.time = time;
			this.msg  = msg;
		}

	}
}
