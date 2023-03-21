using System;
using System.Collections.Generic;
using System.Linq;
using WebServices.Battles;
using CTPServer.MongoDB;
using CBShare.Data;
using CBShare.Common;
using CBShare.Configuration;

public class RoomController
{
    private static object syncObj = new object();
    private int totalRoomsCount;
    private Dictionary<long, int> roomIDByGID;
    private List<Battle2PController> battle2PsList;
    private Dictionary<int, Battle3PController> battle4PsList;

    public RoomController()
    {
        this.totalRoomsCount = 1;
        this.roomIDByGID = new Dictionary<long, int>();
        this.battle2PsList = new List<Battle2PController>();
        this.battle4PsList = new Dictionary<int, Battle3PController>();

        /*var allsPlayingBattlePropsList = BattlePropMongoDB.GetAllsPlayingList();
        for (int i = 0; i < allsPlayingBattlePropsList.Count; i++)
        {
            var battleProps = allsPlayingBattlePropsList[i];
            var battleController = new Battle2PController();
            battleController.Init(battleProps);
            this.battle2PsList.Add(battleController);
        }*/
    }

    public int TryJoinRoom(long gid, RoomTypeCode _roomType, RoomLevelCode _roomLevel)
    {
        lock (syncObj)
        {
            var _roomID = 0;
            if (this.roomIDByGID.ContainsKey(gid))
            {
                _roomID = this.roomIDByGID[gid];
            }
            else
            {
                var battleController = this.battle2PsList.FirstOrDefault(e => e.properties.state == BattleState.MATCHING);
                if (battleController == null)
                {
                    _roomID = GetRoomIDFromTypeLevel(_roomType, _roomLevel, this.totalRoomsCount);
                    battleController = new Battle2PController();
                    var battleProps = new BattleProperty()
                    {
                        ID = _roomID,
                        type = _roomType,
                        level = _roomLevel,
                        state = BattleState.MATCHING
                    };
                    battleProps.Init();
                    battleController.Init(battleProps);
                    this.totalRoomsCount++;
                }
                else
                {
                    _roomID = battleController.properties.ID;
                }
                this.battle2PsList.Add(battleController);
            }
            this.roomIDByGID[gid] = _roomID;
            return _roomID;
        }
    }

    /*public bool TryCancelJoinRoom(long gid)
    {
        lock (syncObj)
        {
            if (!this.roomIDByGID.ContainsKey(gid))
            {
                return false;
            }
            var roomID = this.roomIDByGID[gid];
            ParseRoomTypeLevelFromID(roomID, out var roomType, out var roomLevel);
            if (this.waitingGIDsList[roomType].Contains(gid))
            {
                this.waitingGIDsList[roomType].Remove(gid);
            }
            this.roomIDByGID.Remove(gid);
            return true;
        }
    }*/

    public bool TryExitRoom(long gid)
    {
        lock (syncObj)
        {
            if (!this.roomIDByGID.ContainsKey(gid))
            {
                return false;
            }
            var roomID = this.roomIDByGID[gid];
            ParseRoomTypeLevelFromID(roomID, out var roomType, out var roomLevel);
            this.roomIDByGID.Remove(gid);
            return true;
        }
    }

    public BattleBaseController GetBattleControllerByID(int _roomID)
    {
        ParseRoomTypeLevelFromID(_roomID, out var roomType, out var roomLevel);
        if (roomType == RoomTypeCode.BATTLE_2P)
        {
            return this.battle2PsList.FirstOrDefault(e => e.properties.ID == _roomID);
        }
        else
        {
            return null;
        }
    }

    public static int GetRoomIDFromTypeLevel(RoomTypeCode roomType, RoomLevelCode roomLevel, int roomCount)
    {
        return (int)roomType * 1000 + (int)roomLevel * 100 + roomCount;
    }

    public static bool ParseRoomTypeLevelFromID(int roomID, out RoomTypeCode roomType, out RoomLevelCode roomLevel)
    {
        roomType = (RoomTypeCode)(roomID / 1000);
        roomLevel = (RoomLevelCode)(roomID % 1000 / 100);
        return true;
    }

    /*public BattleBaseController TryCreateNewBattleController(int roomID)
    {
        lock (syncObj)
        {
            ParseRoomTypeLevelFromID(roomID, out var roomType, out var roomLevel);
            if (roomType == RoomTypeCode.BATTLE_2P)
            {
                if (!this.battle2PsList.ContainsKey(roomID))
                {
                    var battleController = new Battle2PController()
                    {
                        //roomID = roomID,
                        roomConfig = ConfigManager.instance.GetRoomConfig(roomLevel)
                    };
                }
                return this.battle2PsList[roomID];
            }
            else
            {
                return null;
            }
        }
    }*/
}
