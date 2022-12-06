using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCatServer.Class.Enums
{
    public enum MoveError
    {
        Success = 0,
        NotFound = 1,
        GateNotFound = 2,
        SelfSector = 2
    }
}
