﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using kevincastejon;

namespace BombPeli
{
    public enum Channel : byte
    {
        DEFAULT =   0x01,
        MANAGEMENT = 0x02,
        GAME = 0x03
    };
    class P2PComm
    {
            
        private UDPManager udpm;
        public int Port { get; private set; }
        public P2PComm(int port)
        {
            Port = port;
            udpm = new UDPManager(port);

            udpm.On<UDPManagerEvent>(UDPManagerEvent.Names.BOUND, UDPManagerBoundHandler);
            udpm.On<UDPManagerEvent>(UDPManagerEvent.Names.DATA_CANCELED, DataCancelledHandler);
            udpm.On<UDPManagerEvent>(UDPManagerEvent.Names.DATA_DELIVERED, DataDeliveredHandler);
            udpm.On<UDPManagerEvent>(UDPManagerEvent.Names.DATA_RECEIVED, DataReceivedHandler);
            udpm.On<UDPManagerEvent>(UDPManagerEvent.Names.DATA_RETRIED, DataRetriedHandler);
            udpm.On<UDPManagerEvent>(UDPManagerEvent.Names.DATA_SENT, DataSentHandler);
        }

        public event EventHandler<P2PCommEventArgs> UDPManagerBound;
        public event EventHandler<P2PCommEventArgs> DataCancelled;
        public event EventHandler<P2PCommEventArgs> DataReceived;
        public event EventHandler<P2PCommEventArgs> DataDelivered;
        public event EventHandler<P2PCommEventArgs> DataRetried;
        public event EventHandler<P2PCommEventArgs> DataSent;

        public void Send(Channel channel,object data, string address, int port)
        {
            udpm.Send(channel.ToString(), data, address, port);
        }
               

        private void UDPManagerBoundHandler(UDPManagerEvent e)
        {
            P2PCommEventArgs p2peventargs = P2PCommEventArgsFromUDPManagerEvent(e);
            OnUDPManagerBound(p2peventargs);
        }


        private void DataReceivedHandler(UDPManagerEvent e)
        {
            P2PCommEventArgs p2peventargs = P2PCommEventArgsFromUDPManagerEvent(e);
            OnDataReceived(p2peventargs);
        }

        private void DataDeliveredHandler(UDPManagerEvent e)
        {
            P2PCommEventArgs p2peventargs = P2PCommEventArgsFromUDPManagerEvent(e);
            OnDataDelivered(p2peventargs);
        }

        private void DataCancelledHandler(UDPManagerEvent e)
        {
            P2PCommEventArgs p2peventargs = P2PCommEventArgsFromUDPManagerEvent(e);
            OnDataCancelled(p2peventargs);
        }

        private void DataRetriedHandler(UDPManagerEvent e)
        {
            P2PCommEventArgs p2peventargs = P2PCommEventArgsFromUDPManagerEvent(e);
            OnDataRetried(p2peventargs);
        } 

        private void DataSentHandler(UDPManagerEvent e)
        {
            P2PCommEventArgs p2peventargs = P2PCommEventArgsFromUDPManagerEvent(e);
            OnDataSent(p2peventargs);
        }

        private void OnUDPManagerBound(P2PCommEventArgs e)
        {
            EventHandler<P2PCommEventArgs> handler = UDPManagerBound;

            if(handler!= null)
            {
                handler(this, e);
            }
        }

        private void OnDataCancelled(P2PCommEventArgs e)
        {
            EventHandler<P2PCommEventArgs> handler = DataCancelled;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnDataDelivered(P2PCommEventArgs e)
        {
            EventHandler<P2PCommEventArgs> handler = DataDelivered;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnDataReceived(P2PCommEventArgs e)
        {
            EventHandler<P2PCommEventArgs> handler = DataReceived;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnDataRetried(P2PCommEventArgs e)
        {
            EventHandler<P2PCommEventArgs> handler = DataRetried;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnDataSent(P2PCommEventArgs e)
        {
            EventHandler<P2PCommEventArgs> handler = DataSent;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        private P2PCommEventArgs P2PCommEventArgsFromUDPManagerEvent(UDPManagerEvent udpmevent)
        {
            P2PCommEventArgs e = new P2PCommEventArgs();
            e.RemoteAddress = udpmevent.UDPdataInfo.RemoteAddress;
            e.RemotePort = udpmevent.UDPdataInfo.RemotePort;
            e.ID = udpmevent.UDPdataInfo.ID;
            e.Data = udpmevent.UDPdataInfo.Data;
            if (udpmevent.UDPdataInfo.ChannelName.Equals(Channel.MANAGEMENT.ToString()))
                e.MessageChannel = Channel.MANAGEMENT;
            else if (udpmevent.UDPdataInfo.ChannelName.Equals(Channel.GAME.ToString()))
                e.MessageChannel = Channel.GAME;
            return e;
        }
    }
}
