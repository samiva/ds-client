# UDPManager

### The UDPManager package offers a event-driven framework on top of UDP with many reliability options, peer-to-peer communication, server and client connection features, and much more.

## Sources
[https://github.com/kevincastejon/cs-udp-manager](https://github.com/kevincastejon/cs-udp-manager)

## Nuget package
[https://www.nuget.org/packages/UDPManager/](https://www.nuget.org/packages/UDPManager/)

## Documentation
[https://github.com/kevincastejon/cs-udp-manager/tree/master/Documentation/html](https://github.com/kevincastejon/cs-udp-manager/tree/master/Documentation/html)

## Usages:

### Peer-to-peer usage:
```
//Instantiate UDPManager and bind on the port of your choice
UDPManager udpm = new UDPManager(9876);

//Add listeners on the instance of UDPManager
udpm.On<UDPManagerEvent>(UDPManagerEvent.Names.BOUND, UDPManagerHandler);
udpm.On<UDPManagerEvent>(UDPManagerEvent.Names.DATA_CANCELED, UDPManagerHandler);
udpm.On<UDPManagerEvent>(UDPManagerEvent.Names.DATA_DELIVERED, UDPManagerHandler);
udpm.On<UDPManagerEvent>(UDPManagerEvent.Names.DATA_RECEIVED, UDPManagerHandler);
udpm.On<UDPManagerEvent>(UDPManagerEvent.Names.DATA_RETRIED, UDPManagerHandler);
udpm.On<UDPManagerEvent>(UDPManagerEvent.Names.DATA_SENT, UDPManagerHandler);

//Add a UDPChannel
udpm.AddChannel("mainChannel", true, true, 50, 1000);

//Send a message to the target IP and port
udpm.Send("mainChannel", new { msg = "Hello!" }, "x.x.x.x", 6789);

private void UDPManagerHandler(UDPManagerEvent e){
//Monitor UDPManagerEvents
Console.WriteLine(e.Name);

    if(e.name==UDPManagerEvent.Names.DATA_RECEIVED){
    //Display received messages
    Console.WriteLine(e.UdpDataInfo.Data);
    }
}

```

### Client-server usage:

Client:

```
class UDPClientTester
{
    private UDPClient client = new UDPClient();
    public UDPClientTester(string serverIP, int serverPort,int localPort=0)
    {
        client.AddEventListener<UDPManagerEvent>(UDPManagerEvent.Names.BOUND, UDPManagerHandler);
        client.AddEventListener<UDPClientEvent>(UDPClientEvent.Names.CONNECTED_TO_SERVER, ClientHandler);
        client.AddEventListener<UDPClientEvent>(UDPClientEvent.Names.CONNECTION_FAILED, ClientHandler);
        client.AddEventListener<UDPClientEvent>(UDPClientEvent.Names.SERVER_PONG, ClientHandler);
        client.AddEventListener<UDPClientEvent>(UDPClientEvent.Names.SERVER_SENT_DATA, ClientHandler);
        client.AddEventListener<UDPClientEvent>(UDPClientEvent.Names.SERVER_TIMED_OUT, ClientHandler);
        client.AddChannel("mainChannel", false, true, 50, 1000);
        client.Connect(serverIP, serverPort, localPort);
    }
    private void ClientHandler(UDPClientEvent e)
    {
        //Console.WriteLine("clientside event: "+e.Name);
        if (e.Name == UDPClientEvent.Names.CONNECTED_TO_SERVER.ToString())
        {
            client.SendToServer("mainChannel", new { message = "Thanks for accepting my connection !" });
        }
        else if (e.Name == UDPClientEvent.Names.SERVER_SENT_DATA.ToString())
        {
            Console.WriteLine("Server sent : " + e.UDPdataInfo.Data["message"]);
        }
    }
    private void UDPManagerHandler(UDPManagerEvent e)
    {
        Console.WriteLine(e.Name);
    }
}

```

Server:

```
class UDPServerTester
{
     UDPServer server = new UDPServer();
     public UDPServerTester(int localPort)
     {
         server.AddEventListener<UDPManagerEvent>(UDPManagerEvent.Names.BOUND, UDPManagerHandler);
         server.AddEventListener<UDPServerEvent>(UDPServerEvent.Names.CLIENT_CONNECTED, ServerHandler);
         server.AddEventListener<UDPServerEvent>(UDPServerEvent.Names.CLIENT_PONG, ServerHandler);
         server.AddEventListener<UDPServerEvent>(UDPServerEvent.Names.CLIENT_RECONNECTED, ServerHandler);
         server.AddEventListener<UDPServerEvent>(UDPServerEvent.Names.CLIENT_SENT_DATA, ServerHandler);
         server.AddEventListener<UDPServerEvent>(UDPServerEvent.Names.CLIENT_TIMED_OUT, ServerHandler);
         server.AddChannel("mainChannel",false,true,50,1000);
         server.Start(localPort);
     }
     private void ServerHandler(UDPServerEvent e)
     {
         if(e.Name == UDPServerEvent.Names.CLIENT_CONNECTED.ToString())
         {
             Console.WriteLine("A client is connected! ID:" + e.UDPpeer.ID.ToString());   
         }
         else if (e.Name == UDPServerEvent.Names.CLIENT_SENT_DATA.ToString())
         {
             Console.WriteLine("Client sent : " + e.UDPdataInfo.Data["message"]);
             server.SendToClient("mainChannel",new { message="You're welcome!"},e.UDPpeer);
         }
     }
     private void UDPManagerHandler(UDPManagerEvent e)
     {
         Console.WriteLine(e.Name);
         
     }
}

```