using SharedMemory;
using System;

namespace SharedMemoryConsole
{
    class Program
    {

        static void Main(string[] args)
        {
            EventMemory eventMemory = new EventMemory(1);

            eventMemory.WriteEvent("hola mundo\r\n");
            eventMemory.WriteEvent("otro evento\r\n");
            eventMemory.WriteEvent("evento final\r\n");
            eventMemory.WriteEvent("CAtalog", "Bizagi", "Un valor pequeñop");

            for (int i = 0; i < 100; i++)
            {
                eventMemory.WriteEvent("Other", "Custom" + i, @"2022-04-08 08:54:23 BIZAGI UPGRADER LOG ----- INFORMATION -- UpdateAsyncTaskTime
2022-04-08 08:54:24 BIZAGI UPGRADER LOG ----- INFORMATION -- In process Generar Comprobante Recepción, version 1.3, in activity Generar Documento the timeout was modified from 0 to 30 seconds.
2022-04-08 08:54:24 BIZAGI UPGRADER LOG ----- INFORMATION -- In process Generar Comprobante Recepción, version 1.4, in activity Generar Documento the timeout was modified from 0 to 30 seconds.
2022-04-08 08:54:24 BIZAGI UPGRADER LOG ----- INFORMATION -- RemoveInvisbleCharsInEnvironmentObject
2022-04-08 08:54:24 BIZAGI UPGRADER LOG ----- INFORMATION -- UpgradeAuthenticationObject
2022-04-08 08:54:24 BIZAGI UPGRADER LOG ----- INFORMATION -- UpgradeAttribBooleanDefaultValue
2022-04-08 08:54:24 BIZAGI UPGRADER LOG ----- INFORMATION -- End: Upgrading database FlujoPAMDesa on server MAXPC\SQL2019DEV...");
            }

            Console.ReadLine();
        }

    }
}
