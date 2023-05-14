using System;
using Microsoft.AspNetCore.SignalR;
using WebServices.Hubs;
using System.Collections.Generic;
using CBShare.Common;
using System.Threading.Tasks;
using CTPServer.MongoDB;
using CBShare.Configuration;
using System.Linq;
using CBShare.Data;
using LitJson;
using CBShare.Battle;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebServices.Battles
{
    public class BattleBaseController : IDisposable
    {
        protected static object syncObj = new object();
        public BattleProperty properties { get; set; }
        protected RoomConfig roomConfig { get; set; }
        protected BattleReplayData replayData { get; set; }
        protected int lastReplayStepsCount;
        //protected float lastBattleTime;
        public string roomKey { get; set; }
        protected Dictionary<long, string> hubConnectionIDsList = new Dictionary<long, string>();
        protected IHubContext<BattleHub> hubContext;
        protected readonly System.Timers.Timer updateTimer = new System.Timers.Timer();
        protected float saveRoomElapsedTime = 0f;
        protected GamerBattleProperty currentTurnGamer { get { return this.properties.gamersPropertiesList[this.properties.turnGamerIndex]; } }
        protected bool needProcessAFK = false;
        protected float updateDeltaTime = 0.2f;
        //protected float elapsedTime = 0f;
        protected BattleConfig battleCfg;

        public BattleBaseController()
        {
            this.hubContext = Program.host.Services.GetService(typeof(IHubContext<BattleHub>)) as IHubContext<BattleHub>;
            this.updateTimer.Interval = this.updateDeltaTime * 1000;
            this.updateTimer.Elapsed += this.Update;
            this.updateTimer.Start();
        }

        public void Init(BattleProperty _props)
        {
            this.properties = _props;
            this.battleCfg = ConfigManager.instance.battlesConfig[this.properties.type.ToString()];
            this.replayData = BattleReplayMongoDB.GetByBattleID(_props.ID);
            this.replayData.stepsList.Clear();
            this.roomKey = string.Format("LUDO_{0}", _props.ID);
            this.roomConfig = ConfigManager.instance.GetRoomConfig(_props.level);
        }

        protected virtual void Update(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (this.properties != null && this.properties.state > BattleState.NONE && 
                    this.properties.waitingAction != null && !this.properties.waitingAction.isInvoked)
                {
                    this.properties.elapsedTime += this.updateDeltaTime;
                    if (this.properties.elapsedTime >= this.properties.waitingAction.invokeTime)
                    {
                        this.ProcessInvokeWaitingAction();
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        protected void SetWaitingAction(BattleActionType _actionType, float _delayTime, GamerColor _gamerColor = GamerColor.NONE/*, string _jsPrams = ""*/)
        {
            try
            {
                //var waitingTime = ConfigManager.instance.battleConfig.waitTimes[_waitingAction.ToString()];
                lock (syncObj)
                {
                    /*this.waitingActionController = new BattleWaitingActionController()
                    {
                        actionCode = _waitingAction,
                        gamerIndex = this.properties.turnGamerIndex,
                    };*/
                    this.properties.waitingAction = new BattleActionData()
                    {
                        actionType = _actionType,
                        invokeTime = this.properties.elapsedTime + _delayTime,
                        gamerColor = _gamerColor,
                        //jsonParams = _jsPrams,
                        isInvoked = false
                    };
                }
                BattleMongoDB.Save(this.properties);
                BattleReplayMongoDB.Save(this.replayData);
                if (_gamerColor > GamerColor.NONE)
                {
                    var replayStepsList = new List<ReplayStepData>();
                    for (int i = this.lastReplayStepsCount; i < this.replayData.stepsList.Count; i++)
                    {
                        replayStepsList.Add(this.replayData.stepsList[i]);
                    }
                    //_gamerActionData.delayTime = this.properties.battleTime - this.lastBattleTime;
                    this.hubContext.Clients.Group(this.roomKey).SendAsync("WaitingGamerAction", this.properties.waitingAction, replayStepsList);
                }
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        protected virtual void ProcessInvokeWaitingAction()
        {
            try
            {
                this.properties.waitingAction.isInvoked = true;
                switch (this.properties.waitingAction.actionType)
                {
                    case BattleActionType.MatchingSuccess:
                        {
                            var waitingTime = 5f;// this.battleCfg.waitTimes[BattleState.BUY_BOOSTER.ToString()];
                            for (int i = 0; i < this.properties.gamersPropertiesList.Count; i++)
                            {
                                var gamerColor = (GamerColor)i;
                                var gamerProperties = this.properties.gamersPropertiesList[i];
                                this.hubContext.Clients.Client(this.hubConnectionIDsList[gamerProperties.gid]).SendAsync("WaitingBuyBoosterItem", gamerColor, waitingTime, gamerProperties);
                            }
                            this.SetWaitingAction(BattleActionType.StartBattle, waitingTime);
                        }
                        break;

                    case BattleActionType.StartBattle:
                        {
                            this.properties.turnGamerIndex = RandomUtils.GetRandomInt(0, 2);
                            this.properties.firstTurnGamerIndex = this.properties.turnGamerIndex;
                            //var actionCardCfg = ConfigManager.instance.GetActionCardConfig(ActionCardCode.StartGift);
                            for (int i = 0; i < this.properties.gamersPropertiesList.Count; i++)
                            {
                                var gamerProperties = this.properties.gamersPropertiesList[i];
                                gamerProperties.rematch = false;
                            }
                            this.hubContext.Clients.Group(this.roomKey).SendAsync("StartBattle", this.properties);
                            this.ProcessStartTurn();
                        }
                        break;

                    /*case BattleState.MOVE_TO_BLOCK:
                        {
                            var destBlockIndex = this.properties.destBlockIndex;
                            var skillCharacterCode = this.properties.skillCharacter;
                            var delayTime = 0f;
                            if (destBlockIndex < this.currentTurnGamer.currentBlockIndex)
                            {
                                this.hubContext.Clients.Group(this.roomKey).SendAsync("MoveCharacterToBlock", this.properties.turnGamerIndex, this.currentTurnGamer.currentBlockIndex, 0, skillCharacterCode);
                                delayTime += ConfigManager.instance.battleConfig.TIME_MOVE_CHARACTER_PER_STEP * (this.properties.blocksList.Count - this.currentTurnGamer.currentBlockIndex);
                                var addSalary = this.currentTurnGamer.AddSalary(this.roomConfig.salary);
                                this.hubContext.Clients.Group(this.roomKey).SendAsync("ChangeCash", this.currentTurnGamer, CharacterAnim.HAPPY, addSalary, -1, delayTime);

                                delayTime += 0.5f;
                                if (destBlockIndex > 0)
                                {
                                    this.hubContext.Clients.Group(this.roomKey).SendAsync("MoveCharacterToBlock", this.properties.turnGamerIndex, 0, destBlockIndex, skillCharacterCode);
                                    delayTime += ConfigManager.instance.battleConfig.TIME_MOVE_CHARACTER_PER_STEP * destBlockIndex;
                                }
                            }
                            else
                            {
                                this.hubContext.Clients.Group(this.roomKey).SendAsync("MoveCharacterToBlock", this.properties.turnGamerIndex, this.currentTurnGamer.currentBlockIndex, destBlockIndex, skillCharacterCode);
                                var numSteps = (this.properties.blocksList.Count + destBlockIndex - this.currentTurnGamer.currentBlockIndex) % this.properties.blocksList.Count;
                                delayTime += ConfigManager.instance.battleConfig.TIME_MOVE_CHARACTER_PER_STEP * numSteps;
                            }
                            this.SetNextState(BattleState.STAY_AT_BLOCK, delayTime);
                        }
                        break;

                    case BattleState.STAY_AT_BLOCK:
                        {

                        }
                        break;

                    case BattleState.END_TURN:
                        {
                            if (!this.currentTurnGamer.isWaitingUseCharacterSkill)
                            {
                                if (this.currentTurnGamer.skillTurnCount > 0)
                                {
                                    this.currentTurnGamer.skillTurnCount--;
                                    this.hubContext.Clients.Client(this.hubConnectionIDsList[this.currentTurnGamer.gid]).SendAsync("UpdateGamerProperties", this.currentTurnGamer);
                                }
                                if (this.CheckTurnGamerCanUseCharacterSkill()) // Use Character Skill.
                                {
                                    this.currentTurnGamer.isWaitingUseCharacterSkill = true;
                                    this.SendWaitingActionResponse(BattleWaitingActionCode.WAITING_USE_CHARACTER_SKILL);
                                    return;
                                }
                            }
                            foreach (var block in this.properties.blocksList)
                            {
                                //var blockIndex = this.currentTurnGamer.ownedBlockIndexsList[i];
                                //var block = this.properties.blocksList[blockIndex];
                                if (block.ownerIndex != this.properties.turnGamerIndex)
                                    continue;
                                for (int j = 0; j < block.tollRatesByTurn.Count; j++)
                                {
                                    var tollRateByTurn = block.tollRatesByTurn[j];
                                    if (tollRateByTurn.turn > 0)
                                    {
                                        tollRateByTurn.turn--;
                                    }
                                }
                            }
                            for (int i = 0; i < this.currentTurnGamer.tollRatesByTurn.Count; i++)
                            {
                                var tollRateByTurn = this.currentTurnGamer.tollRatesByTurn[i];
                                if (tollRateByTurn.turn > 0)
                                {
                                    tollRateByTurn.turn--;
                                }
                            }

                            if (this.currentTurnGamer.numDicesByTurn != null && this.currentTurnGamer.numDicesByTurn.turn > 0)
                            {
                                this.currentTurnGamer.numDicesByTurn.turn--;
                            }
                            this.properties.turnGamerIndex = (this.properties.turnGamerIndex + 1) % this.properties.gamersPropertiesList.Count;
                            if (this.properties.turnGamerIndex == this.properties.firstTurnGamerIndex)
                            {
                                if (this.properties.turnCount == this.roomConfig.maxTurn)
                                {
                                    this.ProcessEndBattle(EndBattleType.TURN_OFF);
                                }
                                else
                                {
                                    if (this.properties.turnCount == this.roomConfig.maxTurn - 3)
                                    {
                                        this.hubContext.Clients.Group(this.roomKey).SendAsync("ShowWarning", BattleWarningType.WARNING_REMAIN_3_TURN, -1);
                                    }
                                    this.properties.turnCount++;
                                    this.SetNextState(BattleState.START_TURN, this.updateDeltaTime);
                                }
                            }
                            else
                            {
                                this.SetNextState(BattleState.START_TURN, this.updateDeltaTime);
                            }
                        }
                        break;*/
                }
                BattleMongoDB.Save(this.properties);
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.hubContext.Clients.Group(this.roomKey).SendAsync("ShowDisplayMessage", ex.ToString(), true);
            }
        }

        /*protected void SetNextState(BattleState _state, float _delayTime)
        {
            this.elapsedTime = 0f;
            this.properties.nextStateTime = _delayTime;
            this.properties.nextState = _state;
            BattleMongoDB.Save(this.properties);
        }*/

        /*protected void ProcessStartBattle()
        {
            this.properties.turnGamerIndex = RandomUtils.GetRandomInt(0, 2);
            this.properties.firstTurnGamerIndex = this.properties.turnGamerIndex;

            var actionCardCfg = ConfigManager.instance.GetActionCardConfig(ActionCardCode.StartGift);
            for (int i = 0; i < this.properties.gamersPropertiesList.Count; i++)
            {
                var gamerProperties = this.properties.gamersPropertiesList[i];
                if (gamerProperties.actionCardsList.ContainsKey(ActionCardCode.StartGift.ToString()))
                {
                    var giftCash = gamerProperties.cash * actionCardCfg.value / 100;
                    gamerProperties.AddStartGift(giftCash);
                    gamerProperties.actionCardsList[ActionCardCode.StartGift.ToString()] = false;
                }
            }
            this.hubContext.Clients.Group(this.roomKey).SendAsync("StartBattle", this.properties);
        }*/

        protected void ProcessStartTurn()
        {
            try
            {
                this.properties.ProcessSortGamersByPoint();
                //this.SyncBattlePropertiesToAllGamers();
                this.lastReplayStepsCount = this.replayData.stepsList.Count;
                //this.lastBattleTime = this.properties.battleTime;
                this.SetWaitingAction(BattleActionType.RollDice, 10, (GamerColor)this.properties.turnGamerIndex);
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        protected bool CheckValidWaitingGamerAction(BattleActionType _actionType, GamerColor _gamerColor)
        {
            lock (syncObj)
            {
                if (this.properties.waitingAction == null)
                {
                    return false;
                }
                if (this.properties.waitingAction.actionType != _actionType)
                {
                    return false;
                }
                if (this.properties.waitingAction.gamerColor != _gamerColor)
                {
                    return false;
                }
                this.properties.waitingAction = null;
                return true;
            }
        }

        protected void ProcessTurnGamerStartHorse(int _spaceIdx)
        {
            try
            {
                this.AddReplayStep(ReplayStepType.StartHorse, this.properties.turnGamerIndex, 0.5f, _spaceIdx);
                this.ProcessStartTurn();
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        protected void ProcessTurnGamerMoveHorse(int _diceValue)
        {
            try
            {
                var horseCanMoveIdxsList = new List<int>();
                for (int i = 0; i < this.currentTurnGamer.horseSpaceIndexsList.Count; i++)
                {
                    var horseSpaceIdx = this.currentTurnGamer.horseSpaceIndexsList[i];
                    if (horseSpaceIdx >= 0 && horseSpaceIdx < 30)
                    {
                        horseCanMoveIdxsList.Add(i);
                    }
                }
                if (horseCanMoveIdxsList.Count == 0)
                {
                    this.ProcessEndTurn();
                }
                else if (horseCanMoveIdxsList.Count == 1)
                {
                    var _horseIdx = horseCanMoveIdxsList[0];
                    var startSpaceIdx = this.battleCfg.startIndexs[this.currentTurnGamer.color.ToString()];
                    var horseSpaceIdx = this.currentTurnGamer.horseSpaceIndexsList[_horseIdx];
                    var destSpaceIdx = horseSpaceIdx % this.battleCfg.numSpaces;
                    if (destSpaceIdx > startSpaceIdx)
                    {
                        this.ProcessApplyHorseMoveToSpace(_horseIdx, horseSpaceIdx, destSpaceIdx);
                    }
                    else
                    {
                        var endSpaceIdx = this.battleCfg.endIndexs[this.currentTurnGamer.color.ToString()];
                        if (destSpaceIdx <= endSpaceIdx)
                        {
                            this.ProcessApplyHorseMoveToSpace(_horseIdx, horseSpaceIdx, destSpaceIdx);
                        }
                        else
                        {

                        }
                    }
                }
                else
                {
                    this.WaitTurnGamerChooseHorseToMove(horseCanMoveIdxsList);
                }
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        protected void WaitTurnGamerChooseHorseToMove(List<int> _horseIdxsList)
        {

        }

        protected void ProcessApplyHorseMoveToSpace(int _horseIdx, int _fromSpaceIdx, int _destSpaceIdx)
        {
            try
            {
                var spaceValue = this.properties.spacesList[_destSpaceIdx];
                if (spaceValue > 0)
                {
                    var stayGamerIdx = spaceValue / 10;
                    var stayGamerColor = (GamerColor)stayGamerIdx;
                    if (stayGamerColor == this.currentTurnGamer.color)
                    {
                        return;
                    }
                    this.ProcessTurnGamerKickOpponentHorse(_destSpaceIdx, spaceValue);
                }
                this.properties.spacesList[_destSpaceIdx] = (int)this.currentTurnGamer.color * 10 + _horseIdx;
                this.currentTurnGamer.horseSpaceIndexsList[_horseIdx] = _destSpaceIdx;
                var moveTime = 1f;// this.battleCfg.TIME_MOVE_CHARACTER_PER_STEP * (this.properties.blocksList.Count - this.currentTurnGamer.currentBlockIndex);
                this.AddReplayStep(ReplayStepType.MoveHorse, this.properties.turnGamerIndex, moveTime, _fromSpaceIdx, _destSpaceIdx);
                /*if (_destBlockIndex < this.currentTurnGamer.currentBlockIndex)
                {
                    
                    if (_destBlockIndex > 0)
                    {
                        moveTime = ConfigManager.instance.battleConfig.TIME_MOVE_CHARACTER_PER_STEP * _destBlockIndex;
                        this.AddReplayStep(ReplayStepType.MoveCharacterToBlock, this.properties.turnGamerIndex, new MoveCharacterReplayParameter()
                        {
                            fB = 0,
                            dB = _destBlockIndex,
                            sC = _skillCharacter
                        }, moveTime);
                    }
                }
                else
                {
                    var numSteps = (this.properties.blocksList.Count + _destBlockIndex - this.currentTurnGamer.currentBlockIndex) % this.properties.blocksList.Count;
                    var moveTime = ConfigManager.instance.battleConfig.TIME_MOVE_CHARACTER_PER_STEP * numSteps;
                    this.AddReplayStep(ReplayStepType.MoveCharacterToBlock, this.properties.turnGamerIndex, new MoveCharacterReplayParameter()
                    {
                        fB = this.currentTurnGamer.currentBlockIndex,
                        dB = _destBlockIndex,
                        sC = _skillCharacter
                    }, moveTime);
                }*/
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        protected void ProcessTurnGamerKickOpponentHorse(int _spaceIdx, int _spaceValue)
        {
            try
            {
                var stayGamerIdx = _spaceValue / 10;
                var stayGamerColor = (GamerColor)stayGamerIdx;
                var stayGamer = this.GetGamerByColor(stayGamerColor);
                var stayHorseIdx = _spaceValue % 10;
                stayGamer.horseSpaceIndexsList[stayHorseIdx] = -1;
                this.properties.spacesList[_spaceIdx] = -1;
                /*if (_destBlockIndex < this.currentTurnGamer.currentBlockIndex)
                {
                    var moveTime = ConfigManager.instance.battleConfig.TIME_MOVE_CHARACTER_PER_STEP * (this.properties.blocksList.Count - this.currentTurnGamer.currentBlockIndex);
                    this.AddReplayStep(ReplayStepType.MoveCharacterToBlock, this.properties.turnGamerIndex, new MoveCharacterReplayParameter()
                    {
                        fB = this.currentTurnGamer.currentBlockIndex,
                        dB = 0,
                        sC = _skillCharacter
                    }, moveTime);
                    if (_destBlockIndex > 0)
                    {
                        moveTime = ConfigManager.instance.battleConfig.TIME_MOVE_CHARACTER_PER_STEP * _destBlockIndex;
                        this.AddReplayStep(ReplayStepType.MoveCharacterToBlock, this.properties.turnGamerIndex, new MoveCharacterReplayParameter()
                        {
                            fB = 0,
                            dB = _destBlockIndex,
                            sC = _skillCharacter
                        }, moveTime);
                    }
                }
                else
                {
                    var numSteps = (this.properties.blocksList.Count + _destBlockIndex - this.currentTurnGamer.currentBlockIndex) % this.properties.blocksList.Count;
                    var moveTime = ConfigManager.instance.battleConfig.TIME_MOVE_CHARACTER_PER_STEP * numSteps;
                    this.AddReplayStep(ReplayStepType.MoveCharacterToBlock, this.properties.turnGamerIndex, new MoveCharacterReplayParameter()
                    {
                        fB = this.currentTurnGamer.currentBlockIndex,
                        dB = _destBlockIndex,
                        sC = _skillCharacter
                    }, moveTime);
                }*/
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        /*protected virtual void ContinueTurnGamerAction()
        {
            try
            {
                if (this.currentTurnGamer.isRollingDoubleDices)
                {
                    //this.SetNextState(BattleState.START_TURN, this.updateDeltaTime);
                    this.ProcessStartTurn();
                }
                else
                {
                    //this.SetNextState(BattleState.END_TURN, this.updateDeltaTime);
                    this.ProcessEndTurn();
                }
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }*/

        protected void ProcessEndTurn()
        {
            try
            {
               
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        protected void ProcessEndBattle(EndBattleType endBattleType, int winGamerIndex)
        {
            try
            {
                this.properties.state = BattleState.Finised;
                this.properties.ProcessSortGamersByPoint();
                BattleMongoDB.Save(this.properties);
                BattleReplayMongoDB.Save(this.replayData);
                this.hubContext.Clients.Group(this.roomKey).SendAsync("ShowEndBattle", this.properties);
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        protected void SendDisplayMessageToAllGamers(string msg)
        {
            this.hubContext.Clients.Group(this.roomKey).SendAsync("ShowDisplayMessage", msg, true);
        }

        protected void AddReplayStep(ReplayStepType _type, int _gamerIdx, float _stepTime = 0.25f, params object[] _paramValues)
        {
            var stepData = new ReplayStepData()
            {
                ID = this.replayData.stepsList.Count,
                ty = _type,
                g = (GamerColor)_gamerIdx,
                t = this.properties.elapsedTime
            };
            if (_paramValues.Length > 0 )
            {
                stepData.v1 = _paramValues[0];
            }
            if (_paramValues.Length > 1 )
            {
                stepData.v2 = _paramValues[1];
            }
            if ( _paramValues.Length > 2 )
            {
                stepData.v3 = _paramValues[2];
            }
            this.replayData.stepsList.Add(stepData);
            this.properties.elapsedTime += _stepTime;
        }

        #region Listen action from gamers.
        public Task OnGamerJoinRoom(BattleHub hub, long gid)
        {
            try
            {
                var joinGamerProperties = this.properties.gamersPropertiesList.Find(e => e.gid == gid);
                if (joinGamerProperties == null)
                {
                    var userInfo = GameManager.GetUserInfo(gid, new List<string>() { GameRequests.PROPS_GAMER_DATA, GameRequests.PROPS_STAR_CARD_DATA });
                    joinGamerProperties = new GamerBattleProperty()
                    {
                        gid = gid,
                        name = userInfo.gamerData.displayName,
                        avatar = userInfo.gamerData.Avatar,
                        money = userInfo.gamerData.GetCurrencyValue(CurrencyCode.MONEY),
                        color = (GamerColor)this.properties.gamersPropertiesList.Count,
                    };
                    this.properties.gamersPropertiesList.Add(joinGamerProperties);
                }
                if (this.properties.state == BattleState.Matching)
                {
                    
                    hub.Clients.GroupExcept(this.roomKey, hub.Context.ConnectionId).SendAsync("OnOtherGamerJoinRoomSuccess", this.properties.gamersPropertiesList);
                }
                else
                {

                }
                this.hubConnectionIDsList[gid] = hub.Context.ConnectionId;
                this.hubContext.Groups.AddToGroupAsync(hub.Context.ConnectionId, this.roomKey);
                hub.Clients.Caller.SendAsync("OnJoinRoomSuccess", this.properties.ID, joinGamerProperties.color, this.properties.gamersPropertiesList);

                if (this.properties.state == BattleState.Matching)
                {
                    RoomController.ParseRoomTypeLevelFromID(this.properties.ID, out var roomType, out var roomLevel);
                    var gamerCount = this.properties.gamersPropertiesList.Count;
                    if (gamerCount == 2)
                    {
                        this.SetWaitingAction(BattleActionType.MatchingSuccess, 5);
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

        public async Task OnGamerCancelJoinRoom(long gid)
        {
            try
            {
                if (this.properties.state != BattleState.Matching)
                    return;
                await this.hubContext.Clients.Client(this.hubConnectionIDsList[gid]).SendAsync("OnCancelJoinRoomSuccess");
                this.hubConnectionIDsList.Remove(gid);
                var gamerProperties = this.properties.gamersPropertiesList.Find(e => e.gid == gid);
                this.properties.gamersPropertiesList.Remove(gamerProperties);
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        public async Task OnGamerBuyBoosterItem(GamerColor _gamerColor, int _itemIdx)
        {
            try
            {
                if (this.properties.state != BattleState.BuyBoosterItem)
                {
                    return;
                }
                var gamerProperties = this.properties.gamersPropertiesList.Find(e => e.color == _gamerColor);
                var randItem = BoosterItemType.NONE;
                if (_itemIdx >= gamerProperties.boosterItemsList.Count)
                {
                    var allBoosterItemsList = new List<BoosterItemType>();
                    foreach (BoosterItemType cardCode in (BoosterItemType[])Enum.GetValues(typeof(BoosterItemType)))
                    {
                        if (cardCode == BoosterItemType.NONE) 
                            continue;
                        if (gamerProperties.boosterItemsList.ContainsKey(cardCode.ToString())) continue;
                        allBoosterItemsList.Add(cardCode);
                    }
                    var randIndex = RandomUtils.GetRandomInt(0, allBoosterItemsList.Count);
                    randItem = allBoosterItemsList[randIndex];
                    gamerProperties.boosterItemsList.Add(randItem.ToString(), true);
                }
                await this.hubContext.Clients.Client(this.hubConnectionIDsList[gamerProperties.gid]).SendAsync("BuyBoosterItemResponse", _itemIdx, randItem);
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        public virtual Task OnGamerRollDice(long _gid, int _testValue, bool isAFK)
        {
            try
            {
                var gamerProperties = this.properties.gamersPropertiesList.Find(e => e.gid == _gid);
                if (gamerProperties == null)
                {
                    return Task.CompletedTask;
                }
                if (!this.CheckValidWaitingGamerAction(BattleActionType.RollDice, gamerProperties.color))
                {
                    return Task.CompletedTask;
                }
                this.lastReplayStepsCount = this.replayData.stepsList.Count;
                //this.lastBattleTime = this.properties.battleTime;
                var diceValue = DiceController.getRollValue(this.currentTurnGamer.currentDice, false);
                if (_testValue > 0)
                {
                    diceValue = _testValue;
                }
                this.AddReplayStep(ReplayStepType.RollDice, this.properties.turnGamerIndex, 3f, diceValue);
                if (diceValue == 6)
                {
                    var freeHorseIdx = this.currentTurnGamer.GetFreeHorse();
                    if (freeHorseIdx >= 0)
                    {
                        var startHorseSpaceIdx = this.battleCfg.startIndexs[gamerProperties.color.ToString()];
                        var spaceValue = this.properties.spacesList[startHorseSpaceIdx];
                        if (spaceValue <= 0)
                        {
                            this.ProcessTurnGamerStartHorse(startHorseSpaceIdx);
                        }
                        else if (spaceValue / 10 == (int)gamerProperties.color)
                        {
                            this.ProcessTurnGamerMoveHorse(diceValue);
                        }
                        else
                        {
                            this.ProcessTurnGamerKickOpponentHorse(startHorseSpaceIdx, spaceValue);
                            this.ProcessTurnGamerStartHorse(startHorseSpaceIdx);
                        }
                    }
                    else
                    {
                        this.ProcessTurnGamerMoveHorse(diceValue);
                    }
                }
                else
                {
                    this.ProcessTurnGamerMoveHorse(diceValue);
                }
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
            return Task.CompletedTask;
        }

        public async Task OnGamerRematch(int gamerIndex)
        {
            try
            {
                if (this.properties.state != BattleState.Finised)
                {
                    return;
                }
                var gamerProperties = this.properties.gamersPropertiesList[gamerIndex];
                gamerProperties.rematch = true;
                await this.hubContext.Clients.Client(this.hubConnectionIDsList[gamerProperties.gid]).SendAsync("OnGamerRematchSuccess");

                RoomController.ParseRoomTypeLevelFromID(this.properties.ID, out var roomType, out var roomLevel);
                if (this.properties.gamersPropertiesList.FindAll(e => e.rematch).Count == (int)roomType)
                {
                    this.properties.Init();
                    this.replayData = BattleReplayMongoDB.GetByBattleID(this.properties.ID);
                    this.replayData.stepsList.Clear();
                    foreach (var _gamerProperties in this.properties.gamersPropertiesList)
                    {
                        _gamerProperties.Init(this.roomConfig);
                    }
                    //this.ProcessState(BattleState.BUY_BOOSTER);
                }
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }
        #endregion

        protected void SyncBattleProperties()
        {
            this.hubContext.Clients.Group(this.roomKey).SendAsync("UpdateBattleProperties", this.properties);
        }

        protected GamerBattleProperty GetGamerByColor(GamerColor _color)
        {
            return this.properties.gamersPropertiesList.Find(e => e.color == _color);
        }
    }
}