using SharedMemory;
using System;
using System.Threading;

namespace SharedMemoryConsole
{
    class Program
    {
        /*
         * Si se quiere compartir entre varias aplicaciones, se debe dejar de hacer el Clear en el constructor, manejar el esado en el mapfile para el header
         * Para que funcione adecuadamente el webapp de go debe crear los espacios de memoria primero, o volver a ejecutar luego que arranque la webapp de go las pruebas
         */
        static void Main(string[] args)
        {
            //TestDirectSharedMemory();
            TestRestSharedMemory();

            Console.WriteLine("Ended");
            Console.ReadLine();
        }

        private static void TestRestSharedMemory()
        {
            SharedMemoryRest.ClearVars();
            SharedMemoryRest.SetVar("new", 4548);
            SharedMemoryRest.SetVar("sln", 4458);
            SharedMemoryRest.SetVar("gug", 45);
            SharedMemoryRest.SetVar("gag", 40);
        }

        private static void TestDirectSharedMemory()
        {
            EventMemory eventMemory = new EventMemory(1);
            //EventMemory eventMemory1 = EventMemory.GetSingletonInstace(1);

            eventMemory.WriteEvent("hola mundo\r\n");
            eventMemory.WriteEvent("otro evento\r\n");
            eventMemory.WriteEvent("evento final\r\n");
            eventMemory.WriteEvent("cat", 56);
            eventMemory.WriteEvent("CAtalog", "Bizagi", "Un valor pequeñop");

            for (int i = 0; i < 100; i++)
            {
                if (i % 25 == 0)
                {
                    eventMemory = new EventMemory(1);
                }
                eventMemory.WriteEvent("Other", "Custom" + i, @"2022-04-08 08:54:23 BIZAGI UPGRADER LOG ----- INFORMATION -- UpdateAsyncTaskTime
2022-04-08 08:54:24 BIZAGI UPGRADER LOG ----- INFORMATION -- In process Generar Comprobante Recepción, version 1.3, in activity Generar Documento the timeout was modified from 0 to 30 seconds.
2022-04-08 08:54:24 BIZAGI UPGRADER LOG ----- INFORMATION -- In process Generar Comprobante Recepción, version 1.4, in activity Generar Documento the timeout was modified from 0 to 30 seconds.
2022-04-08 08:54:24 BIZAGI UPGRADER LOG ----- INFORMATION -- RemoveInvisbleCharsInEnvironmentObject
2022-04-08 08:54:24 BIZAGI UPGRADER LOG ----- INFORMATION -- UpgradeAuthenticationObject
2022-04-08 08:54:24 BIZAGI UPGRADER LOG ----- INFORMATION -- UpgradeAttribBooleanDefaultValue
2022-04-08 08:54:24 BIZAGI UPGRADER LOG ----- INFORMATION -- End: Upgrading database FlujoPAMDesa on server MAXPC\SQL2019DEV...");
            }

            VarsMemory varsMemory = VarsMemory.GetSingletonInstance();
            varsMemory.SetVar("sl", 445);
            eventMemory.WriteEvent("newF", "vars", varsMemory.GetVars());

            //varsMemory.ClearVars();           
            varsMemory.SetVar("new", 4545);
        }
    }
}
