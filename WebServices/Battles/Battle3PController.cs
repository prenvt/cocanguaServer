using CBShare.Battle;
using CBShare.Data;
using CTPServer.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebServices.Battles
{
    public class Battle3PController : BattleBaseController
    {
        public override Task OnGamerRollDice(int gamerIndex, bool isSpecialRoll, int _testValue, bool isAFK)
        {
            try
            {
                if (!this.CheckValidWaitingGamerAction(BattleGamerAction.RollDice, gamerIndex))
                {
                    return Task.CompletedTask;
                }
                this.lastReplayStepsCount = this.replayData.stepsList.Count;
                this.lastBattleTime = this.properties.battleTime;
                var diceValues = DiceController.getValues(1, this.currentTurnGamer.currentDice, isSpecialRoll, _testValue);
                var dicesTotalValue = 0;

                /*var destBlockIndex = (this.currentTurnGamer.currentBlockIndex + dicesTotalValue) % this.properties.blocksList.Count;
                this.AddReplayStep(ReplayStepType.RollDice, this.properties.turnGamerIndex, new RollDiceReplayParameter()
                {
                    d1 = diceValues[0],
                    d2 = diceValues[1],
                    dB = destBlockIndex
                }, 3f);*/
                //this.properties.battleTime += 3f;
                //this.ProcessMoveCharacterToBlock(destBlockIndex, CharacterCode.NONE);
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
