using System.ComponentModel;

namespace BombPeli
{
	public class GameInfoView : INotifyPropertyChanged
	{

		public event PropertyChangedEventHandler? PropertyChanged;
		
		private string name;
		private ushort port;

		public string Name {
			get {
				return name;
			}
			set {
				if (value != name) {
					name = value;
					OnPropertyChanged (nameof (Name));
				}
			}
		}

		public ushort Port {
			get {
				return port;
			}
			set {
				if (value != port) {
					port = value;
					OnPropertyChanged (nameof (Port));
				}
			}
		}

		private void OnPropertyChanged (string? propertyName) {
			this.PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
		}
	}
}
