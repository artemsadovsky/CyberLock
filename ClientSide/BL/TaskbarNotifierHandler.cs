using System;
using SunRise.CyberLock.ClientSide.DAL;

namespace SunRise.CyberLock.ClientSide.BL
{
    public class TaskbarNotifierHandler
    {

        public readonly Session ClientSession = new Session();
        public TaskbarNotifierHandler(Session session)
        {
            ClientSession = session;
        }

        public String GetRemainingTime()
        {
            return ClientSession.RemainingTime;
        }
    }
}
