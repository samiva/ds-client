using System.ComponentModel;

namespace BombPeli
{
	public class PeerInfoView : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		private string peer;
		private ushort port;

		public string Peer {
			get {
				return this.peer;
			}
			set {
				if (value != this.peer) {
					this.peer = value;
					OnPropertyChanged (nameof (Peer));
				}
			}
		}

		public ushort Port {
			get {
				return this.port;
			}
			set {
				if (value != this.port) {
					this.port = value;
					OnPropertyChanged (nameof (Port));
				}
			}
		}

		private void OnPropertyChanged (string? propertyName) {
			this.PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
		}
	}
}
