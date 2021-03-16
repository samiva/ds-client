using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombPeli
{
    public enum GameStatus : byte
    {
        OPEN = 0x01,
        RUNNING = 0x02,
        ENDED = 0x03
    }
}
