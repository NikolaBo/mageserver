using System;
using System.Collections.Generic;
using System.Text;

namespace MageServer
{
    class GameLogic
    {
        public static void Update()
        {
            foreach(Client c in Server.clients.Values)
            {
                if(c.player != null)
                {
                    c.player.Update(); 
                }
            }

            ThreadManager.UpdateMain();
        }
    }
}
