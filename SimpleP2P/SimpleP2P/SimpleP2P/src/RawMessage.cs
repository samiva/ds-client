using System;

namespace SimpleP2P
{
	
	
	internal class RawMessage
	{
		private const long INVALID_ID          = -1;

		readonly private MsgType    type;
		readonly public  MsgContent msg;
		readonly public  long       id;
		readonly public  byte[]     data;

		public RawMessage (MsgType type, MsgContent msg, long id, byte[]? data = null) {
			this.type = type;
			this.msg  = msg;
			this.id   = id;
			this.data = data ?? Array.Empty<byte> ();
		} 

		public RawMessage (byte[] rawData) {
			int length = rawData.Length;
			if (length < 10) {
				throw new Exception ();
			}
			this.type = (MsgType)rawData [0];
			this.msg  = (MsgContent)rawData [1];
			this.id   = this.parseId (rawData);
			if (length > 10) {
				this.data = new byte[length - 10];
				Array.Copy (rawData, 10, this.data, 0, length - 10);
			} else {
				this.data = Array.Empty<byte> ();
			}
		}

		public bool isAck () {
			return (this.type & MsgType.MSG_TYPE_ACK_FLAG) == MsgType.MSG_TYPE_ACK_FLAG;
		}

		public MsgType getType () {
			return (this.type & MsgType.MSG_TYPE_MASK);
		}

		public byte[] make () {
			int    length  = this.data.Length;
			byte[] bytes   = new byte[length + 10];
			byte[] idBytes = BitConverter.GetBytes (this.id);
			if (length != 0) {
				Array.Copy (this.data, 0, bytes, 10, length);
			}
			bytes [0] = (byte)this.type;
			bytes [1] = (byte)this.msg;
			bytes [2] = idBytes [0];
			bytes [3] = idBytes [1];
			bytes [4] = idBytes [2];
			bytes [5] = idBytes [3];
			bytes [6] = idBytes [4];
			bytes [7] = idBytes [5];
			bytes [8] = idBytes [6];
			bytes [9] = idBytes [7];
			return bytes;
		}
		
		private long parseId (byte[] msg) {
			if (msg.Length < 10) {
				return INVALID_ID;
			}
			return BitConverter.ToInt64 (msg, 2);
		}
		
	}
}
