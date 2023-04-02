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
        MoveHorse = 2,
        KickHorse = 3,
        StarHorse = 4,
        UpdatePoint = 5,
        ShowWarning = 6,
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
        public int dV;
    }

    public class MoveHorseReplayParameter
    {
        public int fS;
        public int dS;
    }

    public class UpdatePointReplayParameter
    {
        public int p;
    }

    public class ShowWarningReplayParameter
    {
        public BattleWarningType wT;
        public int bI;
    }
}


