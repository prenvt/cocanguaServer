using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CBShare.Common;
using CBShare.Data;
using CBShare.Configuration;
using LitJson;

namespace CBShare.Data
{
    public enum ReplayStepType
    {
        NONE = 0,
        RollDice = 1,
        MoveCharacterToBlock = 2,
        ChangeCash = 3,
        SetHouseAtBlock = 4,
        ShowMessageOnChracterHead = 5,
        ShowWarning = 6,
        DrawChanceCard = 7,
        FallCharacterToBlock = 8,
        CannonShotToBlock = 9,
        SetBlockForCharacterSkill = 10
    }

    public class ReplayStepData
    {
        public int ID;
        public int g;
        public ReplayStepType sT;
        public float aT;
        public string jV;
    }

    public class BattleReplayData
    {
        public long battleID;
        public List<ReplayStepData> stepsList = new List<ReplayStepData>();
        public DateTime lastUpdate;
    }

    public class RollDiceReplayParameter
    {
        public int d1, d2;
        public int dB;
    }

    public class MoveCharacterReplayParameter
    {
        public int fB;
        public int dB;
        public CharacterCode sC;
    }

    public class FallToBlockReplayParameter
    {
        public int fB;
        public int dB;
    }

    public class ChangeCashReplayParameter
    {
        public string aN;
        public int cV;
        public int bI;
        public int cA;
        public int cC;
    }

    public class ShowMessageOnHeadReplayParameter
    {
        public string m;
    }

    public class CannonShotToBlockReplayParameter
    {
        public int bI;
    }

    public class SetBlockForCharacterSkillReplayParameter
    {
        public int bI;
    }

    public class ShowWarningReplayParameter
    {
        public BattleWarningType wT;
        public int bI;
    }
}


