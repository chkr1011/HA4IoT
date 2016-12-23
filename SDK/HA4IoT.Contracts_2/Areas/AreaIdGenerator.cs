using System;

namespace HA4IoT.Contracts.Areas
{
    public static class AreaIdGenerator
    {
        public static readonly AreaId EmptyId = new AreaId("?");

        public static AreaId Generate(Enum id)
        {
            return new AreaId(id.ToString());
        }
    }
}
