using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DI.Test
{
    public interface InterfaceWithoutImplementation
    {
    }

    public interface IUseInterfaceWithoutImplementation
    {

    }

    public class UseInterfaceWithoutImplementation : IUseInterfaceWithoutImplementation
    {
        public UseInterfaceWithoutImplementation(InterfaceWithoutImplementation useI)
        {

        }
    }
}
