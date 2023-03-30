using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CBShare.Common;

namespace CBShare.Data
{
    public class FriendInfoData
    {
        public int id;
        public string name;
        public int money;
        public CharacterCode characterId;
        public DiceType diceId;
        public int totalMatch;
        public int winMatch;
        public int winMoney;
        public bool isCanSendHeart;
        public int receiverHeart;
        public string statusText;
        public string facebookId;
        public Dictionary<EventCode, int> eventWinMoney;
    }
}
