﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CBShare.Battle;
using CBShare.Configuration;
using Microsoft.AspNetCore.SignalR;
using CBShare.Common;
using WebServices.Battles;

namespace WebServices.Hubs
{
    public class BattleHub : Hub
    {
        public async Task RequestJoinRoom(long gid, RoomTypeCode roomeType, RoomLevelCode roomLevel)
        {
            var roomID = GameManager.Instance.roomController.TryJoinRoom(gid, roomeType, roomLevel);
            if (roomID <= 0)
            {
                await this.Clients.Caller.SendAsync("ShowDisplayMessage", "JoinRoomFail", true);
                return;
            }
            var battleController = GameManager.Instance.roomController.GetBattleControllerByID(roomID) as Battle2PController;
            await battleController.OnGamerJoinRoom(this, gid);
        }

        public async Task RequestCancelJoinRoom(long gid, int roomID)
        {
            /*if (!GameManager.Instance.roomController.TryCancelJoinRoom(gid))
            {
                await this.Clients.Caller.SendAsync("ShowDisplayMessage", "CancelJoinRoomFail", true);
                return;
            }*/
            var battleController = GameManager.Instance.roomController.GetBattleControllerByID(roomID) as Battle2PController;
            await battleController.OnGamerCancelJoinRoom(gid);
        }

        public async Task RequestRematch(int roomID, int gamerIndex)
        {
            var battleController = GameManager.Instance.roomController.GetBattleControllerByID(roomID) as Battle2PController;
            await battleController.OnGamerRematch(gamerIndex);
        }

        public async Task RequestBuyActionCard(int roomID, int gamerIndex, int cardIndex)
        {
            var battleController = GameManager.Instance.roomController.GetBattleControllerByID(roomID) as Battle2PController;
            await battleController.OnGamerBuyActionCard(gamerIndex, cardIndex);
        }

        public async Task RequestRollDice(int roomID, int gamerIndex, bool isSpecialRoll, int testValue, ChanceCardCode testChanceCard)
        {
            var battleController = GameManager.Instance.roomController.GetBattleControllerByID(roomID) as Battle2PController;
            await battleController.OnGamerRollDice(gamerIndex, isSpecialRoll, testValue, testChanceCard, false);
        }

        public async Task RequestBuildHouse(int roomID, int gamerIndex, int blockIndex, HouseCode houseCode)
        {
            var battleController = GameManager.Instance.roomController.GetBattleControllerByID(roomID) as Battle2PController;
            await battleController.OnGamerBuildHouse(gamerIndex, blockIndex, houseCode, false);
        }

        public async Task RequestUseCharacterSkill(int roomID, int gamerIndex, bool use)
        {
            var battleController = GameManager.Instance.roomController.GetBattleControllerByID(roomID) as Battle2PController;
            await battleController.OnGamerUseCharacterSkill(gamerIndex, use, false);
        }

        public async Task RequestUseActionCard(int roomID, int gamerIndex, ActionCardCode cardCode, bool use)
        {
            var battleController = GameManager.Instance.roomController.GetBattleControllerByID(roomID) as Battle2PController;
            await battleController.OnGamerUseActionCard(gamerIndex, cardCode, use, false);
        }

        public async Task RequestSelectBlock(int roomID, int gamerIndex, int selectedBlockIndex, SelectBlockActionCode selectActionCode)
        {
            var battleController = GameManager.Instance.roomController.GetBattleControllerByID(roomID) as Battle2PController;
            await battleController.OnGamerSelectBlock(gamerIndex, selectedBlockIndex, selectActionCode, false);
        }

        public async Task RequestSellHouses(int roomID, int gamerIndex, List<int> selectedBlockIndexsList)
        {
            var battleController = GameManager.Instance.roomController.GetBattleControllerByID(roomID) as Battle2PController;
            await battleController.OnGamerSellHouses(gamerIndex, selectedBlockIndexsList, false);
        }

        public async Task RequestExchangeBlocks(int roomID, int gamerIndex, List<int> selectedBlockIndexsList)
        {
            var battleController = GameManager.Instance.roomController.GetBattleControllerByID(roomID) as Battle2PController;
            await battleController.OnGamerExchangeBlocks(gamerIndex, selectedBlockIndexsList, false);
        }

        public async Task RequestExitRoom(long gid, int roomID)
        {
            if (!GameManager.Instance.roomController.TryExitRoom(gid))
            {
                await this.Clients.Caller.SendAsync("ShowDisplayMessage", "ExitRoomFail", true);
                return;
            }
            await this.Clients.Caller.SendAsync("OnExitRoomSuccess");
        }
    }
}   
