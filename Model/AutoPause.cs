using System;
using System.Threading;
using Newtonsoft.Json;
using _4RTools.Utils;

namespace _4RTools.Model
{
    public class AutoPause : Action
    {
        public event EventHandler<AntiBotEventArgs> AntiBot;
        public static string ACTION_NAME_AUTOPAUSE = "AutoPause";
        private bool _prevAntiBot = false;

        private _4RThread thread;

        public void Start()
        {
            Stop();
            Client roClient = ClientSingleton.GetClient();
            if (roClient != null)
            {
                if (this.thread != null)
                {
                    _4RThread.Stop(this.thread);
                }
                this.thread = AutoPauseThread(roClient);
                _4RThread.Start(this.thread);
            }
        }

        public _4RThread AutoPauseThread(Client c)
        {
            _4RThread AutoPauseThread = new _4RThread(_ =>
            {
                bool AntiBotFound = false;
                for (int i = 1; i < Constants.MAX_BUFF_LIST_INDEX_SIZE; i++)
                {
                    uint currentStatus = c.CurrentBuffStatusCode(i);

                    if (currentStatus == uint.MaxValue) { continue; }

                    EffectStatusIDs status = (EffectStatusIDs)currentStatus;

                    if (EffectStatusIDs.ANTI_BOT.Equals(status))
                    {
                        AntiBotFound = true;
                        break;
                    }
                }

                if (AntiBotFound != _prevAntiBot)
                {
                    _prevAntiBot = AntiBotFound;
                    AntiBot(this, new AntiBotEventArgs(AntiBotFound));
                }

                Thread.Sleep(100);
                return 0;

            });

            return AutoPauseThread;
        }

        public void Stop()
        {
            _4RThread.Stop(this.thread);
            _prevAntiBot = false;
        }

        public string GetConfiguration()
        {
            return JsonConvert.SerializeObject(this);
        }

        public string GetActionName()
        {
            return ACTION_NAME_AUTOPAUSE;
        }

        public class AntiBotEventArgs : EventArgs
        {
            public AntiBotEventArgs(bool status)
            {
                Status = status;
            }

            public bool Status { get; }
        }
    }
}
