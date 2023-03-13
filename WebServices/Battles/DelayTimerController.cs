using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebServices.Battles
{
    public class DelayTimerController
    {
        protected readonly System.Timers.Timer delayTimer = new System.Timers.Timer();
        public System.Action delayAction;

        public void AddAction(System.Action _delayAction, float _delayTime)
        {
            this.delayAction = _delayAction;
            this.delayTimer.Interval = _delayTime * 1000;
            this.delayTimer.Elapsed += this.ProcessDelayAction;
            this.delayTimer.Start();
        }

        private void ProcessDelayAction(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.delayAction != null)
            {
                this.delayAction.Invoke();
            }
            this.delayAction = null;
        }
    }
}
