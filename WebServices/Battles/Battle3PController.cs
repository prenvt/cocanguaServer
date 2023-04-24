using CBShare.Battle;
using CBShare.Common;
using CBShare.Configuration;
using CBShare.Data;
using CTPServer.MongoDB;
using LitJson;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebServices.Hubs;

namespace WebServices.Battles
{
    public class Battle3PController : BattleBaseController
    {
        public override Task OnGamerJoinRoom(BattleHub hub, long gid)
        {
            try
            {
                if (this.properties.state == BattleState.MATCHING)
                {
                    var gamerProperties = this.properties.gamersPropertiesList.Find(e => e.gid == gid);
                    if (gamerProperties == null)
                    {
                        var userInfo = GameManager.GetUserInfo(gid, new List<string>() { GameRequests.PROPS_GAMER_DATA, GameRequests.PROPS_STAR_CARD_DATA });
                        gamerProperties = new GamerBattleProperty()
                        {
                            gid = gid,
                            name = userInfo.gamerData.displayName,
                            avatar = userInfo.gamerData.Avatar,
                            money = userInfo.gamerData.GetCurrencyValue(CurrencyCode.MONEY),
                            color = (GamerColor)this.properties.gamersPropertiesList.Count,
                        };
                        this.properties.gamersPropertiesList.Add(gamerProperties);
                    }
                    hub.Clients.GroupExcept(this.roomKey, hub.Context.ConnectionId).SendAsync("OnOtherGamerJoinRoomSuccess", this.properties.gamersPropertiesList);
                }
                else
                {

                }
                this.hubConnectionIDsList[gid] = hub.Context.ConnectionId;
                this.hubContext.Groups.AddToGroupAsync(hub.Context.ConnectionId, this.roomKey);
                hub.Clients.Caller.SendAsync("OnJoinRoomSuccess", this.properties);

                if (this.properties.state == BattleState.MATCHING)
                {
                    RoomController.ParseRoomTypeLevelFromID(this.properties.ID, out var roomType, out var roomLevel);
                    var gamerCount = this.properties.gamersPropertiesList.Count;
                    if (gamerCount == (int)roomType)
                    {
                        this.ProcessState(BattleState.BUY_BOOSTER);
                    }
                    else if (gamerCount == 2)
                    {
                        this.SendWaitingGamerAction(new BattleGamerActionData()
                        {
                            actionType = BattleGamerAction.MatchingSuccess,
                            /*gamerColor = (GamerColor)this.properties.turnGamerIndex,
                            jsonValue = JsonMapper.ToJson(new RollDiceActionParameter()
                            {
                                isSpecial = false
                            })*/
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
            return Task.CompletedTask;
        }
    }
}
