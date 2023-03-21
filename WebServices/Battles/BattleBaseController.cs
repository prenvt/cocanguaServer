using System;
using Microsoft.AspNetCore.SignalR;
using WebServices.Hubs;
using WebServices;
using System.Collections.Generic;
using CBShare.Battle;
using CBShare.Common;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using CTPServer.MongoDB;
using CBShare.Configuration;
using System.Linq;
using CBShare.Data;
using LitJson;
using MongoDB.Bson;
using System.Security.Cryptography;

namespace WebServices.Battles
{
    public class BattleBaseController : IDisposable
    {
        protected static object syncObj = new object();
        public BattleProperty properties { get; set; }
        protected RoomConfig roomConfig { get; set; }
        protected BattleReplayData replayData { get; set; }
        protected int lastReplayStepsCount;
        protected float lastBattleTime;
        public string roomKey { get; set; }
        protected Dictionary<long, string> hubConnectionIDsList = new Dictionary<long, string>();
        protected IHubContext<BattleHub> hubContext;
        protected readonly System.Timers.Timer updateTimer = new System.Timers.Timer();
        protected float saveRoomElapsedTime = 0f;
        //protected BattleWaitingActionController waitingActionController;
        protected BattleGamerActionData waitingGamerAction { get; set; }
        protected GamerBattleProperty currentTurnGamer { get { return this.properties.gamersPropertiesList[this.properties.turnGamerIndex]; } }
        //protected bool needProcessAFK = false;
        protected float updateDeltaTime = 0.2f;
        protected float elapsedTime = 0f;

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
            this.replayData = BattleReplayMongoDB.GetByBattleID(_props.ID);
            this.replayData.stepsList.Clear();
            this.roomKey = string.Format("CTP_{0}", _props.ID);
            this.roomConfig = ConfigManager.instance.GetRoomConfig(_props.level);
        }

        protected virtual void Update(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (this.properties != null && this.properties.nextState != BattleState.NONE)
                {
                    this.elapsedTime += this.updateDeltaTime;
                    if (this.elapsedTime >= this.properties.nextStateTime)
                    {
                        this.ProcessState(this.properties.nextState);
                        //this.properties.nextState = BattleState.NONE;
                        //this.elapsedTime = 0f;
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

        protected void ProcessState(BattleState _state)
        {
            try
            {
                this.properties.state = _state;
                this.properties.nextState = BattleState.NONE;
                switch (_state)
                {
                    case BattleState.BUY_ACTION_CARD:
                        {
                            var waitingTime = ConfigManager.instance.battleConfig.waitTimes[BattleState.BUY_ACTION_CARD.ToString()];
                            for (int i = 0; i < this.properties.gamersPropertiesList.Count; i++)
                            {
                                var gamerIndex = i;
                                var gamerProperties = this.properties.gamersPropertiesList[i];
                                this.hubContext.Clients.Client(this.hubConnectionIDsList[gamerProperties.gid]).SendAsync("WaitingBuyActionCard", gamerIndex, waitingTime, gamerProperties);
                            }
                            this.SetNextState(BattleState.START_BATTLE, waitingTime);
                            this.properties.battleTime += waitingTime;
                        }
                        break;

                    case BattleState.START_BATTLE:
                        {
                            this.properties.turnGamerIndex = RandomUtils.GetRandomInt(0, 2);
                            this.properties.firstTurnGamerIndex = this.properties.turnGamerIndex;
                            var actionCardCfg = ConfigManager.instance.GetActionCardConfig(ActionCardCode.StartGift);
                            for (int i = 0; i < this.properties.gamersPropertiesList.Count; i++)
                            {
                                var gamerProperties = this.properties.gamersPropertiesList[i];
                                gamerProperties.rematch = false;
                                if (gamerProperties.actionCardsList.ContainsKey(ActionCardCode.StartGift.ToString()))
                                {
                                    var giftCash = gamerProperties.cash * actionCardCfg.value / 100;
                                    gamerProperties.AddStartGift(giftCash);
                                    gamerProperties.actionCardsList[ActionCardCode.StartGift.ToString()] = false;
                                }
                                gamerProperties.startCash = gamerProperties.cash;
                            }
                            this.hubContext.Clients.Group(this.roomKey).SendAsync("StartBattle", this.properties);
                            this.SetNextState(BattleState.START_TURN, 1f);
                        }
                        break;

                    case BattleState.START_TURN:
                        {
                            this.lastReplayStepsCount = this.replayData.stepsList.Count;
                            this.lastBattleTime = this.properties.battleTime;
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

        protected void SetNextState(BattleState _state, float _delayTime)
        {
            this.elapsedTime = 0f;
            this.properties.nextStateTime = _delayTime;
            this.properties.nextState = _state;
            BattleMongoDB.Save(this.properties);
        }

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
                this.currentTurnGamer.isRollingDoubleDices = false;
                this.currentTurnGamer.isWaitingUseCharacterSkill = false;
                this.properties.ProcessSortGamersByAsset();
                //this.SyncBattlePropertiesToAllGamers();
                //this.lastReplayStepsCount = this.replayData.stepsList.Count;
                //this.lastBattleTime = this.properties.battleTime;
                this.SendWaitingGamerAction(new BattleGamerActionData()
                {
                    actionType = BattleGamerAction.RollDice,
                    indexInBattle = this.properties.turnGamerIndex,
                    jsonValue = JsonMapper.ToJson(new RollDiceActionParameter()
                    {
                        isSpecial = false
                    })
                });
                //if (this.needProcessAFK)
                {
                    //await Task.Delay((int)(waitingTime * 1000) + 250);
                    //this.OnGamerRollDice(this.properties.turnGamerIndex, false, true);
                }
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        protected bool CheckValidWaitingGamerAction(BattleGamerAction _actionType, int _gamerIndex)
        {
            lock (syncObj)
            {
                if (this.waitingGamerAction == null)
                {
                    return false;
                }
                if (this.waitingGamerAction.actionType != _actionType)
                {
                    return false;
                }
                if (this.waitingGamerAction.indexInBattle != _gamerIndex)
                {
                    return false;
                }
                this.waitingGamerAction = null;
                return true;
            }
        }

        protected void ProcessMoveCharacterToBlock(int _destBlockIndex, CharacterCode _skillCharacter = CharacterCode.NONE)
        {
            try
            {
                if (_destBlockIndex < this.currentTurnGamer.currentBlockIndex)
                {
                    var moveTime = ConfigManager.instance.battleConfig.TIME_MOVE_CHARACTER_PER_STEP * (this.properties.blocksList.Count - this.currentTurnGamer.currentBlockIndex);
                    this.AddReplayStep(ReplayStepType.MoveCharacterToBlock, this.properties.turnGamerIndex, new MoveCharacterReplayParameter()
                    {
                        fB = this.currentTurnGamer.currentBlockIndex,
                        dB = 0,
                        sC = _skillCharacter
                    }, moveTime);
                    var addSalary = this.currentTurnGamer.AddSalary(this.roomConfig.salary);
                    this.AddReplayStep(ReplayStepType.ChangeCash, this.properties.turnGamerIndex, new ChangeCashReplayParameter()
                    {
                        aN = CharacterAnim.HAPPY,
                        cV = addSalary,
                        cA = this.currentTurnGamer.asset,
                        cC = this.currentTurnGamer.cash
                    }, 0.5f);
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
                }
                this.ProcessCharacterStayAtBlock(_destBlockIndex);
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        protected void ProcessCharacterStayAtBlock(int _blockIndex)
        {
            this.currentTurnGamer.currentBlockIndex = _blockIndex;
            var block = this.properties.blocksList[_blockIndex];

            switch (block.type)
            {
                case BlockType.GO:
                    {
                        if (this.CheckTurnGamerHasActionCard(ActionCardCode.BonusSalaryWhenAtGO))
                        {
                            //this.SendWaitingGamerAction(BattleGamerAction.UseActionCard, ActionCardCode.BonusSalaryWhenAtGO);
                            this.SendWaitingGamerAction(new BattleGamerActionData()
                            {
                                actionType = BattleGamerAction.UseActionCard,
                                indexInBattle = this.properties.turnGamerIndex,
                                jsonValue = JsonMapper.ToJson(new UseActionCardActionParameter()
                                {
                                    actionCard = ActionCardCode.BonusSalaryWhenAtGO
                                })
                            });
                        }
                        else
                        {
                            this.ContinueTurnGamerAction();
                        }
                    }
                    break;

                case BlockType.Chance:
                    {
                        if (this.properties.chanceCardsList.Count > 0)
                        {
                            //this.SetNextState(BattleState.DRAW_CHANCE_CARD, 0.25f);
                            this.ProcessDrawChanceCard();
                        }
                        else
                        {
                            //this.SetNextState(BattleState.CONTINUE_TURN, 0.25f);
                            this.ContinueTurnGamerAction();
                        }
                    }
                    break;

                case BlockType.Park:
                    {
                        var turnGamerBlockIndexsList = this.properties.GetBlockIndexsByGamer(this.properties.turnGamerIndex);
                        if (turnGamerBlockIndexsList.Count > 0)
                        {
                            //this.SendWaitingGamerAction(BattleGamerAction.SelectBlock, turnGamerBlockIndexsList, SelectBlockActionCode.SET_CASINO);
                            this.SendWaitingGamerAction(new BattleGamerActionData()
                            {
                                actionType = BattleGamerAction.SelectBlock,
                                indexInBattle = this.properties.turnGamerIndex,
                                jsonValue = JsonMapper.ToJson(new SelectBlockActionParameter()
                                {
                                    blockIndexsList = turnGamerBlockIndexsList,
                                    selectAction = SelectBlockActionCode.SET_CASINO
                                })
                            });
                        }
                        else
                        {
                            //this.SetNextState(BattleState.CONTINUE_TURN, 0.25f);
                            this.ContinueTurnGamerAction();
                        }
                    }
                    break;

                case BlockType.Cannon:
                    {
                        var randBlockIndex = RandomUtils.GetRandomWithExcepts(this.properties.blocksList.Count - 1, new int[] { 0, 3, 6, 9, 12, 15, 18, 20 });
                        this.AddReplayStep(ReplayStepType.CannonShotToBlock, this.properties.turnGamerIndex, new CannonShotToBlockReplayParameter()
                        {
                            bI = randBlockIndex,
                        });
                        this.ProcessDestroyHouseAtBlocks(new List<int>() { randBlockIndex }, false);
                        this.ContinueTurnGamerAction();
                    }
                    break;

                case BlockType.Tornado:
                    {
                        int randBlockIndex = RandomUtils.GetRandomWithExcepts(this.properties.blocksList.Count - 1, new int[] { 6 });
                        this.AddReplayStep(ReplayStepType.FallCharacterToBlock, this.properties.turnGamerIndex, new FallToBlockReplayParameter()
                        {
                            fB = this.currentTurnGamer.currentBlockIndex,
                            dB = randBlockIndex,
                        }, 1.5f);
                        this.ProcessCharacterStayAtBlock(randBlockIndex);
                    }
                    break;

                case BlockType.House:
                    {
                        var opponentGamerIndex = this.properties.gamersPropertiesList.Count - 1 - this.properties.turnGamerIndex;
                        if (block.ownerIndex == opponentGamerIndex)
                        {
                            if (block.GetTollRate() > 0f)
                            {
                                if (this.CheckTurnGamerHasActionCard(ActionCardCode.FreeTaxes))
                                {
                                    //this.SendWaitingGamerAction(BattleGamerAction.UseActionCard, ActionCardCode.FreeTaxes);
                                    this.SendWaitingGamerAction(new BattleGamerActionData()
                                    {
                                        actionType = BattleGamerAction.UseActionCard,
                                        indexInBattle = this.properties.turnGamerIndex,
                                        jsonValue = JsonMapper.ToJson(new UseActionCardActionParameter()
                                        {
                                            actionCard = ActionCardCode.FreeTaxes
                                        })
                                    });
                                }
                                else
                                {
                                    this.ProcessWhenStayAtOpponentBlock(false);
                                }
                            }
                            else
                            {
                                this.ContinueTurnGamerAction();
                            }
                        }
                        // Block is Empty.
                        else if (block.ownerIndex < 0)
                        {
                            var discountHouseCostPercent = this.currentTurnGamer.GetDiscountHouseCostPercent();
                            var msg = "";
                            if (this.CheckTurnGamerCanBuildHouseAtBlock(block, discountHouseCostPercent, out msg))
                            {
                                if (this.CheckTurnGamerHasActionCard(ActionCardCode.DiscountHouseCost))
                                {
                                    //this.SendWaitingGamerAction(BattleGamerAction.UseActionCard, ActionCardCode.DiscountHouseCost);
                                    this.SendWaitingGamerAction(new BattleGamerActionData()
                                    {
                                        actionType = BattleGamerAction.UseActionCard,
                                        indexInBattle = this.properties.turnGamerIndex,
                                        jsonValue = JsonMapper.ToJson(new UseActionCardActionParameter()
                                        {
                                            actionCard = ActionCardCode.DiscountHouseCost
                                        })
                                    });
                                }
                                else
                                {
                                    this.SendWaitingGamerAction(new BattleGamerActionData()
                                    {
                                        actionType = BattleGamerAction.BuildHouse,
                                        indexInBattle = this.properties.turnGamerIndex,
                                        jsonValue = JsonMapper.ToJson(new BuildHouseActionParameter()
                                        {
                                            blockIndex = _blockIndex,
                                            currentHouse = block.currentHouseCode,
                                            discountHouseCostPercent = discountHouseCostPercent
                                        })
                                    });
                                }
                            }
                            else
                            {
                                this.AddReplayStep(ReplayStepType.ShowMessageOnChracterHead, this.properties.turnGamerIndex, new ShowMessageOnHeadReplayParameter()
                                {
                                    m = msg
                                });
                                this.ContinueTurnGamerAction();
                            }
                        }
                        else if (block.ownerIndex == this.properties.turnGamerIndex)
                        {
                            if (this.CheckTurnGamerHasActionCard(ActionCardCode.StarCity))
                            {
                                //this.SendWaitingGamerAction(BattleGamerAction.UseActionCard, ActionCardCode.StarCity);
                                this.SendWaitingGamerAction(new BattleGamerActionData()
                                {
                                    actionType = BattleGamerAction.UseActionCard,
                                    indexInBattle = this.properties.turnGamerIndex,
                                    jsonValue = JsonMapper.ToJson(new UseActionCardActionParameter()
                                    {
                                        actionCard = ActionCardCode.StarCity
                                    })
                                });
                            }
                            else
                            {
                                this.ProcessWhenStayAtMineBlock();
                            }
                        }
                    }
                    break;
            }
        }

        protected void ProcessWhenStayAtMineBlock(float _discountHouseCostByActionCard = 0f)
        {
            var discountHouseCostPercent = this.currentTurnGamer.GetDiscountHouseCostPercent();
            var msg = "";
            var block = this.properties.blocksList[this.currentTurnGamer.currentBlockIndex];
            if (this.CheckTurnGamerCanBuildHouseAtBlock(block, discountHouseCostPercent, out msg))
            {
                if (this.CheckTurnGamerHasActionCard(ActionCardCode.DiscountHouseCost))
                {
                    //this.SendWaitingGamerAction(BattleGamerAction.UseActionCard, ActionCardCode.DiscountHouseCost);
                    this.SendWaitingGamerAction(new BattleGamerActionData()
                    {
                        actionType = BattleGamerAction.UseActionCard,
                        indexInBattle = this.properties.turnGamerIndex,
                        jsonValue = JsonMapper.ToJson(new UseActionCardActionParameter()
                        {
                            actionCard = ActionCardCode.DiscountHouseCost
                        })
                    });
                }
                else
                {
                    this.SendWaitingGamerAction(new BattleGamerActionData()
                    {
                        actionType = BattleGamerAction.BuildHouse,
                        indexInBattle = this.properties.turnGamerIndex,
                        jsonValue = JsonMapper.ToJson(new BuildHouseActionParameter()
                        {
                            blockIndex = block.index,
                            currentHouse = block.currentHouseCode,
                            discountHouseCostPercent = discountHouseCostPercent
                        })
                    });
                }
            }
            this.AddReplayStep(ReplayStepType.ShowMessageOnChracterHead, this.properties.turnGamerIndex, new ShowMessageOnHeadReplayParameter()
            {
                m = msg
            }, 1f);
            this.ContinueTurnGamerAction();
        }

        protected void ContinueTurnGamerAction(params Block[] checkingBlocks)
        {
            try
            {
                if (checkingBlocks.Length > 0)
                {
                    for (int i = 0; i < checkingBlocks.Length; i++)
                    {
                        var checkingBlock = checkingBlocks[i];
                        if (checkingBlock.ownerIndex < 0) continue;
                        var blockCfg = ConfigManager.instance.GetBlockConfig(checkingBlock.index);

                        var numBlockOwned = 0;
                        var warningBlockIndex = -1;
                        for (int j = 0; j < blockCfg.monopolyIndexs.Count; j++)
                        {
                            var blockIndex = blockCfg.monopolyIndexs[j];
                            var block = this.properties.blocksList[blockIndex];
                            if (block.ownerIndex >= 0 && block.ownerIndex == checkingBlock.ownerIndex)
                            {
                                numBlockOwned++;
                            }
                            else
                            {
                                warningBlockIndex = blockIndex;
                            }
                        }
                        if (numBlockOwned == blockCfg.monopolyIndexs.Count)
                        {
                            this.ProcessEndBattle(EndBattleType.MONOPOLY, checkingBlock.ownerIndex);
                            return;
                        }
                        else if (numBlockOwned == blockCfg.monopolyIndexs.Count - 1)
                        {
                            this.AddReplayStep(ReplayStepType.ShowWarning, this.properties.turnGamerIndex, new ShowWarningReplayParameter()
                            {
                                wT = BattleWarningType.WARNING_MONOPOLY,
                                bI = warningBlockIndex
                            });
                        }
                    }
                }

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
        }

        protected void ProcessEndTurn()
        {
            try
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
                        //this.SendWaitingGamerAction(BattleGamerAction.UseCharacterSkill);
                        this.SendWaitingGamerAction(new BattleGamerActionData()
                        {
                            actionType = BattleGamerAction.UseCharacterSkill,
                            indexInBattle = this.properties.turnGamerIndex,
                            jsonValue = JsonMapper.ToJson(new UseCharacterSkillActionParameter()
                            {
                                character = this.currentTurnGamer.currentCharacter
                            })
                        });
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
                        this.ProcessEndBattle(EndBattleType.TURN_OFF, -1);
                    }
                    else
                    {
                        if (this.properties.turnCount == this.roomConfig.maxTurn - 3)
                        {
                            this.AddReplayStep(ReplayStepType.ShowWarning, this.properties.turnGamerIndex, new ShowWarningReplayParameter()
                            {
                                wT = BattleWarningType.WARNING_REMAIN_3_TURN,
                                bI = -1
                            });
                        }
                        this.properties.turnCount++;
                        //this.SetNextState(BattleState.START_TURN, this.updateDeltaTime);
                        this.ProcessStartTurn();
                    }
                }
                else
                {
                    //this.SetNextState(BattleState.START_TURN, this.updateDeltaTime);
                    this.ProcessStartTurn();
                }
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        protected void ProcessDestroyHouseAtBlocks(List<int> blockIndexsList, bool isSell)
        {
            try
            {
                GamerBattleProperty ownerGamer = null;
                var totalSellCash = 0;
                foreach (var blockIndex in blockIndexsList)
                {
                    var block = this.properties.blocksList[blockIndex];
                    if (block.currentHouseCode == HouseCode.NONE)
                        return;
                    if (block.ownerIndex < 0)
                        return;
                    var sellHousePrice = 0;
                    ownerGamer = this.properties.gamersPropertiesList[block.ownerIndex];
                    //ownerGamer.ownedBlockIndexsList.Remove(blockIndex);

                    this.AddReplayStep(ReplayStepType.SetHouseAtBlock, block.ownerIndex, new SetHouseAtBlockReplayParameter()
                    {
                        bI = blockIndex,
                        hC = HouseCode.NONE
                    });

                    var blockCfg = ConfigManager.instance.GetBlockConfig(blockIndex);
                    if (blockCfg.coupleIndex > 0)
                    {
                        var coupleBlock = this.properties.blocksList[blockCfg.coupleIndex];
                        coupleBlock.isCouple = false;
                    }
                    var houseCost = blockCfg.GetHouseCost(block.currentHouseCode);
                    ownerGamer.asset -= houseCost;
                    if (isSell)
                    {
                        var reduceSellHouseFeesPercent = ownerGamer.currentStarCard.GetStatValue(StarCardStat.ReduceSellHouseFees);
                        sellHousePrice = (int)((50f + reduceSellHouseFeesPercent) * houseCost / 100f);
                        //ownerGamer.properties.asset -= houseCost;
                        ownerGamer.AddCash(sellHousePrice);
                        totalSellCash += sellHousePrice;
                    }
                    block.DestroyHouses();
                }
                if (isSell)
                {
                    this.AddReplayStep(ReplayStepType.ChangeCash, ownerGamer.indexInBattle, new ChangeCashReplayParameter()
                    {
                        aN = CharacterAnim.SAD,
                        cV = totalSellCash,
                        cA = ownerGamer.asset,
                        cC = ownerGamer.cash
                    });
                }
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        protected void ProcessWhenStayAtOpponentBlock(bool isFreeToll)
        {
            try
            {
                var blockIndex = this.currentTurnGamer.currentBlockIndex;
                Block block = this.properties.blocksList[blockIndex];
                var blockCfg = ConfigManager.instance.GetBlockConfig(blockIndex);
                var reduceTollPercent = this.currentTurnGamer.currentStarCard.GetStatValue(StarCardStat.ReduceToll);
                var blockToll = blockCfg.GetHouseToll(block.currentHouseCode);
                var tollsNeedPay = (int)((100f - reduceTollPercent) * blockToll / 100 * block.GetTollRate() * this.currentTurnGamer.GetTollRate());
                if (isFreeToll)
                {
                    tollsNeedPay = 0;
                }
                var opponentGamerIndex = this.properties.gamersPropertiesList.Count - 1 - this.properties.turnGamerIndex;
                if (tollsNeedPay == 0)
                {
                    //this.properties.battleTime += 1f;
                    this.ContinueTurnGamerAction();
                }
                else if (this.currentTurnGamer.cash >= tollsNeedPay)
                {
                    this.ProcessSendCash(this.properties.turnGamerIndex, opponentGamerIndex, tollsNeedPay);
                    //this.properties.battleTime += 1f;
                    this.ContinueTurnGamerAction();
                }
                else
                {
                    this.currentTurnGamer.currentTollsNeedPay = tollsNeedPay;
                    var sellAllHousesCashes = this.GetCashFromSellAllHousesOfGamer(this.properties.turnGamerIndex);
                    if (this.currentTurnGamer.cash + sellAllHousesCashes > tollsNeedPay)
                    {
                        //this.SendWaitingGamerAction(BattleGamerAction.SellHouses, missingTolls);
                        var missingTolls = tollsNeedPay - this.currentTurnGamer.cash;
                        this.SendWaitingGamerAction(new BattleGamerActionData()
                        {
                            actionType = BattleGamerAction.SellHouses,
                            indexInBattle = this.properties.turnGamerIndex,
                            jsonValue = JsonMapper.ToJson(new SellHouseActionParameter()
                            {
                                blockIndexsList = this.properties.GetBlockIndexsByGamer(this.properties.turnGamerIndex),
                                defaultBlockIndexsList = this.GetSellBlocksListEnoughtPayTolls(this.properties.turnGamerIndex, missingTolls),
                                missingTolls = missingTolls,
                                reduceSellHouseFeesPercent = this.currentTurnGamer.currentStarCard.GetStatValue(StarCardStat.ReduceSellHouseFees)
                            })
                        });
                    }
                    else
                    {
                        if (sellAllHousesCashes > 0)
                        {
                            var sellBlockIndexsList = this.properties.GetBlockIndexsByGamer(this.properties.turnGamerIndex);
                            for (int i = 0; i < sellBlockIndexsList.Count; i++)
                            {
                                var sellBlockIndex = sellBlockIndexsList[i];
                                this.ProcessDestroyHouseAtBlocks(new List<int>() { sellBlockIndex }, true);
                            }
                            //this.properties.battleTime += 1f;
                        }
                        this.ProcessSendCash(this.properties.turnGamerIndex, opponentGamerIndex, this.currentTurnGamer.cash);
                        this.ProcessEndBattle(EndBattleType.BANKRUPT, opponentGamerIndex);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        protected void ProcessSetBlockForCharacterSkill(int _blockIndex)
        {
            try
            {
                this.AddReplayStep(ReplayStepType.SetBlockForCharacterSkill, this.properties.turnGamerIndex, new SetBlockForCharacterSkillReplayParameter()
                {
                    bI = _blockIndex,
                }, 0.5f);
                this.ProcessMoveCharacterToBlock(_blockIndex, this.currentTurnGamer.currentCharacter);
                switch (this.currentTurnGamer.currentCharacter)
                {
                    case CharacterCode.PHI_CONG:
                        {

                        }
                        break;

                    case CharacterCode.DOANH_NHAN:
                        {

                        }
                        break;
                }
                
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        protected void ProcessDrawChanceCard()
        {
            try
            {
                var chanceCardCode = ChanceCardCode.NONE;
                if (this.currentTurnGamer.testChanceCard > ChanceCardCode.NONE)
                {
                    // ThangNV. Test
                    chanceCardCode = this.currentTurnGamer.testChanceCard;
                }
                else
                {
                    var remainChanceCardsList = new List<string>();
                    foreach (var cardCode in this.properties.chanceCardsList.Keys)
                    {
                        if (this.properties.chanceCardsList[cardCode]) continue;
                        remainChanceCardsList.Add(cardCode);
                    }
                    if (remainChanceCardsList.Count == 0)
                        return;
                    var randIndex = RandomUtils.GetRandomInt(0, remainChanceCardsList.Count - 1);
                    chanceCardCode = UtilsHelper.ParseEnum<ChanceCardCode>(remainChanceCardsList[randIndex]);
                }

                this.properties.chanceCardsList[chanceCardCode.ToString()] = true;
                this.AddReplayStep(ReplayStepType.DrawChanceCard, this.properties.turnGamerIndex, new DrawChanceCardReplayParameter()
                {
                    cC = chanceCardCode
                }, 2.5f);
                switch (chanceCardCode)
                {
                    case ChanceCardCode.MoveForward_1:
                        {
                            var numSteps = 1;
                            var destBlockIndex = (this.currentTurnGamer.currentBlockIndex + numSteps) % this.properties.blocksList.Count;
                            this.ProcessMoveCharacterToBlock(destBlockIndex);
                            break;
                        }

                    case ChanceCardCode.MoveForward_2:
                        {
                            var numSteps = 2;
                            var destBlockIndex = (this.currentTurnGamer.currentBlockIndex + numSteps) % this.properties.blocksList.Count;
                            this.ProcessMoveCharacterToBlock(destBlockIndex);
                            break;
                        }

                    /*case ChanceCardCode.MoveBack:
                        {
                            var numSteps = (int)chanceCardCfg.parameters[0];
                            var destBlockIndex = (this.currentTurnGamer.properties.currentBlockIndex + numSteps) % this.properties.blocksList.Count;
                            this.ProcessMoveCharacterToBlock(destBlockIndex);
                            break;
                        }*/

                    case ChanceCardCode.GoToCannon:
                        {
                            var destBlockIndex = ConfigManager.instance.FindBlockIndexByType(BlockType.Cannon);
                            this.ProcessMoveCharacterToBlock(destBlockIndex);
                            break;
                        }

                    case ChanceCardCode.GoToTornado:
                        {
                            var destBlockIndex = ConfigManager.instance.FindBlockIndexByType(BlockType.Tornado);
                            this.ProcessMoveCharacterToBlock(destBlockIndex);
                            break;
                        }

                    case ChanceCardCode.GoToPark:
                        {
                            var destBlockIndex = ConfigManager.instance.FindBlockIndexByType(BlockType.Park);
                            this.ProcessMoveCharacterToBlock(destBlockIndex);
                            break;
                        }

                    case ChanceCardCode.OrganizeOlympic:
                        {
                            var turnGamerBlockIndexsList = this.properties.GetBlockIndexsByGamer(this.properties.turnGamerIndex);
                            if (turnGamerBlockIndexsList.Count > 0)
                            {
                                //this.SendWaitingGamerAction(BattleGamerAction.SelectBlock, turnGamerBlockIndexsList, SelectBlockActionCode.SET_OLYMPIC);
                                this.SendWaitingGamerAction(new BattleGamerActionData()
                                {
                                    actionType = BattleGamerAction.SelectBlock,
                                    indexInBattle = this.properties.turnGamerIndex,
                                    jsonValue = JsonMapper.ToJson(new SelectBlockActionParameter()
                                    {
                                        blockIndexsList = turnGamerBlockIndexsList,
                                        selectAction = SelectBlockActionCode.SET_OLYMPIC
                                    })
                                });
                            }
                            else
                            {
                                this.ContinueTurnGamerAction();
                            }
                            break;
                        }

                    case ChanceCardCode.GoToOlympic:
                        {
                            var olympicBlock = this.properties.blocksList.Find(e => e.isOlympic);
                            if (olympicBlock != null)
                            {
                                this.ProcessMoveCharacterToBlock(olympicBlock.index);
                            }
                            else
                            {
                                this.ContinueTurnGamerAction();
                            }
                            break;
                        }

                    case ChanceCardCode.OrganizeFestival:
                        {
                            var turnGamerBlockIndexsList = this.properties.GetBlockIndexsByGamer(this.properties.turnGamerIndex);
                            if (turnGamerBlockIndexsList.Count > 0)
                            {
                                //this.SendWaitingGamerAction(BattleGamerAction.SelectBlock, turnGamerBlockIndexsList, SelectBlockActionCode.SET_FESTIVAL);
                                this.SendWaitingGamerAction(new BattleGamerActionData()
                                {
                                    actionType = BattleGamerAction.SelectBlock,
                                    indexInBattle = this.properties.turnGamerIndex,
                                    jsonValue = JsonMapper.ToJson(new SelectBlockActionParameter()
                                    {
                                        blockIndexsList = turnGamerBlockIndexsList,
                                        selectAction = SelectBlockActionCode.SET_FESTIVAL
                                    })
                                });
                            }
                            else
                            {
                                this.ContinueTurnGamerAction();
                            }
                            break;
                        }

                    case ChanceCardCode.GoToFestival:
                        {
                            var festivalBlock = this.properties.blocksList.Find(e => e.isFestival);
                            if (festivalBlock != null)
                            {
                                this.ProcessMoveCharacterToBlock(festivalBlock.index);
                            }
                            else
                            {
                                this.ContinueTurnGamerAction();
                            }
                            break;
                        }

                    case ChanceCardCode.SetStarCity:
                        {
                            var turnGamerBlockIndexsList = this.properties.GetBlockIndexsByGamer(this.properties.turnGamerIndex);
                            if (turnGamerBlockIndexsList.Count > 0)
                            {
                                //this.SendWaitingGamerAction(BattleGamerAction.SelectBlock, turnGamerBlockIndexsList, SelectBlockActionCode.SET_STAR_CITY);
                                this.SendWaitingGamerAction(new BattleGamerActionData()
                                {
                                    actionType = BattleGamerAction.SelectBlock,
                                    indexInBattle = this.properties.turnGamerIndex,
                                    jsonValue = JsonMapper.ToJson(new SelectBlockActionParameter()
                                    {
                                        blockIndexsList = turnGamerBlockIndexsList,
                                        selectAction = SelectBlockActionCode.SET_STAR_CITY
                                    })
                                });
                            }
                            else
                            {
                                this.ContinueTurnGamerAction();
                            }
                            break;
                        }

                    case ChanceCardCode.GoToStarCity:
                        {
                            var starCityBlock = this.properties.blocksList.Find(e => e.isStarCity);
                            if (starCityBlock != null)
                            {
                                this.ProcessMoveCharacterToBlock(starCityBlock.index);
                            }
                            else
                            {
                                this.ContinueTurnGamerAction();
                            }
                            break;
                        }

                    case ChanceCardCode.DowngradeOpponentHouse:
                        {
                            var opponentGamerIndex = this.properties.gamersPropertiesList.Count - 1 - this.properties.turnGamerIndex;
                            var opponentGamerBlockIndexsList = this.properties.GetBlockIndexsByGamer(opponentGamerIndex);
                            if (opponentGamerBlockIndexsList.Count > 0)
                            {
                                //this.SendWaitingGamerAction(BattleGamerAction.SelectBlock, opponentGamerBlockIndexsList, SelectBlockActionCode.DOWNGRADE_OPPONENT_HOUSE);
                                this.SendWaitingGamerAction(new BattleGamerActionData()
                                {
                                    actionType = BattleGamerAction.SelectBlock,
                                    indexInBattle = this.properties.turnGamerIndex,
                                    jsonValue = JsonMapper.ToJson(new SelectBlockActionParameter()
                                    {
                                        blockIndexsList = opponentGamerBlockIndexsList,
                                        selectAction = SelectBlockActionCode.DOWNGRADE_OPPONENT_HOUSE
                                    })
                                });
                            }
                            else
                            {
                                this.ContinueTurnGamerAction();
                            }
                            break;
                        }

                    case ChanceCardCode.ReduceOpponentBlockToll:
                        {
                            var opponentGamerIndex = this.properties.gamersPropertiesList.Count - 1 - this.properties.turnGamerIndex;
                            var opponentGamerBlockIndexsList = this.properties.GetBlockIndexsByGamer(opponentGamerIndex);
                            if (opponentGamerBlockIndexsList.Count > 0)
                            {
                                //this.SendWaitingGamerAction(BattleGamerAction.SelectBlock, opponentGamerBlockIndexsList, SelectBlockActionCode.REDUCE_OPPONENT_BLOCK_TOLL);
                                this.SendWaitingGamerAction(new BattleGamerActionData()
                                {
                                    actionType = BattleGamerAction.SelectBlock,
                                    indexInBattle = this.properties.turnGamerIndex,
                                    jsonValue = JsonMapper.ToJson(new SelectBlockActionParameter()
                                    {
                                        blockIndexsList = opponentGamerBlockIndexsList,
                                        selectAction = SelectBlockActionCode.REDUCE_OPPONENT_BLOCK_TOLL
                                    })
                                });
                            }
                            else
                            {
                                this.ContinueTurnGamerAction();
                            }
                            break;
                        }

                    case ChanceCardCode.SellOpponentHouse:
                        {
                            var opponentGamerIndex = this.properties.gamersPropertiesList.Count - 1 - this.properties.turnGamerIndex;
                            var opponentGamerBlockIndexsList = this.properties.GetBlockIndexsByGamer(opponentGamerIndex);
                            if (opponentGamerBlockIndexsList.Count > 0)
                            {
                                //this.SendWaitingGamerAction(BattleGamerAction.SelectBlock, opponentGamerBlockIndexsList, SelectBlockActionCode.SELL_OPPONENT_HOUSE);
                                this.SendWaitingGamerAction(new BattleGamerActionData()
                                {
                                    actionType = BattleGamerAction.SelectBlock,
                                    indexInBattle = this.properties.turnGamerIndex,
                                    jsonValue = JsonMapper.ToJson(new SelectBlockActionParameter()
                                    {
                                        blockIndexsList = opponentGamerBlockIndexsList,
                                        selectAction = SelectBlockActionCode.SELL_OPPONENT_HOUSE
                                    })
                                });
                            }
                            else
                            {
                                this.ContinueTurnGamerAction();
                            }
                            break;
                        }

                    case ChanceCardCode.IncreaseAllBlocksToll:
                        {
                            this.currentTurnGamer.tollRatesByTurn.Add(new TollRateByTurnData()
                            {
                                rate = 2f,
                                turn = 3
                            });
                            //this.properties.battleTime += 1f;
                            //await Task.Delay(1000);
                            this.ContinueTurnGamerAction();
                            break;
                        }

                    case ChanceCardCode.ExchangeBlocks:
                        {
                            var turnGamerBlockIndexsList = this.properties.GetBlockIndexsByGamer(this.properties.turnGamerIndex);
                            var opponentGamerIndex = this.properties.gamersPropertiesList.Count - 1 - this.properties.turnGamerIndex;
                            var opponentGamerBlockIndexsList = this.properties.GetBlockIndexsByGamer(opponentGamerIndex);
                            if (turnGamerBlockIndexsList.Count > 0 && opponentGamerBlockIndexsList.Count > 0)
                            {
                                //this.SendWaitingGamerAction(BattleGamerAction.ExchangeBlocks);
                                this.SendWaitingGamerAction(new BattleGamerActionData()
                                {
                                    actionType = BattleGamerAction.ExchangeBlocks,
                                    indexInBattle = this.properties.turnGamerIndex,
                                    jsonValue = JsonMapper.ToJson(new ExchangeBlocksActionParameter()
                                    {
                                        gamer0_BlockIndexsList = this.properties.GetBlockIndexsByGamer(0),
                                        gamer1_BlockIndexsList = this.properties.GetBlockIndexsByGamer(1)
                                    })
                                });
                            }
                            else
                            {
                                this.ContinueTurnGamerAction();
                            }
                            break;
                        }

                    case ChanceCardCode.Help:
                        {
                            var gamerPropertiesList = new List<GamerBattleProperty>();
                            for (int i = 0; i < this.properties.gamersPropertiesList.Count; i++)
                            {
                                var gamerProperties = this.properties.gamersPropertiesList[i];
                                gamerPropertiesList.Add(gamerProperties);
                            }
                            gamerPropertiesList = gamerPropertiesList.OrderByDescending(e => e.asset).ToList();
                            var helpCash = (int)(gamerPropertiesList[0].cash * 30 / 100f);
                            this.ProcessSendCash(gamerPropertiesList[0].indexInBattle, gamerPropertiesList.Last().indexInBattle, helpCash);
                            this.ContinueTurnGamerAction();
                            break;
                        }

                    case ChanceCardCode.Donate:
                        {
                            var turnGamerBlockIndexsList = this.properties.GetBlockIndexsByGamer(this.properties.turnGamerIndex);
                            if (turnGamerBlockIndexsList.Count > 0)
                            {
                                //this.SendWaitingGamerAction(BattleGamerAction.SelectBlock, turnGamerBlockIndexsList, SelectBlockActionCode.SET_DONATE);
                                this.SendWaitingGamerAction(new BattleGamerActionData()
                                {
                                    actionType = BattleGamerAction.SelectBlock,
                                    indexInBattle = this.properties.turnGamerIndex,
                                    jsonValue = JsonMapper.ToJson(new SelectBlockActionParameter()
                                    {
                                        blockIndexsList = turnGamerBlockIndexsList,
                                        selectAction = SelectBlockActionCode.SET_DONATE
                                    })
                                });
                            }
                            else
                            {
                                this.ContinueTurnGamerAction();
                            }
                            break;
                        }

                    case ChanceCardCode.RagsToRich:
                        {
                            var gamerPropertiesList = new List<GamerBattleProperty>();
                            for (int i = 0; i < this.properties.gamersPropertiesList.Count; i++)
                            {
                                var gamerProperties = this.properties.gamersPropertiesList[i];
                                gamerPropertiesList.Add(gamerProperties);
                            }
                            gamerPropertiesList = gamerPropertiesList.OrderByDescending(e => e.cash).ToList();
                            var shareCash = (int)(gamerPropertiesList[0].cash * 30 / 100f);
                            this.ProcessSendCash(gamerPropertiesList[0].indexInBattle, gamerPropertiesList.Last().indexInBattle, shareCash);
                            this.ContinueTurnGamerAction();
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        protected void ProcessBuildHouseAtBlock(int _ownerGamerIndex, int _blockIndex, HouseCode _selectedHouseCode)
        {
            try
            {
                var block = this.properties.blocksList[_blockIndex];
                var ownerGamer = this.properties.gamersPropertiesList[_ownerGamerIndex];
                //ownerGamer.ownedBlockIndexsList.Add(block.index);
                block.ownerIndex = _ownerGamerIndex;
                block.currentHouseCode = _selectedHouseCode;
                var blockCfg = ConfigManager.instance.GetBlockConfig(_blockIndex);
                var coupleBlock = this.properties.blocksList[blockCfg.coupleIndex];
                if (coupleBlock.ownerIndex == _ownerGamerIndex)
                {
                    block.isCouple = true;
                    coupleBlock.isCouple = true;
                }
                this.AddReplayStep(ReplayStepType.SetHouseAtBlock, _ownerGamerIndex, new SetHouseAtBlockReplayParameter()
                {
                    bI = _blockIndex,
                    hC = _selectedHouseCode
                });
                this.properties.ProcessSortGamersByAsset();
                //this.SyncBattlePropertiesToAllGamers();
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        protected void ProcessSendCash(int sendGamerIndex, int receiveGamerIndex, int cashValue)
        {
            try
            {
                if (cashValue <= 0) return;
                var sendGamer = this.properties.gamersPropertiesList[sendGamerIndex];
                sendGamer.PayToll(cashValue);
                var receiveGamer = this.properties.gamersPropertiesList[receiveGamerIndex];
                receiveGamer.AddCash(cashValue);

                this.AddReplayStep(ReplayStepType.ChangeCash, sendGamer.indexInBattle, new ChangeCashReplayParameter()
                {
                    aN = CharacterAnim.SAD,
                    cV = -cashValue,
                    bI = sendGamer.currentBlockIndex,
                    cA = sendGamer.asset,
                    cC = sendGamer.cash
                });
                this.AddReplayStep(ReplayStepType.ChangeCash, receiveGamer.indexInBattle, new ChangeCashReplayParameter()
                {
                    aN = CharacterAnim.HAPPY,
                    cV = cashValue,
                    bI = receiveGamer.currentBlockIndex,
                    cA = receiveGamer.asset,
                    cC = receiveGamer.cash
                });

            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        protected void ProcessUseCharacterSkill()
        {
            try
            {
                this.currentTurnGamer.ResetSkillTurnCount();
                switch (this.currentTurnGamer.currentCharacter)
                {
                    case CharacterCode.DOANH_NHAN:
                        {
                            var blockIndexsList = new List<int>();
                            var numBlocks = this.properties.blocksList.Count;
                            var ignoreBlockIndexsList = new List<int>() { 0, 6, 12, 18 };
                            var bonusByStarCard = 0;
                            if (this.currentTurnGamer.currentStarCard.characterCode == this.currentTurnGamer.currentCharacter)
                            {
                                bonusByStarCard = (int)this.currentTurnGamer.currentStarCard.GetStatValue(StarCardStat.BonusSkillValue);
                            }
                            for (int i = 1; i <= 3 + bonusByStarCard; i++)
                            {
                                var blockIndex = (this.currentTurnGamer.currentBlockIndex + i) % numBlocks;
                                if (!ignoreBlockIndexsList.Contains(blockIndex))
                                {
                                    blockIndexsList.Add(blockIndex);
                                }
                                blockIndex = (this.currentTurnGamer.currentBlockIndex - i + numBlocks) % numBlocks;
                                if (!ignoreBlockIndexsList.Contains(blockIndex))
                                {
                                    blockIndexsList.Add(blockIndex);
                                }
                            }
                            //this.SendWaitingGamerAction(BattleGamerAction.SelectBlock, blockIndexsList, SelectBlockActionCode.SET_BLOCK_FOR_CHARACTER_SKILL);
                            this.SendWaitingGamerAction(new BattleGamerActionData()
                            {
                                actionType = BattleGamerAction.SelectBlock,
                                indexInBattle = this.properties.turnGamerIndex,
                                jsonValue = JsonMapper.ToJson(new SelectBlockActionParameter()
                                {
                                    blockIndexsList = blockIndexsList,
                                    selectAction = SelectBlockActionCode.SET_BLOCK_FOR_CHARACTER_SKILL
                                })
                            });
                        }
                        break;

                    case CharacterCode.PHI_CONG:
                        {
                            var blockIndexsList = this.properties.GetEmptyHouseBlocks();
                            int rand = RandomUtils.GetRandomIndexInList(blockIndexsList.Count);
                            var randBlockIndex = blockIndexsList[rand];
                            this.currentTurnGamer.discountHouseCostByCharacterSkill = 30;
                            if (this.currentTurnGamer.currentStarCard.characterCode == this.currentTurnGamer.currentCharacter)
                            {
                                this.currentTurnGamer.discountHouseCostByCharacterSkill += this.currentTurnGamer.currentStarCard.GetStatValue(StarCardStat.BonusSkillValue);
                            }
                            this.AddReplayStep(ReplayStepType.SetBlockForCharacterSkill, this.properties.turnGamerIndex, new SetBlockForCharacterSkillReplayParameter()
                            {
                                bI = randBlockIndex,
                            });
                            this.ProcessMoveCharacterToBlock(randBlockIndex, CharacterCode.PHI_CONG);
                        }
                        break;

                    case CharacterCode.CO_GAI:
                        {
                            //this.SendWaitingGamerAction(BattleGamerAction.ExchangeBlocks);
                            this.SendWaitingGamerAction(new BattleGamerActionData()
                            {
                                actionType = BattleGamerAction.ExchangeBlocks,
                                indexInBattle = this.properties.turnGamerIndex,
                                jsonValue = JsonMapper.ToJson(new ExchangeBlocksActionParameter()
                                {
                                    gamer0_BlockIndexsList = this.properties.GetBlockIndexsByGamer(0),
                                    gamer1_BlockIndexsList = this.properties.GetBlockIndexsByGamer(1)
                                })
                            });
                        }
                        break;

                    case CharacterCode.TEN_TROM:
                        {
                            var block = this.properties.blocksList[this.currentTurnGamer.currentBlockIndex];
                            var currentHouseCode = block.currentHouseCode;
                            this.ProcessDestroyHouseAtBlocks(new List<int>() { this.currentTurnGamer.currentBlockIndex }, false);
                            var opponentGamerIndex = this.properties.gamersPropertiesList.Count - 1 - this.properties.turnGamerIndex;
                            this.ProcessBuildHouseAtBlock(this.properties.turnGamerIndex, this.currentTurnGamer.currentBlockIndex, currentHouseCode);
                            block.tollRateBySkill = 1.5f;
                            if (this.currentTurnGamer.currentStarCard.characterCode == this.currentTurnGamer.currentCharacter)
                            {
                                block.tollRateBySkill += this.currentTurnGamer.currentStarCard.GetStatValue(StarCardStat.BonusSkillValue) / 100f;
                            }
                            var blockCfg = ConfigManager.instance.GetBlockConfig(block.index);
                            var houseCost = blockCfg.GetUpgradeHouseCost(HouseCode.NONE, currentHouseCode, 0);
                            this.currentTurnGamer.asset += houseCost;
                            this.ContinueTurnGamerAction();
                        }
                        break;

                    case CharacterCode.ELON_MUSK:
                        {
                            var opponentGamerIndex = this.properties.gamersPropertiesList.Count - 1 - this.properties.turnGamerIndex;
                            var opponentGamer = this.properties.gamersPropertiesList[opponentGamerIndex];
                            opponentGamer.numDicesByTurn = new GamerStatByTurn()
                            {
                                statValue = 1,
                                turn = 3
                            };
                            var shareCashPercent = 30f;
                            if (this.currentTurnGamer.currentStarCard.characterCode == this.currentTurnGamer.currentCharacter)
                            {
                                shareCashPercent += this.currentTurnGamer.currentStarCard.GetStatValue(StarCardStat.BonusSkillValue);
                            }
                            var shareCash = (int)(opponentGamer.cash * shareCashPercent / 100f);
                            this.ProcessSendCash(opponentGamerIndex, this.properties.turnGamerIndex, shareCash);
                            //this.properties.battleTime += 1f;
                            this.ContinueTurnGamerAction();
                        }
                        break;

                    case CharacterCode.DONAL_TRUMP:
                        {
                            foreach (var block in this.properties.blocksList)
                            {
                                if (block.ownerIndex == this.properties.turnGamerIndex)
                                {
                                    block.tollRatesByTurn.Add(new TollRateByTurnData()
                                    {
                                        rate = 2f,
                                        turn = 3
                                    });
                                }
                            }
                            this.ContinueTurnGamerAction();
                        }
                        break;

                    case CharacterCode.RONALDO:
                        {
                            var opponentGamerIndex = this.properties.gamersPropertiesList.Count - 1 - this.properties.turnGamerIndex;
                            var opponentGamer = this.properties.gamersPropertiesList[opponentGamerIndex];
                            int destBlockIndex = (opponentGamer.currentBlockIndex + 3) + this.properties.blocksList.Count;
                            this.AddReplayStep(ReplayStepType.FallCharacterToBlock, opponentGamerIndex, new FallToBlockReplayParameter()
                            {
                                fB = opponentGamer.currentBlockIndex,
                                dB = destBlockIndex,
                            }, 1.5f);
                            this.ProcessStartTurn();
                        }
                        break;

                    case CharacterCode.DR_STRANGE:
                        {
                            var opponentGamerIndex = this.properties.gamersPropertiesList.Count - 1 - this.properties.turnGamerIndex;
                            var opponentGamer = this.properties.gamersPropertiesList[opponentGamerIndex];
                            var blockIndexsList = this.properties.GetBlockIndexsByGamer(this.properties.turnGamerIndex);
                            int rand = RandomUtils.GetRandomIndexInList(blockIndexsList.Count);
                            var randBlockIndex = blockIndexsList[rand];
                            this.AddReplayStep(ReplayStepType.FallCharacterToBlock, opponentGamerIndex, new FallToBlockReplayParameter()
                            {
                                fB = opponentGamer.currentBlockIndex,
                                dB = randBlockIndex,
                            }, 1.5f);
                            this.ProcessStartTurn();
                        }
                        break;
                }
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
                this.properties.state = BattleState.END_BATTLE;
                switch (endBattleType)
                {
                    case EndBattleType.MONOPOLY:
                        {
                            this.AddReplayStep(ReplayStepType.ShowWarning, this.properties.turnGamerIndex, new ShowWarningReplayParameter()
                            {
                                wT = BattleWarningType.END_BATTLE_MONOPOLY,
                                bI = -1
                            });
                            var loseGamerIndex = this.properties.gamersPropertiesList.Count - 1 - winGamerIndex;
                            var loseGamer = this.properties.gamersPropertiesList[loseGamerIndex];
                            loseGamer.cash = 0;
                            loseGamer.asset = 0;
                        }
                        break;

                    case EndBattleType.BANKRUPT:
                        {
                            this.AddReplayStep(ReplayStepType.ShowWarning, this.properties.turnGamerIndex, new ShowWarningReplayParameter()
                            {
                                wT = BattleWarningType.END_BATTLE_BANKRUPT,
                                bI = -1
                            });
                            var loseGamerIndex = this.properties.gamersPropertiesList.Count - 1 - winGamerIndex;
                            var loseGamer = this.properties.gamersPropertiesList[loseGamerIndex];
                            loseGamer.cash = 0;
                            loseGamer.asset = 0;
                        }
                        break;

                    case EndBattleType.TURN_OFF:
                        {
                            this.AddReplayStep(ReplayStepType.ShowWarning, this.properties.turnGamerIndex, new ShowWarningReplayParameter()
                            {
                                wT = BattleWarningType.END_BATTLE_TURN_OFF,
                                bI = -1
                            });
                        }
                        break;
                }
                this.properties.ProcessSortGamersByAsset();
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

        /*protected async Task AddAFKAction(System.Action afkAction, float afkTime)
        {
            await Task.Delay((int)(afkTime * 1000) + 250);
            await Task.Run(afkAction);
        }*/

        protected int GetCashFromSellAllHousesOfGamer(int gamerIndex)
        {
            try
            {
                var sellAllHousesCash = 0;
                var sellGamer = this.properties.gamersPropertiesList[gamerIndex];
                var reduceSellHouseFeesPercent = sellGamer.currentStarCard.GetStatValue(StarCardStat.ReduceSellHouseFees);
                foreach (var block in this.properties.blocksList)
                {
                    if (block.ownerIndex != gamerIndex)
                        continue;
                    var blockCfg = ConfigManager.instance.GetBlockConfig(block.index);
                    //var block = this.properties.blocksList[blockIndex];
                    var houseCost = blockCfg.GetHouseCost(block.currentHouseCode);
                    var sellHousePrice = (int)((50f + reduceSellHouseFeesPercent) * houseCost / 100f);
                    sellAllHousesCash += sellHousePrice;
                }
                return sellAllHousesCash;
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        protected List<int> GetSellBlocksListEnoughtPayTolls(int gamerIndex, int missingTolls)
        {
            try
            {
                var sellBlockIndexsList = new List<int>();
                var sellGamer = this.properties.gamersPropertiesList[gamerIndex];
                var reduceSellHouseFeesPercent = sellGamer.currentStarCard.GetStatValue(StarCardStat.ReduceSellHouseFees);
                var totalSellHousesCash = 0;
                foreach (var block in this.properties.blocksList)
                {
                    if (block.ownerIndex != gamerIndex)
                        continue;
                    var blockCfg = ConfigManager.instance.GetBlockConfig(block.index);
                    //var block = this.properties.blocksList[blockIndex];
                    var houseCost = blockCfg.GetHouseCost(block.currentHouseCode);
                    var sellHousePrice = (int)((50f + reduceSellHouseFeesPercent) * houseCost / 100f);
                    totalSellHousesCash += sellHousePrice;
                    sellBlockIndexsList.Add(block.index);
                    if (totalSellHousesCash >= missingTolls)
                    {
                        break;
                    }
                }
                return sellBlockIndexsList;
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        protected bool CheckTurnGamerHasActionCard(ActionCardCode cardCode)
        {
            //return false;
            if (!this.currentTurnGamer.actionCardsList.ContainsKey(cardCode.ToString()))
            {
                return false;
            }
            return this.currentTurnGamer.actionCardsList[cardCode.ToString()];
        }

        protected bool CheckTurnGamerCanBuildHouseAtBlock(Block block, float discountHouseCostPercent, out string msg)
        {
            var blockIndex = block.index;
            var blockCfg = ConfigManager.instance.GetBlockConfig(blockIndex);
            msg = "";
            if (this.currentTurnGamer.cash < blockCfg.GetUpgradeHouseCost(block.currentHouseCode, block.currentHouseCode + 1, discountHouseCostPercent))
            {
                msg = "NotEnoughCashToBuildHouse";
                return false;
            }
            if ((int)block.currentHouseCode >= blockCfg.housesCost.Count - 1)
            {
                msg = "MaxHouse";
                return false;
            }
            return true;
        }

        protected bool CheckTurnGamerCanUseCharacterSkill()
        {
            try
            {
                if (this.currentTurnGamer.skillTurnCount > 0)
                {
                    return false;
                }
                switch (this.currentTurnGamer.currentCharacter)
                {
                    case CharacterCode.TEN_TROM:
                        {
                            var opponentGamerIndex = this.properties.gamersPropertiesList.Count - 1 - this.properties.turnGamerIndex;
                            var opponentGamerBlockIndexsList = this.properties.GetBlockIndexsByGamer(opponentGamerIndex);
                            return opponentGamerBlockIndexsList.Contains(this.currentTurnGamer.currentBlockIndex);
                        }

                    case CharacterCode.DOANH_NHAN:
                        {
                            return true;
                        }

                    case CharacterCode.PHI_CONG:
                        {
                            return this.properties.GetEmptyHouseBlocks().Count > 0;
                        }

                    case CharacterCode.CO_GAI:
                        {
                            var turnGamerBlockIndexsList = this.properties.GetBlockIndexsByGamer(this.properties.turnGamerIndex);
                            var opponentGamerIndex = this.properties.gamersPropertiesList.Count - 1 - this.properties.turnGamerIndex;
                            var opponentGamerBlockIndexsList = this.properties.GetBlockIndexsByGamer(opponentGamerIndex);
                            return turnGamerBlockIndexsList.Count > 0 && opponentGamerBlockIndexsList.Count > 0;
                        }

                    case CharacterCode.ELON_MUSK:
                        {
                            var opponentGamerIndex = this.properties.gamersPropertiesList.Count - 1 - this.properties.turnGamerIndex;
                            var opponentGamer = this.properties.gamersPropertiesList[opponentGamerIndex];
                            return this.currentTurnGamer.currentBlockIndex == opponentGamer.currentBlockIndex;
                        }

                    case CharacterCode.DONAL_TRUMP:
                        {
                            var turnGamerBlockIndexsList = this.properties.GetBlockIndexsByGamer(this.properties.turnGamerIndex);
                            return turnGamerBlockIndexsList.Count > 0;
                        }

                    case CharacterCode.RONALDO:
                        {
                            var opponentGamerIndex = this.properties.gamersPropertiesList.Count - 1 - this.properties.turnGamerIndex;
                            var opponentGamer = this.properties.gamersPropertiesList[opponentGamerIndex];
                            if (this.currentTurnGamer.currentBlockIndex == 23 && opponentGamer.currentBlockIndex == 0)
                            {
                                return true;
                            }
                            return this.currentTurnGamer.currentBlockIndex == opponentGamer.currentBlockIndex || this.currentTurnGamer.currentBlockIndex == opponentGamer.currentBlockIndex - 1;
                        }

                    case CharacterCode.DR_STRANGE:
                        {
                            var opponentGamerIndex = this.properties.gamersPropertiesList.Count - 1 - this.properties.turnGamerIndex;
                            var opponentGamer = this.properties.gamersPropertiesList[opponentGamerIndex];
                            if (this.currentTurnGamer.currentBlockIndex == 23 && opponentGamer.currentBlockIndex == 0)
                            {
                                return true;
                            }
                            return this.currentTurnGamer.currentBlockIndex == opponentGamer.currentBlockIndex || this.currentTurnGamer.currentBlockIndex == opponentGamer.currentBlockIndex - 1;
                        }
                }
                return false;
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        protected void SendWaitingGamerAction(BattleGamerActionData _gamerActionData)
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
                    this.waitingGamerAction = _gamerActionData;
                }

                var replayStepsList = new List<ReplayStepData>();
                for (int i = this.lastReplayStepsCount; i < this.replayData.stepsList.Count; i++)
                {
                    replayStepsList.Add(this.replayData.stepsList[i]);
                }
                _gamerActionData.delayTime = this.properties.battleTime - this.lastBattleTime;
                this.hubContext.Clients.Group(this.roomKey).SendAsync("WaitingGamerAction", _gamerActionData, this.properties, replayStepsList);

                BattleMongoDB.Save(this.properties);
                BattleReplayMongoDB.Save(this.replayData);
                //this.SendReplayDatas(this.lastReplayStepsCount);
                //this.hubContext.Clients.Group(this.roomKey).SendAsync("UpdateTurnGamerWaitingTime", this.properties.turnGamerIndex, waitingTime, _waitingAction);
                //var turnClientProxy = this.hubContext.Clients.Client(this.hubConnectionIDsList[this.currentTurnGamer.gid]);
                //var methodName = $"Waiting{_gamerActionData.actionType.ToString()}";
                /*switch (_waitingAction)
                {
                    case BattleGamerAction.RollDice:
                        {
                            this.hubContext.Clients.Client(this.hubConnectionIDsList[this.currentTurnGamer.gid]).SendAsync(methodName, false);
                            //if (this.needProcessAFK)
                            {
                                //await Task.Delay((int)(waitingTime * 1000) + 250);
                                //await this.OnGamerRollDice(this.properties.turnGamerIndex, false, true);
                            }
                        }
                        break;

                    case BattleGamerAction.UseActionCard:
                        {
                            this.currentTurnGamer.waitingActionCardCode = (ActionCardCode)_params[0];
                            this.hubContext.Clients.Client(this.hubConnectionIDsList[this.currentTurnGamer.gid]).SendAsync(methodName, this.currentTurnGamer.waitingActionCardCode);
                            //if (this.needProcessAFK)
                            {
                                //await Task.Delay((int)(waitingTime * 1000) + 250);
                                //await this.OnGamerRollDice(this.properties.turnGamerIndex, false, true);
                            }
                        }
                        break;

                    case BattleGamerAction.BuildHouse:
                        {
                            var block = this.properties.blocksList[this.currentTurnGamer.currentBlockIndex];
                            //this.hubContext.Clients.Client(this.hubConnectionIDsList[this.currentTurnGamer.gid]).SendAsync(methodName, this.properties.turnGamerIndex, this.currentTurnGamer.currentBlockIndex, block.currentHouseCode, this.currentTurnGamer.GetDiscountHouseCostPercent());
                            //if (this.needProcessAFK)
                            {
                                //await Task.Delay((int)(waitingTime * 1000) + 250);
                                //await this.OnGamerBuildHouse(this.properties.turnGamerIndex, this.currentTurnGamer.currentBlockIndex, block.currentHouseCode + 1, true);
                            }
                        }
                        break;

                    case BattleGamerAction.UseCharacterSkill:
                        {
                            this.hubContext.Clients.Client(this.hubConnectionIDsList[this.currentTurnGamer.gid]).SendAsync(methodName, this.currentTurnGamer.currentCharacter);
                            //if (this.needProcessAFK)
                            {
                                //await Task.Delay((int)(waitingTime * 1000) + 250);
                                //await this.OnGamerUseCharacterSkill(this.properties.turnGamerIndex, false, true);
                            }
                        }
                        break;

                    case BattleGamerAction.SelectBlock:
                        {
                            var blockIndexsList = (List<int>)_params[0];
                            var selectActionCode = (SelectBlockActionCode)_params[1];
                            this.hubContext.Clients.Client(this.hubConnectionIDsList[this.currentTurnGamer.gid]).SendAsync(methodName, blockIndexsList, selectActionCode);
                            //if (this.needProcessAFK)
                            {
                                //await Task.Delay((int)(waitingTime * 1000) + 250);
                                //await this.OnGamerSelectBlock(this.properties.turnGamerIndex, blockIndexsList[0], selectActionCode, true);
                            }
                        }
                        break;


                    case BattleGamerAction.SellHouses:
                        {
                            var missingTolls = (int)_params[0];
                            var turnGamerBlockIndexsList = this.properties.GetBlockIndexsByGamer(this.properties.turnGamerIndex);
                            var defaultBlockIndexsList = this.GetSellBlocksListEnoughtPayTolls(this.properties.turnGamerIndex, missingTolls);
                            var reduceSellHouseFeesPercent = this.currentTurnGamer.currentStarCard.GetStatValue(StarCardStat.ReduceSellHouseFees);
                            this.hubContext.Clients.Client(this.hubConnectionIDsList[this.currentTurnGamer.gid]).SendAsync(methodName, missingTolls, turnGamerBlockIndexsList, reduceSellHouseFeesPercent, defaultBlockIndexsList);
                            //if (this.needProcessAFK)
                            {
                                //await Task.Delay((int)(waitingTime * 1000) + 250);
                                //await this.OnGamerSellHouses(this.properties.turnGamerIndex, defaultBlockIndexsList, true);
                            }
                        }
                        break;

                    case BattleGamerAction.ExchangeBlocks:
                        {
                            var gamer0_BlockIndexsList = this.properties.GetBlockIndexsByGamer(0);
                            var gamer1_BlockIndexsList = this.properties.GetBlockIndexsByGamer(1);
                            this.hubContext.Clients.Client(this.hubConnectionIDsList[this.currentTurnGamer.gid]).SendAsync(methodName, gamer0_BlockIndexsList, gamer1_BlockIndexsList);
                            //if (this.needProcessAFK)
                            {
                                //await Task.Delay((int)(waitingTime * 1000) + 250);
                                //await this.OnGamerExchangeBlocks(this.properties.turnGamerIndex, new List<int>() { gamer0_BlockIndexsList[0], gamer1_BlockIndexsList[0] }, true);
                            }
                        }
                        break;
                }*/
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        /*protected void SendReplayDatas(int _fromStepIndex)
        {
            try
            {
                var replayStepsList = new List<ReplayStepData>();
                for (int i = _fromStepIndex; i < this.replayData.stepsList.Count; i++)
                {
                    replayStepsList.Add(this.replayData.stepsList[i]);
                }
                this.hubContext.Clients.Group(this.roomKey).SendAsync("UpdateReplayDatas", this.properties, replayStepsList);
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }*/

        protected void SendDisplayMessageToAllGamers(string msg)
        {
            this.hubContext.Clients.Group(this.roomKey).SendAsync("ShowDisplayMessage", msg, true);
        }

        /*protected void SyncBattlePropertiesToAllGamers()
        {
            this.hubContext.Clients.Group(this.roomKey).SendAsync("UpdateBattleProperties", this.properties);
        }*/

        protected void AddReplayStep(ReplayStepType _type, int _gamerIndex, object _param, float _stepTime = 0.25f)
        {
            var stepData = new ReplayStepData()
            {
                ID = this.replayData.stepsList.Count,
                sT = _type,
                g = _gamerIndex,
                aT = this.properties.battleTime,
                jV = JsonMapper.ToJson(_param)
            };
            this.replayData.stepsList.Add(stepData);
            this.properties.battleTime += _stepTime;
        }

        #region Listen action from gamers.
        public async Task OnGamerJoinRoom(BattleHub hub, long gid)
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
                            currentCharacter = userInfo.gamerData.currentCharacter,
                            currentDice = userInfo.gamerData.currentDice,
                            indexInBattle = this.properties.gamersPropertiesList.Count,
                            currentStarCard = userInfo.GetCurrentStarCard(),
                            cash = this.roomConfig.startCash,
                            betCash = this.roomConfig.startCash,
                            asset = this.roomConfig.startCash
                        };
                        this.properties.gamersPropertiesList.Add(gamerProperties);
                    }
                    await hub.Clients.GroupExcept(this.roomKey, hub.Context.ConnectionId).SendAsync("OnOtherGamerJoinRoomSuccess", this.properties.gamersPropertiesList);
                }
                else
                {

                }
                this.hubConnectionIDsList[gid] = hub.Context.ConnectionId;
                await this.hubContext.Groups.AddToGroupAsync(hub.Context.ConnectionId, this.roomKey);
                await hub.Clients.Caller.SendAsync("OnJoinRoomSuccess", this.properties);

                if (this.properties.state == BattleState.MATCHING)
                {
                    RoomController.ParseRoomTypeLevelFromID(this.properties.ID, out var roomType, out var roomLevel);
                    if (this.properties.gamersPropertiesList.Count == (int)roomType)
                    {
                        //this.ProcessStartBattle();
                        this.ProcessState(BattleState.BUY_ACTION_CARD);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        public async Task OnGamerCancelJoinRoom(long gid)
        {
            try
            {
                if (this.properties.state != BattleState.MATCHING)
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

        public async Task OnGamerBuyActionCard(int gamerIndex, int cardIndex)
        {
            try
            {
                if (this.properties.state != BattleState.BUY_ACTION_CARD)
                {
                    return;
                }
                var gamerProperties = this.properties.gamersPropertiesList[gamerIndex];
                var randActionCard = ActionCardCode.None;
                if (cardIndex >= gamerProperties.actionCardsList.Count)
                {
                    var allActionCardsList = new List<ActionCardCode>();
                    foreach (ActionCardCode cardCode in (ActionCardCode[])Enum.GetValues(typeof(ActionCardCode)))
                    {
                        if (cardCode == ActionCardCode.None) continue;
                        if (gamerProperties.actionCardsList.ContainsKey(cardCode.ToString())) continue;
                        allActionCardsList.Add(cardCode);
                    }
                    var randIndex = RandomUtils.GetRandomInt(0, allActionCardsList.Count);
                    randActionCard = allActionCardsList[randIndex];
                    gamerProperties.actionCardsList.Add(randActionCard.ToString(), true);
                }
                await this.hubContext.Clients.Client(this.hubConnectionIDsList[gamerProperties.gid]).SendAsync("BuyActionCardResponse", cardIndex, randActionCard);
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        public Task OnGamerRollDice(int gamerIndex, bool isSpecialRoll, int _testValue, ChanceCardCode _testChanceCard, bool isAFK)
        {
            try
            {
                if (!this.CheckValidWaitingGamerAction(BattleGamerAction.RollDice, gamerIndex))
                {
                    return Task.CompletedTask;
                }
                this.lastReplayStepsCount = this.replayData.stepsList.Count;
                this.lastBattleTime = this.properties.battleTime;
                this.currentTurnGamer.isRollingDoubleDices = false;
                var diceValues = DiceController.getValues(this.currentTurnGamer.numDices, this.currentTurnGamer.currentDice, isSpecialRoll, _testValue);
                this.currentTurnGamer.isRollingDoubleDices = diceValues[0] == diceValues[1];
                this.currentTurnGamer.testChanceCard = _testChanceCard;
                var dicesTotalValue = 0;
                for (int i = 0; i < diceValues.Count; i++)
                {
                    dicesTotalValue += diceValues[i];
                }
                if (this.currentTurnGamer.numDices == 1)
                {
                    this.currentTurnGamer.UseMana(ManaCode.Roll1Dice);
                }
                else
                {
                    this.currentTurnGamer.UseMana(ManaCode.Roll2Dices);
                }
                var destBlockIndex = (this.currentTurnGamer.currentBlockIndex + dicesTotalValue) % this.properties.blocksList.Count;
                this.AddReplayStep(ReplayStepType.RollDice, this.properties.turnGamerIndex, new RollDiceReplayParameter()
                {
                    d1 = diceValues[0],
                    d2 = diceValues[1],
                    dB = destBlockIndex
                }, 3f);
                if (this.currentTurnGamer.isRollingDoubleDices)
                {
                    //this.ProcessMission(MissionCode.DoubleDices, this.currentTurnGamer);
                }
                //this.properties.battleTime += 3f;
                this.ProcessMoveCharacterToBlock(destBlockIndex, CharacterCode.NONE);
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
            return Task.CompletedTask;
        }

        public async Task OnGamerBuildHouse(int gamerIndex, int blockIndex, HouseCode selectedHouseCode, bool isAFK)
        {
            try
            {
                if (!this.CheckValidWaitingGamerAction(BattleGamerAction.BuildHouse, gamerIndex))
                {
                    return;
                }
                if (blockIndex != this.currentTurnGamer.currentBlockIndex)
                    return;
                var block = this.properties.blocksList[blockIndex];
                if (block.ownerIndex >= 0 && block.ownerIndex != this.properties.turnGamerIndex)
                    return;

                this.lastReplayStepsCount = this.replayData.stepsList.Count;
                this.lastBattleTime = this.properties.battleTime;
                if (selectedHouseCode > HouseCode.NONE)
                {
                    if (block.currentHouseCode != HouseCode.NONE && block.currentHouseCode >= selectedHouseCode)
                        return;
                    var blockCfg = ConfigManager.instance.GetBlockConfig(block.index);
                    var upgradeHouseCost = blockCfg.GetUpgradeHouseCost(block.currentHouseCode, selectedHouseCode, this.currentTurnGamer.GetDiscountHouseCostPercent());
                    if (this.currentTurnGamer.cash < upgradeHouseCost)
                    {
                        return;
                    }
                    this.ProcessBuildHouseAtBlock(this.properties.turnGamerIndex, blockIndex, selectedHouseCode);
                    this.currentTurnGamer.cash -= upgradeHouseCost;
                    this.AddReplayStep(ReplayStepType.ChangeCash, this.properties.turnGamerIndex, new ChangeCashReplayParameter()
                    {
                        aN = CharacterAnim.HAPPY,
                        cV = -upgradeHouseCost,
                        bI = -1,
                        cA = this.currentTurnGamer.asset,
                        cC = this.currentTurnGamer.cash
                    }, 1f);
                }
                this.currentTurnGamer.discountHouseCostByCharacterSkill = 0;
                this.currentTurnGamer.discountHouseCostByActionCard = 0;
                this.ContinueTurnGamerAction(block);
                //this.SendReplayDatas(_currentReplayStepsCount);
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        public async Task OnGamerUseCharacterSkill(int gamerIndex, bool use, bool isAFK)
        {
            try
            {
                if (!this.CheckValidWaitingGamerAction(BattleGamerAction.UseCharacterSkill, gamerIndex))
                {
                    return;
                }
                this.lastReplayStepsCount = this.replayData.stepsList.Count;
                this.lastBattleTime = this.properties.battleTime;
                if (use)
                {
                    this.ProcessUseCharacterSkill();
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
        }

        public async Task OnGamerUseActionCard(int gamerIndex, ActionCardCode cardCode, bool use, bool isAFK)
        {
            try
            {
                if (!this.CheckValidWaitingGamerAction(BattleGamerAction.UseActionCard, gamerIndex))
                {
                    return;
                }
                if (use)
                {
                    this.currentTurnGamer.actionCardsList[cardCode.ToString()] = false;
                }
                this.lastReplayStepsCount = this.replayData.stepsList.Count;
                this.lastBattleTime = this.properties.battleTime;
                switch (cardCode)
                {
                    case ActionCardCode.DiscountHouseCost:
                        {
                            if (use)
                            {
                                this.currentTurnGamer.discountHouseCostByActionCard = 50;

                            }
                            else
                            {
                                this.currentTurnGamer.discountHouseCostByActionCard = 0;
                            }
                            var block = this.properties.blocksList[this.currentTurnGamer.currentBlockIndex];
                            this.SendWaitingGamerAction(new BattleGamerActionData()
                            {
                                actionType = BattleGamerAction.BuildHouse,
                                indexInBattle = this.properties.turnGamerIndex,
                                jsonValue = JsonMapper.ToJson(new BuildHouseActionParameter()
                                {
                                    blockIndex = this.currentTurnGamer.currentBlockIndex,
                                    currentHouse = block.currentHouseCode,
                                    discountHouseCostPercent = this.currentTurnGamer.GetDiscountHouseCostPercent()
                                })
                            });
                        }
                        break;

                    case ActionCardCode.FreeTaxes:
                        this.ProcessWhenStayAtOpponentBlock(use);
                        break;

                    case ActionCardCode.StarCity:
                        {
                            if (use)
                            {
                                var block = this.properties.blocksList[this.currentTurnGamer.currentBlockIndex];
                                block.isStarCity = true;
                                this.properties.ProcessSortGamersByAsset();
                                //this.SyncBattlePropertiesToAllGamers();
                                //this.properties.battleTime += 1f;
                            }
                            //await this.ContinueTurnGamerAction();
                            this.ProcessWhenStayAtMineBlock(use ? 25f : 0f);
                        }
                        break;

                    case ActionCardCode.BonusSalaryWhenAtGO:
                        if (use)
                        {
                            var bonusSalary = this.roomConfig.salary * 30 / 100;
                            this.currentTurnGamer.AddCash(bonusSalary);
                            this.AddReplayStep(ReplayStepType.ChangeCash, this.properties.turnGamerIndex, new ChangeCashReplayParameter()
                            {
                                aN = CharacterAnim.HAPPY,
                                cV = bonusSalary,
                                bI = -1,
                                cA = this.currentTurnGamer.asset,
                                cC = this.currentTurnGamer.cash
                            }, 1f);
                        }
                        this.ContinueTurnGamerAction();
                        break;
                }
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        public async Task OnGamerSelectBlock(int gamerIndex, int selectedBlockIndex, SelectBlockActionCode selectActionCode, bool isAFK)
        {
            try
            {
                if (!this.CheckValidWaitingGamerAction(BattleGamerAction.SelectBlock, gamerIndex))
                {
                    return;
                }
                this.lastReplayStepsCount = this.replayData.stepsList.Count;
                this.lastBattleTime = this.properties.battleTime;
                switch (selectActionCode)
                {
                    case SelectBlockActionCode.SET_CASINO:
                        {
                            var currentCasinoBlock = this.properties.blocksList.Find(e => e.isPark);
                            if (currentCasinoBlock != null)
                            {
                                currentCasinoBlock.isPark = false;
                            }
                            var block = this.properties.blocksList[selectedBlockIndex];
                            block.isPark = true;
                            this.properties.ProcessSortGamersByAsset();
                            //this.SyncBattlePropertiesToAllGamers();
                            //this.properties.battleTime += 1f;
                            this.ContinueTurnGamerAction();
                        }
                        break;

                    case SelectBlockActionCode.SET_OLYMPIC:
                        {
                            var currentOlympicBlock = this.properties.blocksList.Find(e => e.isOlympic);
                            if (currentOlympicBlock != null)
                            {
                                currentOlympicBlock.isOlympic = false;
                            }
                            var block = this.properties.blocksList[selectedBlockIndex];
                            block.isOlympic = true;
                            this.properties.ProcessSortGamersByAsset();
                            //this.SyncBattlePropertiesToAllGamers();
                            //this.properties.battleTime += 1f;
                            this.ContinueTurnGamerAction();
                        }
                        break;

                    case SelectBlockActionCode.SET_FESTIVAL:
                        {
                            var currentFestivalBlock = this.properties.blocksList.Find(e => e.isFestival);
                            if (currentFestivalBlock != null)
                            {
                                currentFestivalBlock.isFestival = false;
                            }
                            var block = this.properties.blocksList[selectedBlockIndex];
                            block.isFestival = true;
                            this.properties.ProcessSortGamersByAsset();
                            //this.SyncBattlePropertiesToAllGamers();
                            //this.properties.battleTime += 1f;
                            this.ContinueTurnGamerAction();
                        }
                        break;

                    case SelectBlockActionCode.SET_STAR_CITY:
                        {
                            var currentStarCityBlock = this.properties.blocksList.Find(e => e.isStarCity);
                            if (currentStarCityBlock != null)
                            {
                                currentStarCityBlock.isStarCity = false;
                            }
                            var block = this.properties.blocksList[selectedBlockIndex];
                            block.isStarCity = true;
                            this.properties.ProcessSortGamersByAsset();
                            //this.SyncBattlePropertiesToAllGamers();
                            //this.properties.battleTime += 1f;
                            this.ContinueTurnGamerAction();
                        }
                        break;

                    case SelectBlockActionCode.SET_DONATE:
                        {
                            var block = this.properties.blocksList[selectedBlockIndex];
                            var currentHouseCode = block.currentHouseCode;
                            this.ProcessDestroyHouseAtBlocks(new List<int>() { selectedBlockIndex }, false);
                            var opponentGamerIndex = this.properties.gamersPropertiesList.Count - 1 - this.properties.turnGamerIndex;
                            this.ProcessBuildHouseAtBlock(opponentGamerIndex, selectedBlockIndex, currentHouseCode);
                            var blockCfg = ConfigManager.instance.GetBlockConfig(block.index);
                            var houseCost = blockCfg.GetUpgradeHouseCost(HouseCode.NONE, currentHouseCode, 0);
                            var opponentGamer = this.properties.gamersPropertiesList[opponentGamerIndex];
                            opponentGamer.asset += houseCost;
                            this.properties.ProcessSortGamersByAsset();
                            //this.SyncBattlePropertiesToAllGamers();
                            //this.properties.battleTime += 1f;
                            this.ContinueTurnGamerAction();
                        }
                        break;

                    case SelectBlockActionCode.DOWNGRADE_OPPONENT_HOUSE:
                        {
                            var opponentGamerIndex = this.properties.gamersPropertiesList.Count - 1 - this.properties.turnGamerIndex;
                            var block = this.properties.blocksList[selectedBlockIndex];
                            if (block.ownerIndex == opponentGamerIndex)
                            {
                                var blockCfg = ConfigManager.instance.GetBlockConfig(selectedBlockIndex);
                                block.currentHouseCode--;
                                var opponentGamer = this.properties.gamersPropertiesList[opponentGamerIndex];
                                var downgradeCost = blockCfg.GetUpgradeHouseCost(block.currentHouseCode, block.currentHouseCode + 1, opponentGamer.GetDiscountHouseCostPercent());
                                opponentGamer.asset -= downgradeCost;
                            }
                            this.properties.ProcessSortGamersByAsset();
                            //this.SyncBattlePropertiesToAllGamers();
                            //this.properties.battleTime += 1f;
                            this.ContinueTurnGamerAction();
                        }
                        break;

                    case SelectBlockActionCode.REDUCE_OPPONENT_BLOCK_TOLL:
                        {
                            var opponentGamerIndex = this.properties.gamersPropertiesList.Count - 1 - this.properties.turnGamerIndex;
                            var block = this.properties.blocksList[selectedBlockIndex];
                            if (block.ownerIndex == opponentGamerIndex)
                            {
                                block.tollRatesByTurn.Add(new TollRateByTurnData()
                                {
                                    rate = 0f,
                                    turn = 3
                                });
                            }
                            this.properties.ProcessSortGamersByAsset();
                            //this.SyncBattlePropertiesToAllGamers();
                            //this.properties.battleTime += 1f;
                            this.ContinueTurnGamerAction();
                        }
                        break;

                    case SelectBlockActionCode.SELL_OPPONENT_HOUSE:
                        {
                            var opponentGamerIndex = this.properties.gamersPropertiesList.Count - 1 - this.properties.turnGamerIndex;
                            var ownerGamer = this.properties.gamersPropertiesList[opponentGamerIndex];
                            var block = this.properties.blocksList[selectedBlockIndex];
                            if (block.ownerIndex == opponentGamerIndex)
                            {
                                var opponentGamer = this.properties.gamersPropertiesList[opponentGamerIndex];
                                this.ProcessDestroyHouseAtBlocks(new List<int>() { selectedBlockIndex }, true);
                                //this.properties.battleTime += 0.5f;
                            }
                            this.properties.ProcessSortGamersByAsset();
                            //this.SyncBattlePropertiesToAllGamers();
                            //this.properties.battleTime += 1f;
                            this.ContinueTurnGamerAction();
                        }
                        break;

                    case SelectBlockActionCode.SET_BLOCK_FOR_CHARACTER_SKILL:
                        {
                            this.ProcessSetBlockForCharacterSkill(selectedBlockIndex);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        public async Task OnGamerSellHouses(int gamerIndex, List<int> selectedBlockIndexsList, bool isAFK)
        {
            try
            {
                this.ProcessDestroyHouseAtBlocks(selectedBlockIndexsList, true);
                /*this.currentTurnFramesRelay.Add(new FrameReplayData()
                {
                    E = ReplayEventCode.ChangeCash,
                    T = this.currentTurnFrameTime,
                    G = this.properties.turnGamerIndex,
                    A = CharacterAnim.SAD,
                    V = JsonMapper.ToJson(new ChangeCashReplayData()
                    {
                        changedCash = totalSellCashes,
                        currentCash = this.currentTurnGamer.cash,
                        currentAsset = this.currentTurnGamer.asset
                    })
                });*/
                this.lastReplayStepsCount = this.replayData.stepsList.Count;
                this.lastBattleTime = this.properties.battleTime;
                if (this.currentTurnGamer.currentTollsNeedPay > 0)
                {
                    var opponentGamerIndex = this.properties.gamersPropertiesList.Count - 1 - this.properties.turnGamerIndex;
                    this.ProcessSendCash(this.properties.turnGamerIndex, opponentGamerIndex, this.currentTurnGamer.currentTollsNeedPay);
                }
                this.properties.ProcessSortGamersByAsset();
                //this.SyncBattlePropertiesToAllGamers();
                //this.properties.battleTime += 1f;
                this.ContinueTurnGamerAction();
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        public async Task OnGamerExchangeBlocks(int gamerIndex, List<int> selectedBlockIndexsList, bool isAFK)
        {
            try
            {
                this.lastReplayStepsCount = this.replayData.stepsList.Count;
                this.lastBattleTime = this.properties.battleTime;
                var blockIndexsByGamer = new Dictionary<int, int>();
                var houseByGamers = new Dictionary<int, HouseCode>();
                var checkBlocksList = new List<Block>();
                for (int i = 0; i < selectedBlockIndexsList.Count; i++)
                {
                    var _selectedBlockIndex = selectedBlockIndexsList[i];
                    var _selectedBlock = this.properties.blocksList[_selectedBlockIndex];
                    var _gamerIndex = this.properties.gamersPropertiesList.Count - 1 - i;
                    blockIndexsByGamer[_gamerIndex] = _selectedBlockIndex;
                    houseByGamers[_gamerIndex] = _selectedBlock.currentHouseCode;
                    this.ProcessDestroyHouseAtBlocks(new List<int>() { _selectedBlockIndex }, true);
                    checkBlocksList.Add(_selectedBlock);
                }
                foreach (var _gamerIndex in houseByGamers.Keys)
                {
                    var _blockIndex = blockIndexsByGamer[_gamerIndex];
                    var _houseCode = houseByGamers[_gamerIndex];
                    this.ProcessBuildHouseAtBlock(_gamerIndex, _blockIndex, _houseCode);
                }
                //this.properties.battleTime += 1f;
                this.properties.ProcessSortGamersByAsset();
                //this.SyncBattlePropertiesToAllGamers();
                //this.properties.battleTime += 1f;
                this.ContinueTurnGamerAction(checkBlocksList[0], checkBlocksList[1]);
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                this.SendDisplayMessageToAllGamers(ex.ToString());
            }
        }

        public async Task OnGamerRematch(int gamerIndex)
        {
            try
            {
                if (this.properties.state != BattleState.END_BATTLE)
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
                    this.ProcessState(BattleState.BUY_ACTION_CARD);
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
    }
}