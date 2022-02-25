namespace BombPeliLib
{
	static internal class P2PConstants
	{
		
		internal const byte PASS_BOMB   = 0x01;	// "pass_bomb";
		internal const byte LOSE        = 0x02;	// "lose";
		internal const byte JOIN        = 0x03;	// "join";
		internal const byte QUIT        = 0x04;	// "quit";
		internal const byte LIST_PEERS  = 0x05;	// "list_peers";
		internal const byte PEERS       = 0x06;	// "peers";
		internal const byte PEER_JOINED = 0x07;	// "peer_joined";
		internal const byte PEER_QUIT   = 0x08;	// "peer_quit";
		internal const byte START       = 0x09;	// "start";
	}
}
