using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using kevincastejon;

namespace foobar
{
	class Program
	{

		static void Main (string[] args) {

			UDPManager asd = new UDPManager(12313);

			asd.On<UDPManagerEvent> (UDPManagerEvent.Names.BOUND, UDPManagerBoundHandler);
			asd.On<UDPManagerEvent> (UDPManagerEvent.Names.DATA_CANCELED, DataCancelledHandler);
			asd.On<UDPManagerEvent> (UDPManagerEvent.Names.DATA_DELIVERED, DataDeliveredHandler);
			asd.On<UDPManagerEvent> (UDPManagerEvent.Names.DATA_RECEIVED, DataReceivedHandler);
			asd.On<UDPManagerEvent> (UDPManagerEvent.Names.DATA_RETRIED, DataRetriedHandler);
			asd.On<UDPManagerEvent> (UDPManagerEvent.Names.DATA_SENT, DataSentHandler);

			asd.AddChannel ("asd", true, true);

			asd.Send ("asd", new object (), "127.0.0.1", 29798);
			while (true);
		}

		static private void UDPManagerBoundHandler (UDPManagerEvent e) {

		}

		static private void DataReceivedHandler (UDPManagerEvent e) {

		}

		static private void DataDeliveredHandler (UDPManagerEvent e) {

		}

		static private void DataCancelledHandler (UDPManagerEvent e) {
			
		}

		static private void DataRetriedHandler (UDPManagerEvent e) {

		}

		static private void DataSentHandler (UDPManagerEvent e) {

		}
	}
}
