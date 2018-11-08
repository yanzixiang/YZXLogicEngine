using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YZXLogicEngine.Unit
{
    public class ModbusUnit:YZXUnit
    {

        public ModbusUnit()
        { 
            
        }

        #region YZXUnit
        public override bool ReadB(int wIndex, int bIndex)
        {
            throw new NotImplementedException();
        }

        public override short ReadW(int wIndex)
        {
            throw new NotImplementedException();
        }

        public override int ReadDW(int dwIndex)
        {
            throw new NotImplementedException();
        }
        #endregion YZXUnit
    }
}
