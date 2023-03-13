using CBShare.Common;
using System.Collections.Generic;

namespace CBShare.Data
{
    public class DiceData
    {
		public DiceCode Id { get; set; }
		public string Name { get; set; }
		public string SkillName { get; set; }
		public string SkillDescription { get; set; }
		public int SkillTurnCount { get; set; }
		public Dictionary<PriceType, int> Prices { get; set; }

		public DiceData()
		{
			this.Prices = new Dictionary<PriceType, int>();
		}
	}
}
