using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using Texas.Protocol;
using System.IO;
using System.Security.Cryptography;
using Google.Protobuf;

namespace Example2
{
    public class Echo : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            EnterRoomRSP enterRoomRSP = new EnterRoomRSP();
            enterRoomRSP.Roomid = 5;

            byte[] bytes = null;
            using (MemoryStream rspStream = new MemoryStream())
            {
                enterRoomRSP.WriteTo(rspStream);
                bytes = rspStream.ToArray();
            }

            Send(bytes);
        }
    }
}
