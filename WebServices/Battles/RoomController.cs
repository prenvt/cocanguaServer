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
    private List<Battle3PController> battle3PsList;

    public RoomController()
    {
        this.totalRoomsCount = 1;
        this.roomIDByGID = new Dictionary<long, int>();
        this.battle2PsList = new List<Battle2PController>();
        this.battle3PsList = new List<Battle3PController>();

        /*var allsPlayingBattlePropsList = BattlePropMongoDB.GetAllsPlayingList();
        for (int i = 0; i < allsPlayingBattlePropsList.Count; i++)
        {
            var battleProps = allsPlayingBattlePropsList[i];
            var battleController = new Battle2PController();
            battleController.Init(battleProps);
            this.battle2PsList.Add(battleController);
        }*/
    }

    public int TryJoinRoom(long gid, RoomType _roomType, RoomLevel _roomLevel)
    {
        lock (syncObj)
        {
            var _roomID = 0;
            if (this.roomIDByGID.ContainsKey(gid))
            {
                _roomID = this.roomIDByGID[gid];
            }
            else if (_roomType == RoomType.BATTLE_2P)
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
            else if (_roomType == RoomType.BATTLE_3P)
            {
                var battleController = this.battle3PsList.FirstOrDefault(e => e.properties.state == BattleState.MATCHING);
                if (battleController == null)
                {
                    _roomID = GetRoomIDFromTypeLevel(_roomType, _roomLevel, this.totalRoomsCount);
                    battleController = new Battle3PController();
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
                this.battle3PsList.Add(battleController);
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
        if (roomType == RoomType.BATTLE_2P)
        {
            return this.battle2PsList.FirstOrDefault(e => e.properties.ID == _roomID);
        }
        else if (roomType == RoomType.BATTLE_3P)
        {
            return this.battle3PsList.FirstOrDefault(e => e.properties.ID == _roomID);
        }
        else
        {
            return null;
        }
    }

    public static int GetRoomIDFromTypeLevel(RoomType roomType, RoomLevel roomLevel, int roomCount)
    {
        return (int)roomType * 1000 + (int)roomLevel * 100 + roomCount;
    }

    public static bool ParseRoomTypeLevelFromID(int roomID, out RoomType roomType, out RoomLevel roomLevel)
    {
        roomType = (RoomType)(roomID / 1000);
        roomLevel = (RoomLevel)(roomID % 1000 / 100);
        return true;
    }
}
