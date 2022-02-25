using System.Text.Json;

namespace BombPeliLib.Msgs
{
	public class Msg
	{

		public byte    msg  { get; set; }
		public object? data { get; set; }

		public Msg (byte msg, object? data = null) {
			this.msg  = msg;
			this.data = data;
		}

	}
}
