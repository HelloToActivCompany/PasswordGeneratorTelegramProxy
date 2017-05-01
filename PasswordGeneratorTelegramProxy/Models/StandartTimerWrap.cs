using System;
using System.Timers;

namespace PasswordGeneratorTelegramProxy.Models
{
    public class StandartTimerWrap : IElapsed
    {
        public event EventHandler Elapsed;

        private Timer _timer;

        public StandartTimerWrap(int interval)
        {
            _timer = new Timer(interval);
            _timer.Elapsed += Timer_Elapsed;
            _timer.AutoReset = true;
            _timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Elapsed(this, EventArgs.Empty);
        }
    }
}