using System;
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
        public async Task RequestJoinRoom(long gid, BattleType _roomType, BattleLevel _roomLevel)
        {
            var roomID = GameManager.Instance.roomController.TryJoinRoom(gid, _roomType, _roomLevel);
            if (roomID <= 0)
            {
                await this.Clients.Caller.SendAsync("ShowDisplayMessage", "JoinRoomFail", true);
                return;
            }
            var battleController = GameManager.Instance.roomController.GetBattleControllerByID(roomID);
            await battleController.OnGamerJoinRoom(this, gid);
        }

        public async Task RequestCancelJoinRoom(long gid, int _roomID)
        {
            var battleController = GameManager.Instance.roomController.GetBattleControllerByID(_roomID);
            await battleController.OnGamerCancelJoinRoom(gid);
        }

        public async Task RequestRematch(int _roomID, int _gamerIndex)
        {
            var battleController = GameManager.Instance.roomController.GetBattleControllerByID(_roomID);
            await battleController.OnGamerRematch(_gamerIndex);
        }

        public async Task RequestBuySpecialItem(int _roomID, int _gamerIndex, int _itemIdx)
        {
            var battleController = GameManager.Instance.roomController.GetBattleControllerByID(_roomID);
            await battleController.OnGamerBuyBoosterItem(_gamerIndex, _itemIdx);
        }

        public async Task RequestRollDice(int _roomID, GamerColor _gamerColor, bool _isSpecialRoll, int _testValue)
        {
            var battleController = GameManager.Instance.roomController.GetBattleControllerByID(_roomID);
            await battleController.OnGamerRollDice(_gamerColor, _isSpecialRoll, _testValue, false);
        }

        /*public async Task RequestBuildHouse(int roomID, int gamerIndex, int blockIndex, HouseCode houseCode)
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
        }*/

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
