using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using Texas.Protocol;
using System.IO;
using System.Security.Cryptography;
using Google.Protobuf;
using System.Threading;
using GameService.Protocol;

namespace Example2
{
    public class Echo : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            var protoData = ProtoWrapGo.Parser.ParseFrom(e.RawData);

            ProtoWrapGo protoWrapGo = new ProtoWrapGo();
            protoWrapGo.Op = protoData.Op;
            protoWrapGo.Seq = protoData.Seq;

            switch (protoData.Command)
            {
                case "EnterRoomREQ":
                    UserBrief p1 = new UserBrief();
                    p1.Uid = 123;
                    p1.Name = "mmm";
                    p1.IconUrl = "001";
                    SeatStatus ss1 = new SeatStatus();
                    ss1.Seatid = 2;
                    ss1.Player = p1;
                    ss1.HandChips = 560;
                    ss1.DestopChips = 780;
                    ss1.HasCard = true;

                    UserBrief p2 = new UserBrief();
                    p2.Uid = 2345;
                    p2.Name = "kkk";
                    p2.IconUrl = "001";
                    SeatStatus ss2 = new SeatStatus();
                    ss2.Seatid = 5;
                    ss2.Player = p2;
                    ss2.HandChips = 325;
                    ss2.DestopChips = 885;
                    ss2.HasCard = false;

                    TableStatus table = new TableStatus();
                    table.Pool.Add(300);
                    table.Gameid = "test table";
                    table.CurBlind = 2000;
                    table.Seat.Add(ss1);
                    table.Seat.Add(ss2);
                    table.DIdx = 5;

                    RoomInfo roomInfo = new RoomInfo();
                    roomInfo.ActionTime = 10000;
                    roomInfo.Blind = 400;
                    roomInfo.Ante = 300;

                    PlayingStatus ps = new PlayingStatus();
                    ps.Cards.Add(1);
                    ps.Cards.Add(3);
                    ps.Cards.Add(50);
                    ps.ActionSeatid = 2;
                    ps.ActionTime = 5000;

                    EnterRoomRSP enterRoomRSP = new EnterRoomRSP();
                    enterRoomRSP.Roomid = 100;
                    enterRoomRSP.TableStatus = table;
                    enterRoomRSP.PlayingStatus = ps;
                    enterRoomRSP.RoomInfo = roomInfo;

                    protoWrapGo.Command = "EnterRoomRSP";
                    protoWrapGo.Body = enterRoomRSP.ToByteString();
                    break;
                case "LeaveRoomREQ":
                    LeaveRoomRSP leaveRoomRSP = new LeaveRoomRSP();
                    protoWrapGo.Command = "LeaveRoomRSP";
                    protoWrapGo.Body = leaveRoomRSP.ToByteString();
                    break;
            }
            Console.WriteLine(protoWrapGo.Command);
            var protoBytes = protoWrapGo.ToByteArray();
            Send(protoBytes);
        }
    }
}
