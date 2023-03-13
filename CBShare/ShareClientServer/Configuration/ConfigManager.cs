﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LitJson;
using CBShare.Common;
using CBShare.Data;

namespace CBShare.Configuration
{
    public class ConfigManager
    {
        // Use this for initialization
        private static ConfigManager _instance;
        public OtherConfig otherConfig;
        public List<StarCardConfig> starCardsConfig;
        public Dictionary<string, CharacterConfig> charactersConfig;
        public Dictionary<string, DiceConfig> dicesConfig;
        public Dictionary<string, RoomConfig> roomsConfig;
        //public Dictionary<string, ManaBoosterConfig> manaBoostersConfig;
        //public Dictionary<string, GiftConfig> giftsConfig;
        public ShopConfig shopsConfig;
        public Dictionary<string, ActionCardConfig> actionCardsConfig;
        //public List<BlockConfig> blocks;
        //public Dictionary<string, int> manasConfig;
        public BattleConfig battleConfig;

        public ConfigManager()
        {
        }

        public static ConfigManager instance
        {
            get
            {
                if (ConfigManager._instance == null)
                {
                    ConfigManager._instance = new ConfigManager();

                    return ConfigManager._instance;
                }
                return ConfigManager._instance;
            }
            set
            {
                ConfigManager._instance = value;
            }
        }

        public void ReadAllConfigs(string otherTxt, string charactersTxt, string dicesTxt, string starCardsTxt, string shopTxt, string roomsTxt, string actionCardsTxt, string battleTxt)
        {
            this.otherConfig = JsonMapper.ToObject<OtherConfig>(otherTxt);
            this.starCardsConfig = JsonMapper.ToObject<List<StarCardConfig>>(starCardsTxt);
            this.charactersConfig = JsonMapper.ToObject<Dictionary<string, CharacterConfig>>(charactersTxt);
            this.dicesConfig = JsonMapper.ToObject<Dictionary<string, DiceConfig>>(dicesTxt);
            this.roomsConfig = JsonMapper.ToObject<Dictionary<string, RoomConfig>>(roomsTxt);
            this.shopsConfig = JsonMapper.ToObject<ShopConfig>(shopTxt);
            //this.manaBoostersConfig = JsonMapper.ToObject<Dictionary<string, ManaBoosterConfig>>(manaBoostersTxt);
            //this.giftsConfig = JsonMapper.ToObject<Dictionary<string, GiftConfig>>(giftsTxt);
            //this.jewelPacksConfig = JsonMapper.ToObject<List<JewelPackConfig>>(jewelPacksTxt);
            //this.moneyPacksConfig = JsonMapper.ToObject<List<MoneyPackConfig>>(moneyPacksTxt);
            this.actionCardsConfig = JsonMapper.ToObject<Dictionary<string, ActionCardConfig>>(actionCardsTxt);
            //this.manasConfig = JsonMapper.ToObject<Dictionary<string, int>>(manasTxt);
            this.battleConfig = JsonMapper.ToObject<BattleConfig>(battleTxt);
            //this.blocksConfig = JsonMapper.ToObject<List<BlockConfig>>(blocksTxt);
            //this.missionsConfig = JsonMapper.ToObject<Dictionary<string, MissionConfig>>(missionsTxt);
            //this.replayEventsTimeConfig = JsonMapper.ToObject<Dictionary<string, float>>(replayEventsTimeTxt);
        }

        public CharacterConfig GetCharacterConfig(CharacterCode characterCode)
        {
            return this.charactersConfig[characterCode.ToString()];
        }

        public StarCardConfig GetStarCardConfig(string cardName)
        {
            return this.starCardsConfig.FirstOrDefault(c => c.name == cardName);
        }

        public DiceConfig GetDiceConfig(DiceCode diceCode)
        {
            return this.dicesConfig[diceCode.ToString()];
        }

        public RoomConfig GetRoomConfig(RoomLevelCode roomLevel)
        {
            return this.roomsConfig[roomLevel.ToString()];
        }

        public ActionCardConfig GetActionCardConfig(ActionCardCode actionCardCode)
        {
            return this.actionCardsConfig[actionCardCode.ToString()];
        }

        public BlockConfig GetBlockConfig(int blockIndex)
        {
            return this.battleConfig.blocks[blockIndex];
        }

        public int GetManaValue(ManaCode manaCode)
        {
            return this.battleConfig.manas[manaCode.ToString()];
        }

        public MissionConfig GetMissionConfig(MissionCode missionCode)
        {
            return this.battleConfig.missions[missionCode.ToString()];
        }

        public int FindBlockIndexByType(BlockTypeCode blockTypeCode)
        {
            var blockCfgByType = this.battleConfig.blocks.FirstOrDefault(b => b.type == blockTypeCode.ToString());
            if (blockCfgByType == null) return -1;
            return this.battleConfig.blocks.IndexOf(blockCfgByType);
        }

        public int GetActionCardPrice(RoomLevelCode roomLevel, int cardIndex)
        {
            var roomCfg = this.GetRoomConfig(roomLevel);
            return roomCfg.actionCardCosts[cardIndex];
        }

        /*public float GetBattleWaitTime(MessageTagCode messageTag)
        {
            var messageKey = messageTag.ToString();
            if (this.battleConfig.waitTimes.ContainsKey(messageKey))
            {
                return this.battleConfig.waitTimes[messageKey];
            }
            return 5f;
        }*/

        public bool CheckBanDisplayName(string displayName)
        {
            if (this.otherConfig.banDisplayNames == null)
            {
                return false;
            }
            for (int i = 0; i < this.otherConfig.banDisplayNames.Count; i++)
            {
                var banDisplayName = this.otherConfig.banDisplayNames[i];
                if (displayName.ToLower().Contains(banDisplayName.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
