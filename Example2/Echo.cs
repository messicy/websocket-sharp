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
            if (e.IsBinary)
            {
                HandleProto(e);
            }
            else
            {
                HandleBroadcastTest(e);
            }
        }

        int broadcastSequence = 0;
        private void HandleBroadcastTest(MessageEventArgs e)
        {
            broadcastSequence++;
            ProtoWrapGo protoWrapGo = new ProtoWrapGo();
            protoWrapGo.Op = MessageType.MessageBroadcast;
            protoWrapGo.Seq = broadcastSequence;
            switch (e.Data)
            {
                case "1":
                    protoWrapGo.Command = "RoundStartBRC";
                    RoundStartBRC roundStartBRC = new RoundStartBRC();
                    roundStartBRC.CurBlind = 3;
                    protoWrapGo.Body = roundStartBRC.ToByteString();
                    break;
                case "2":
                    protoWrapGo.Command = "ActionBRC";

                    ActionBRC actionBRC = new ActionBRC();
                    actionBRC.Seatid = 2;
                    actionBRC.ActionType = ActionType.ActionCall;
                    actionBRC.Chips = 100;
                    actionBRC.HandChips = 460;
                    protoWrapGo.Body = actionBRC.ToByteString();
                    break;
                case "3":
                    protoWrapGo.Command = "ShowHandRSP";

                    ShowHandInfo showHandInfo1 = new ShowHandInfo();
                    showHandInfo1.Seatid = 2;
                    showHandInfo1.Cards.Add(51);
                    showHandInfo1.Cards.Add(52);
                    ShowHandInfo showHandInfo2 = new ShowHandInfo();
                    showHandInfo2.Seatid = 5;
                    showHandInfo2.Cards.Add(3);
                    showHandInfo2.Cards.Add(49);

                    ShowHandRSP showhandBRC = new ShowHandRSP();
                    showhandBRC.Info.Add(showHandInfo1);
                    showhandBRC.Info.Add(showHandInfo2);
                    protoWrapGo.Body = showhandBRC.ToByteString();
                    break;
                case "4":
                    protoWrapGo.Command = "WinnerRSP";

                    WinningInfo winInfo1 = new WinningInfo();
                    winInfo1.Seatid = 2;
                    winInfo1.Poolid = 1;
                    winInfo1.Chips = 200;
                    WinningInfo winInfo2 = new WinningInfo();
                    winInfo2.Seatid = 5;
                    winInfo2.Poolid = 2;
                    winInfo2.Chips = 300;

                    WinnerRSP winnerRSP = new WinnerRSP();
                    winnerRSP.Winner.Add(winInfo1);
                    winnerRSP.Winner.Add(winInfo2);
                    protoWrapGo.Body = winnerRSP.ToByteString();
                    break;
                case "5":
                    protoWrapGo.Command = "ActionNotifyBRC";

                    ActionNotifyBRC actionNotifyBRC = new ActionNotifyBRC();
                    actionNotifyBRC.Seatid = 5;
                    actionNotifyBRC.Actions.Add(ActionType.ActionFold);
                    actionNotifyBRC.Actions.Add(ActionType.ActionRaise);
                    actionNotifyBRC.Actions.Add(ActionType.ActionCall);
                    actionNotifyBRC.Call = 128;
                    actionNotifyBRC.LeftTime = 10000;
                    protoWrapGo.Body = actionNotifyBRC.ToByteString();
                    break;
                case "6":
                    protoWrapGo.Command = "DealerInfoRSP";

                    DealerInfoRSP dealerInfoRSP = new DealerInfoRSP();
                    dealerInfoRSP.Dealer = 6;
                    dealerInfoRSP.SmallBlind = 5;
                    dealerInfoRSP.Gameid = "2222";
                    protoWrapGo.Body = dealerInfoRSP.ToByteString();
                    break;
                default: break;
            }
            Console.WriteLine(protoWrapGo.Command);
            var protoBytes = protoWrapGo.ToByteArray();
            Sessions.Broadcast(protoBytes);
        }

        private void HandleProto(MessageEventArgs e)
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
                    table.Pool.Add(200);
                    table.Pool.Add(111);
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
                case "ActionREQ":
                    protoWrapGo.Command = "ActionBRC";

                    ActionBRC actionBRC = new ActionBRC();
                    actionBRC.Seatid = 5;
                    actionBRC.ActionType = ActionType.ActionFold;
                    actionBRC.Chips = 55;
                    actionBRC.HandChips = 66;
                    protoWrapGo.Body = actionBRC.ToByteString();
                    break;
            }
            Console.WriteLine(protoWrapGo.Command);
            var protoBytes = protoWrapGo.ToByteArray();
            Send(protoBytes);
        }
    }
}
