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
        private int actionTime = 10000;
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
            ProtoPacket protoWrapGo = new ProtoPacket();
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
                    ActionData data = new ActionData();
                    data.Actions.Add(ActionType.ActionFold);
                    data.Actions.Add(ActionType.ActionRaise);
                    data.Actions.Add(ActionType.ActionCall);
                    data.Call = 128;
                    data.LeftTime = actionTime;
                    actionNotifyBRC.ActionData = data;

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
            var protoData = ProtoPacket.Parser.ParseFrom(e.RawData);

            ProtoPacket protoWrapGo = new ProtoPacket();
            protoWrapGo.Op = protoData.Op;
            protoWrapGo.Seq = protoData.Seq;

            switch (protoData.Command)
            {
                case "EnterRoomREQ":
                    var reqData = EnterRoomREQ.Parser.ParseFrom(protoData.Body);
                    EnterRoomRSP enterRoomRSP = new EnterRoomRSP();
                    enterRoomRSP.Roomid = 1;

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
                    ss2.Seatid = 1;
                    ss2.Player = p2;
                    ss2.HandChips = 325;
                    //ss2.DestopChips = 885;
                    ss2.HasCard = true;

                    UserBrief p3 = new UserBrief();
                    p3.Uid = 5623;
                    p3.Name = "sss";
                    p3.IconUrl = "4";
                    SeatStatus ss3 = new SeatStatus();
                    ss3.Seatid = 0;
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
                    enterRoomRSP.TableStatus = table;

                    RoomInfo roomInfo = new RoomInfo();
                    roomInfo.ActionTime = actionTime;
                    roomInfo.Blind = 600;
                    roomInfo.Ante = 300;
                    roomInfo.SeatNum = 9;
                    enterRoomRSP.RoomInfo = roomInfo;

                    if (reqData.Roomid == 2)
                    {
                        PlayingStatus ps = new PlayingStatus();
                        ps.ActionSeatid = 5;
                        ps.ActionTime = 5000;
                        ActionData data = new ActionData();
                        data.Actions.Add(ActionType.ActionFold);
                        data.Actions.Add(ActionType.ActionBet);
                        data.Actions.Add(ActionType.ActionCheck);
                        ps.ActionData = data;
                        enterRoomRSP.PlayingStatus = ps;
                    }
                    if (reqData.Roomid == 3)
                    {
                        PlayingStatus ps = new PlayingStatus();
                        ps.ActionSeatid = 2;
                        ps.ActionTime = 5000;
                        ps.PreActionType = 2;
                        ps.PreActionChips = 200;

                        enterRoomRSP.PlayingStatus = ps;
                    }

                    protoWrapGo.Command = "EnterRoomRSP";
                    protoWrapGo.Body = enterRoomRSP.ToByteString();
                    if (reqData.Roomid == 1)
                    {
                        simulation = 1;
                    }
                    break;
                case "LeaveRoomREQ":
                    LeaveRoomRSP leaveRoomRSP = new LeaveRoomRSP();
                    protoWrapGo.Command = "LeaveRoomRSP";
                    protoWrapGo.Body = leaveRoomRSP.ToByteString();
                    break;
                case "ActionREQ":
                    var actionREQ = ActionREQ.Parser.ParseFrom(protoData.Body);
                    if (actionREQ.ActionType == ActionType.ActionCheck)
                    {
                        simulation = 2;
                    }
                    else if (actionREQ.ActionType == ActionType.ActionFold)
                    {
                        simulation = 3;
                    }

                    var temp = new ActionREQ();
                    protoWrapGo.Command = "ActionRSP";
                    protoWrapGo.Body = temp.ToByteString();
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
            else if (simulation == 3)
            {
                StartSimulation3();
            }
        }

        private void StartSimulation1()
        {
            Thread.Sleep(500);
            broadcastSequence++;
            ProtoPacket protoWrapGo = new ProtoPacket();
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

            Thread.Sleep(500);
            protoWrapGo.Command = "ActionNotifyBRC";
            var actionNotifyBRC = new ActionNotifyBRC();
            actionNotifyBRC.Seatid = 5;
            ActionData data = new ActionData();
            data.Actions.Add(ActionType.ActionFold);
            data.Actions.Add(ActionType.ActionBet);
            data.Actions.Add(ActionType.ActionCheck);
            data.LeftTime = actionTime;
            actionNotifyBRC.ActionData = data;
            protoWrapGo.Body = actionNotifyBRC.ToByteString();
            protoBytes = protoWrapGo.ToByteArray();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoBytes);
        }

        private void StartSimulation2()
        {
            Console.WriteLine("StartSimulation2");
            broadcastSequence++;
            ProtoPacket protoWrapGo = new ProtoPacket();
            protoWrapGo.Op = MessageType.MessageBroadcast;
            protoWrapGo.Seq = broadcastSequence;

            Thread.Sleep(500);
            protoWrapGo.Command = "ActionBRC";
            ActionBRC actionBRC = new ActionBRC();
            actionBRC.Seatid = 5;
            actionBRC.ActionType = ActionType.ActionCheck;
            protoWrapGo.Body = actionBRC.ToByteString();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoWrapGo.ToByteArray());

            Thread.Sleep(500);
            protoWrapGo.Command = "RoundOverBRC";
            RoundOverBRC roundOverBRC = new RoundOverBRC();
            roundOverBRC.Pool.Add(300);
            protoWrapGo.Body = roundOverBRC.ToByteString();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoWrapGo.ToByteArray());

            Thread.Sleep(500);
            protoWrapGo.Command = "RoundStartBRC";
            RoundStartBRC roundStartBRC = new RoundStartBRC();
            roundStartBRC.Stage = RoundStage.RoundFlop;
            roundStartBRC.Board.Add("Tc");
            roundStartBRC.Board.Add("As");
            roundStartBRC.Board.Add("4h");
            protoWrapGo.Body = roundStartBRC.ToByteString();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoWrapGo.ToByteArray());

            Thread.Sleep(500);
            protoWrapGo.Command = "ActionNotifyBRC";
            ActionNotifyBRC actionNotifyBRC = new ActionNotifyBRC();
            actionNotifyBRC.Seatid = 7;
            ActionData data = new ActionData();
            data.LeftTime = actionTime;
            actionNotifyBRC.ActionData = data;
            protoWrapGo.Body = actionNotifyBRC.ToByteString();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoWrapGo.ToByteArray());

            Thread.Sleep(9 * 1000);
            protoWrapGo.Command = "ActionBRC";
            actionBRC = new ActionBRC();
            actionBRC.Seatid = 7;
            actionBRC.ActionType = ActionType.ActionBet;
            actionBRC.Chips = 100;
            actionBRC.HandChips = 460;
            protoWrapGo.Body = actionBRC.ToByteString();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoWrapGo.ToByteArray());

            Thread.Sleep(500);
            protoWrapGo.Command = "ActionNotifyBRC";
            actionNotifyBRC = new ActionNotifyBRC();
            actionNotifyBRC.Seatid = 2;
            data = new ActionData();
            data.LeftTime = actionTime;
            actionNotifyBRC.ActionData = data;
            protoWrapGo.Body = actionNotifyBRC.ToByteString();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoWrapGo.ToByteArray());

            Thread.Sleep(9 * 1000);
            protoWrapGo.Command = "ActionBRC";
            actionBRC = new ActionBRC();
            actionBRC.Seatid = 2;
            actionBRC.ActionType = ActionType.ActionRaise;
            actionBRC.Chips = 200;
            actionBRC.HandChips = 360;
            protoWrapGo.Body = actionBRC.ToByteString();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoWrapGo.ToByteArray());

            Thread.Sleep(500);
            protoWrapGo.Command = "ActionNotifyBRC";
            actionNotifyBRC = new ActionNotifyBRC();
            actionNotifyBRC.Seatid = 5;
            data = new ActionData();
            data.Actions.Add(ActionType.ActionFold);
            data.Actions.Add(ActionType.ActionRaise);
            data.Actions.Add(ActionType.ActionCall);
            data.Call = 200;
            data.LeftTime = actionTime;
            actionNotifyBRC.ActionData = data;
            protoWrapGo.Body = actionNotifyBRC.ToByteString();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoWrapGo.ToByteArray());
        }

        private void StartSimulation3()
        {
            Thread.Sleep(500);
            broadcastSequence++;
            ProtoPacket protoWrapGo = new ProtoPacket();
            protoWrapGo.Op = MessageType.MessageBroadcast;
            protoWrapGo.Seq = broadcastSequence;

            Thread.Sleep(500);
            protoWrapGo.Command = "ActionBRC";
            ActionBRC actionBRC = new ActionBRC();
            actionBRC.Seatid = 5;
            actionBRC.ActionType = ActionType.ActionFold;
            protoWrapGo.Body = actionBRC.ToByteString();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoWrapGo.ToByteArray());

            Thread.Sleep(500);
            protoWrapGo.Command = "RoundOverBRC";
            RoundOverBRC roundOverBRC = new RoundOverBRC();
            roundOverBRC.Pool.Add(500);
            protoWrapGo.Body = roundOverBRC.ToByteString();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoWrapGo.ToByteArray());

            Thread.Sleep(500);
            protoWrapGo.Command = "RoundStartBRC";
            RoundStartBRC roundStartBRC = new RoundStartBRC();
            roundStartBRC.Stage = RoundStage.RoundTurn;
            roundStartBRC.Board.Add("Tc");
            roundStartBRC.Board.Add("As");
            roundStartBRC.Board.Add("4h");
            roundStartBRC.Board.Add("9s");
            protoWrapGo.Body = roundStartBRC.ToByteString();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoWrapGo.ToByteArray());

            Thread.Sleep(500);
            protoWrapGo.Command = "ActionNotifyBRC";
            var actionNotifyBRC = new ActionNotifyBRC();
            actionNotifyBRC.Seatid = 7;
            ActionData data = new ActionData();
            data.LeftTime = actionTime;
            actionNotifyBRC.ActionData = data;
            protoWrapGo.Body = actionNotifyBRC.ToByteString();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoWrapGo.ToByteArray());

            Thread.Sleep(3 * 1000);
            protoWrapGo.Command = "ActionBRC";
            actionBRC = new ActionBRC();
            actionBRC.Seatid = 7;
            actionBRC.ActionType = ActionType.ActionBet;
            actionBRC.Chips = 200;
            actionBRC.HandChips = 260;
            protoWrapGo.Body = actionBRC.ToByteString();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoWrapGo.ToByteArray());

            Thread.Sleep(500);
            protoWrapGo.Command = "ActionNotifyBRC";
            actionNotifyBRC = new ActionNotifyBRC();
            actionNotifyBRC.Seatid = 2;
            data = new ActionData();
            data.LeftTime = actionTime;
            actionNotifyBRC.ActionData = data;
            protoWrapGo.Body = actionNotifyBRC.ToByteString();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoWrapGo.ToByteArray());

            Thread.Sleep(3 * 1000);
            protoWrapGo.Command = "ActionBRC";
            actionBRC = new ActionBRC();
            actionBRC.Seatid = 2;
            actionBRC.ActionType = ActionType.ActionCall;
            actionBRC.Chips = 200;
            actionBRC.HandChips = 260;
            protoWrapGo.Body = actionBRC.ToByteString();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoWrapGo.ToByteArray());

            Thread.Sleep(500);
            protoWrapGo.Command = "RoundOverBRC";
            roundOverBRC = new RoundOverBRC();
            roundOverBRC.Pool.Add(700);
            protoWrapGo.Body = roundOverBRC.ToByteString();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoWrapGo.ToByteArray());

            Thread.Sleep(500);
            protoWrapGo.Command = "RoundStartBRC";
            roundStartBRC = new RoundStartBRC();
            roundStartBRC.Stage = RoundStage.RoundRiver;
            roundStartBRC.Board.Add("Tc");
            roundStartBRC.Board.Add("As");
            roundStartBRC.Board.Add("4h");
            roundStartBRC.Board.Add("9s");
            roundStartBRC.Board.Add("Ts");
            protoWrapGo.Body = roundStartBRC.ToByteString();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoWrapGo.ToByteArray());

            Thread.Sleep(500);
            protoWrapGo.Command = "ActionNotifyBRC";
            actionNotifyBRC = new ActionNotifyBRC();
            actionNotifyBRC.Seatid = 7;
            data = new ActionData();
            data.LeftTime = actionTime;
            actionNotifyBRC.ActionData = data;
            protoWrapGo.Body = actionNotifyBRC.ToByteString();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoWrapGo.ToByteArray());

            Thread.Sleep(3 * 1000);
            protoWrapGo.Command = "ActionBRC";
            actionBRC = new ActionBRC();
            actionBRC.Seatid = 7;
            actionBRC.ActionType = ActionType.ActionAllin;
            actionBRC.Chips = 260;
            actionBRC.HandChips = 0;
            protoWrapGo.Body = actionBRC.ToByteString();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoWrapGo.ToByteArray());

            Thread.Sleep(500);
            protoWrapGo.Command = "ActionNotifyBRC";
            actionNotifyBRC = new ActionNotifyBRC();
            actionNotifyBRC.Seatid = 2;
            data.LeftTime = actionTime;
            actionNotifyBRC.ActionData = data;
            protoWrapGo.Body = actionNotifyBRC.ToByteString();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoWrapGo.ToByteArray());

            Thread.Sleep(3 * 1000);
            protoWrapGo.Command = "ActionBRC";
            actionBRC = new ActionBRC();
            actionBRC.Seatid = 2;
            actionBRC.ActionType = ActionType.ActionAllin;
            actionBRC.Chips = 260;
            actionBRC.HandChips = 0;
            protoWrapGo.Body = actionBRC.ToByteString();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoWrapGo.ToByteArray());

            Thread.Sleep(500);
            protoWrapGo.Command = "RoundOverBRC";
            roundOverBRC = new RoundOverBRC();
            roundOverBRC.Pool.Add(700);
            roundOverBRC.Pool.Add(300);
            protoWrapGo.Body = roundOverBRC.ToByteString();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoWrapGo.ToByteArray());

            Thread.Sleep(500);
            protoWrapGo.Command = "ShowHandRSP";
            ShowHandInfo showHandInfo1 = new ShowHandInfo();
            showHandInfo1.Seatid = 2;
            showHandInfo1.Cards.Add("8s");
            showHandInfo1.Cards.Add("3d");
            ShowHandInfo showHandInfo2 = new ShowHandInfo();
            showHandInfo2.Seatid = 7;
            showHandInfo2.Cards.Add("4c");
            showHandInfo2.Cards.Add("7s");
            ShowHandRSP showhandBRC = new ShowHandRSP();
            showhandBRC.Info.Add(showHandInfo1);
            showhandBRC.Info.Add(showHandInfo2);
            protoWrapGo.Body = showhandBRC.ToByteString();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoWrapGo.ToByteArray());

            Thread.Sleep(500);
            protoWrapGo.Command = "WinnerRSP";
            WinningInfo winInfo1 = new WinningInfo();
            winInfo1.Seatid = 2;
            winInfo1.Poolid = 0;
            winInfo1.Chips = 700;
            WinningInfo winInfo2 = new WinningInfo();
            winInfo2.Seatid = 7;
            winInfo2.Poolid = 1;
            winInfo2.Chips = 300;
            WinnerRSP winnerRSP = new WinnerRSP();
            winnerRSP.Winner.Add(winInfo1);
            winnerRSP.Winner.Add(winInfo2);
            protoWrapGo.Body = winnerRSP.ToByteString();
            Console.WriteLine(protoWrapGo.Command);
            Sessions.Broadcast(protoWrapGo.ToByteArray());
        }
    }
}
