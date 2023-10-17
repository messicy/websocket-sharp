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
                    showHandInfo1.Cards.Add("8s");
                    showHandInfo1.Cards.Add("3d");
                    ShowHandInfo showHandInfo2 = new ShowHandInfo();
                    showHandInfo2.Seatid = 5;
                    showHandInfo2.Cards.Add("4c");
                    showHandInfo2.Cards.Add("7s");

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
            int simulation = -1;
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
                    p1.IconUrl = "1";
                    SeatStatus ss1 = new SeatStatus();
                    ss1.Seatid = 2;
                    ss1.Player = p1;
                    ss1.HandChips = 560;
                    //ss1.DestopChips = 780;
                    ss1.HasCard = true;

                    UserBrief p2 = new UserBrief();
                    p2.Uid = 2345;
                    p2.Name = "kkk";
                    p2.IconUrl = "3";
                    SeatStatus ss2 = new SeatStatus();
                    ss2.Seatid = 5;
                    ss2.Player = p2;
                    ss2.HandChips = 325;
                    //ss2.DestopChips = 885;
                    ss2.HasCard = true;

                    UserBrief p3 = new UserBrief();
                    p3.Uid = 5623;
                    p3.Name = "sss";
                    p3.IconUrl = "4";
                    SeatStatus ss3 = new SeatStatus();
                    ss3.Seatid = 7;
                    ss3.Player = p3;
                    ss3.HandChips = 456;
                    //ss3.DestopChips = 251;
                    ss3.HasCard = true;

                    TableStatus table = new TableStatus();
                    table.Pool.Add(300);
                    table.Gameid = "111-3564-568";
                    table.CurBlind = 2000;
                    table.Seat.Add(ss1);
                    table.Seat.Add(ss2);
                    table.Seat.Add(ss3);
                    table.DIdx = 5;

                    RoomInfo roomInfo = new RoomInfo();
                    roomInfo.ActionTime = 10000;
                    roomInfo.Blind = 600;
                    roomInfo.Ante = 300;

                    //PlayingStatus ps = new PlayingStatus();
                    //ps.Cards.Add("6s");
                    //ps.Cards.Add("Td");
                    //ps.Cards.Add("As");
                    //ps.ActionSeatid = 2;
                    //ps.ActionTime = 5000;

                    EnterRoomRSP enterRoomRSP = new EnterRoomRSP();
                    enterRoomRSP.Roomid = 100;
                    enterRoomRSP.TableStatus = table;
                    //enterRoomRSP.PlayingStatus = ps;
                    enterRoomRSP.RoomInfo = roomInfo;

                    protoWrapGo.Command = "EnterRoomRSP";
                    protoWrapGo.Body = enterRoomRSP.ToByteString();
                    simulation = 1;
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
                    simulation = 2;
                    break;
            }
            Console.WriteLine(protoWrapGo.Command);
            var protoBytes = protoWrapGo.ToByteArray();
            Send(protoBytes);

            if (simulation == 1)
            {
                StartSimulation1();
            }
            else if (simulation == 2)
            {
                StartSimulation2();
            }
        }

        private void StartSimulation1()
        {
            Thread.Sleep(500);
            broadcastSequence++;
            ProtoWrapGo protoWrapGo = new ProtoWrapGo();
            protoWrapGo.Op = MessageType.MessageBroadcast;
            protoWrapGo.Seq = broadcastSequence;

            protoWrapGo.Command = "HandCardRSP";
            HandCardRSP handcardBRC = new HandCardRSP();
            handcardBRC.Cards.Add("Ac");
            handcardBRC.Cards.Add("5h");
            protoWrapGo.Body = handcardBRC.ToByteString();
            var protoBytes = protoWrapGo.ToByteArray();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoBytes);

            Thread.Sleep(3 * 1000);
            protoWrapGo.Command = "ActionNotifyBRC";
            ActionNotifyBRC actionNotifyBRC = new ActionNotifyBRC();
            actionNotifyBRC.Seatid = 2;
            actionNotifyBRC.LeftTime = 10000;
            protoWrapGo.Body = actionNotifyBRC.ToByteString();
            protoBytes = protoWrapGo.ToByteArray();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoBytes);

            Thread.Sleep(3 * 1000);
            protoWrapGo.Command = "ActionBRC";
            ActionBRC actionBRC = new ActionBRC();
            actionBRC.Seatid = 2;
            actionBRC.ActionType = ActionType.ActionBet;
            actionBRC.Chips = 100;
            actionBRC.HandChips = 460;
            protoWrapGo.Body = actionBRC.ToByteString();
            protoBytes = protoWrapGo.ToByteArray();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoBytes);

            Thread.Sleep(500);
            protoWrapGo.Command = "ActionNotifyBRC";
            actionNotifyBRC = new ActionNotifyBRC();
            actionNotifyBRC.Seatid = 5;
            actionNotifyBRC.Actions.Add(ActionType.ActionFold);
            actionNotifyBRC.Actions.Add(ActionType.ActionRaise);
            actionNotifyBRC.Actions.Add(ActionType.ActionCall);
            actionNotifyBRC.Call = 100;
            actionNotifyBRC.LeftTime = 10000;
            protoWrapGo.Body = actionNotifyBRC.ToByteString();
            protoBytes = protoWrapGo.ToByteArray();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoBytes);

            //Thread.Sleep(3 * 1000);
            //protoWrapGo.Command = "ActionBRC";
            //actionBRC = new ActionBRC();
            //actionBRC.Seatid = 5;
            //actionBRC.ActionType = ActionType.ActionCall;
            //actionBRC.Chips = 100;
            //actionBRC.HandChips = 460;
            //protoWrapGo.Body = actionBRC.ToByteString();
            //protoBytes = protoWrapGo.ToByteArray();
            //Console.WriteLine(protoWrapGo.Command);
            //Sessions.Broadcast(protoBytes);
        }

        private void StartSimulation2()
        {
            Console.WriteLine("StartSimulation2");
        }
    }
}
