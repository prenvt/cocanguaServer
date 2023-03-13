using CBShare.Common;
using System.Collections.Generic;

namespace CBShare.Data
{
    public class ManaBoosterData
    {
		public ManaBoosterCode Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public int ManaValue { get; set; }
		public Dictionary<RoomLevelCode, int> Prices { get; set; }

		public ManaBoosterData()
		{
			this.Prices = new Dictionary<RoomLevelCode, int>();
		}
	}
}
