using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombPeli
{
    public enum GameAction {
        BOMB_RECEIVED,
        PASS_BOMB,
        PASS_BOMB_FAIL,
        WIN,
        LOSE,
        LEAVE,
        PEER_LOST
    }
}
