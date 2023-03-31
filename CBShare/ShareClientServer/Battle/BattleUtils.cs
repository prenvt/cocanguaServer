using CBShare.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CBShare.Configuration;

namespace CBShare.Battle
{
    public class BattleUtils
    {
        /*public static float GetFrameReplayTime(FrameReplayData frame)
        {
            return 0f;
        }

        public static float GetFramesReplayTime(List<FrameReplayData> framesReplay)
        {
            var replayTime = 0f;
            foreach (var frame in framesReplay)
            {
                replayTime += GetFrameReplayTime(frame);
            }
            return replayTime;
        }*/

        /*public static float GetBattleActionWaitTime(BattleActionCodeTmp actionCode)
        {
            return 0f;
        }*/

        /*public static int GetHouseCostAtBlock(int blockIndex, HouseCode houseCode)
        {
            var blockCfg = ConfigManager.Instance.GetBlockConfig(blockIndex);
            var houseCost = 0;
            var houseCodeIndex = (int)houseCode;
            if (houseCodeIndex < blockCfg.housesCost.Count)
            {
                houseCost = blockCfg.housesCost[houseCodeIndex];
            }
            return houseCost;
        }

        public static int GetHouseTaxAtBlock(int blockIndex, HouseCode houseCode)
        {
            var blockCfg = ConfigManager.Instance.GetBlockConfig(blockIndex);
            var houseTax = 0;
            var houseCodeIndex = (int)houseCode;
            if (houseCodeIndex < blockCfg.housesCost.Count)
            {
                houseTax = blockCfg.housesTax[houseCodeIndex];
            }
            return houseTax;
        }*/

        /*public static int GetSellHousePriceAtBlock(int blockIndex, HouseCode houseCode, int reduceSellHouseFeesPercent)
        {
            var blockCfg = ConfigManager.Instance.GetBlockConfig(blockIndex);
            var houseCost = blockCfg.GetHouseCost(houseCode);
            var sellHousePrice = (int)((50f + reduceSellHouseFeesPercent) * houseCost / 100f);
            return sellHousePrice;
        }*/

        public static int GetNumGamersInBattle(RoomType roomType)
        {
            if (roomType == RoomType.BATTLE_2P)
            {
                return 2;
            }
            if (roomType == RoomType.BATTLE_3P)
            {
                return 3;
            }
            return 0;
        }
    }
}
