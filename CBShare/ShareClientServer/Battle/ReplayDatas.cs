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
        StartHorse = 2,
        MoveHorse = 3,
        KickHorse = 4,
        MoveStarHorse = 6,
        UpdatePoint = 7,
        UseBoosterItem = 8,
    }

    public class ReplayStepData
    {
        public int ID;
        public GamerColor g;
        public ReplayStepType ty;
        public float t;
        public object v1;
        public object v2;
        public object v3;
    }

    public class BattleReplayData
    {
        public long battleID;
        public List<ReplayStepData> stepsList = new List<ReplayStepData>();
        public DateTime lastUpdate;
    }

    /*public class RollDiceReplayParameter
    {
        public int dV;
    }

    public class StartHorseReplayParameter
    {
        public int sS;
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
    }*/
}


