using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombPeli
{
	public class GameInfoView : INotifyPropertyChanged
	{

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

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged (string? propertyName) {
			PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
		}
	}
}
