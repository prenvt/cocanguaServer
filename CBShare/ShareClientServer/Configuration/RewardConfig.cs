using System;
using System.Collections.Generic;

namespace CBShare
{
    namespace Configuration
    {
        [System.Serializable]
        public partial class RewardConfig : BaseConfig
        {
            //public long exp = 0;
            public List<RewardData> Rewards = new List<RewardData>();

            public bool IsTreasureRewardConfig()
            {
                if (Rewards == null) return false;

                foreach (RewardData reward in Rewards)
                {
                    if (reward.Quantity < 0)
                    {
                        return false;
                    }
                }

                return true;
            }
            public RewardConfig Clone()
            {
                RewardConfig rw = new RewardConfig();
                foreach (var e in this.Rewards)
                {
                    rw.Rewards.Add(e.Clone());
                }
                return rw;
            }

            public static RewardConfig XPhanThuong(RewardConfig rw, int x)
            {
                RewardConfig result = new RewardConfig();
                result.Rewards = new List<RewardData>();
                foreach (RewardData reward in rw.Rewards)
                {
                    result.Rewards.Add(new RewardData(reward.CodeName, reward.Quantity * x));
                }
                return result;
            }

            public static RewardConfig CongPhanThuong(RewardConfig rw1_, RewardConfig rw2_)
            {
                RewardConfig rw1 = rw1_.Clone();

                RewardConfig rw2 = rw2_.Clone();

                foreach (RewardData reward in rw1.Rewards)
                {
                    var r = rw2.Rewards.Find(e => e.CodeName == reward.CodeName);
                    if (r != null)
                    {
                        reward.Quantity += r.Quantity;
                        rw2.Rewards.Remove(r);
                    }
                }

                rw1.Rewards.AddRange(rw2.Rewards);
                return rw1;
            }
        }

        [Serializable]
        public class RewardPercent
        {
            public string CodeName;

            public float Percent;

            public RewardPercent()
            {

            }

            public RewardPercent(string codeName, float percent)
            {
                this.CodeName = codeName;
                this.Percent = percent;
            }

        }
        [Serializable]
        public class RewardData
        {
            public string CodeName;
            public long Quantity;


            public RewardData()
            {

            }

            public RewardData(string codeName, long quantity)
            {
                this.CodeName = codeName;
                this.Quantity = quantity;
            }

            public RewardData Clone()
            {
                RewardData r = new RewardData(this.CodeName, this.Quantity);
                return r;
            }
        }

        public class OptionChest : BaseConfig
        {
            public List<RewardData> optionList;
        }
    }
}