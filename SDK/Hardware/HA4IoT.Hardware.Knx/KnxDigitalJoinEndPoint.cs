using HA4IoT.Contracts.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HA4IoT.Hardware.Knx
{
    public class KnxDigitalJoinEndPoint : IBinaryOutput
    {

        public KnxDigitalJoinEndPoint(KnxController knxController)
        {

        }
        public BinaryState Read()
        {
            throw new NotImplementedException();
        }

        public IBinaryOutput WithInvertedState(bool value = true)
        {
            throw new NotImplementedException();
        }

        public void Write(BinaryState state, bool commit = true)
        {
            throw new NotImplementedException();
        }
    }
}
