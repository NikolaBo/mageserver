using System;
using System.Collections.Generic;
using System.Text;

namespace MageServer
{
    class GameLogic
    {
        //Called once every tick
        public static void Update()
        {
            //Call each players update method
            foreach(Client c in Server.clients.Values)
            {
                if(c.player != null)
                {
                    c.player.Update(); 
                }
            }

            //Run logic set aside for main thread
            ThreadManager.UpdateMain();
        }
    }
}
