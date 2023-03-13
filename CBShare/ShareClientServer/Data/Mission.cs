using CBShare.Common;
using System.Collections.Generic;

namespace CBShare.Data
{
    public class Mission
    {
		public string Code { get; set; }
		public string Description { get; set; }
		public int Time { get; set; }
		public string Parameter { get; set; }
		public Dictionary<RoomLevelCode, int> Rewards { get; set; }

		public Mission()
		{
			this.Rewards = new Dictionary<RoomLevelCode, int>();
		}
	}
}
