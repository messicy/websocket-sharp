using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using Texas.Protocol;
using System.IO;
using System.Security.Cryptography;
using Google.Protobuf;
using System.Threading;

namespace Example2
{
    public class Echo : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine("OnMessage " + e.Data);
            byte[] bytes = null;
            switch (e.Data)
            {
                case "1":
                    EnterRoomRSP enterRoomRSP = new EnterRoomRSP();
                    enterRoomRSP.Roomid = 5;

                    using (MemoryStream rspStream = new MemoryStream())
                    {
                        enterRoomRSP.WriteTo(rspStream);
                        bytes = rspStream.ToArray();
                    }
                    break;
                case "2":
                    EnterRoomRSP enterRoomRSP2 = new EnterRoomRSP();
                    enterRoomRSP2.Roomid = 10;

                    using (MemoryStream rspStream = new MemoryStream())
                    {
                        enterRoomRSP2.WriteTo(rspStream);
                        bytes = rspStream.ToArray();
                    }
                    break;
            }

            Send(bytes);
        }
    }
}
