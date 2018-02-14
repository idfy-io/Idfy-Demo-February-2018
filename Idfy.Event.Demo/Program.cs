using System;
using System.Threading.Tasks;
using Idfy.Demo.Common;
using Idfy.Events.Client;
using Idfy.Events.Entities;
using Serilog;

namespace Idfy.Event.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = new Serilog.LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.ColoredConsole()
                .CreateLogger();

            Log.Logger = logger;

            var client = EventClient.Setup(Credentials.ClientId, Credentials.ClienttSecret)
              //  .AddRebusCompatibeLogger(x =>x.ColoredConsole())
               // .LogToConsole(LogLevel.Debug)
                .SubscribeToAllEvents(allEvents)
                .Subscribe<DocumentPackagedEvent>(documentPackageEvent)
                .Subscribe<DocumentSignedEvent>(documentSignedEvent)
                .Subscribe<DocumentLinkOpenedEvent>(documentLinkEvent)
                .Start();

            Console.ReadKey();
            client.Dispose();
        }

        private static Task documentLinkEvent(DocumentLinkOpenedEvent arg)
        {
            Console.WriteLine("Document er åpnet fra: {0} {1}",arg.Payload.UserAgent,arg.Timestamp);
            return Task.FromResult(0);
        }

        private static Task documentSignedEvent(DocumentSignedEvent arg)
        {
            foreach (var payloadSigner in arg.Payload.Signers)
            {
                Console.WriteLine("Documentet er signert av: {0} med {1}",payloadSigner.FullName,payloadSigner.SignatureMethod);
            }
            return Task.FromResult(0);
        }

        private static Task documentPackageEvent(DocumentPackagedEvent arg)
        {
            Log.Logger.Information(Newtonsoft.Json.JsonConvert.SerializeObject(arg));
            return Task.FromResult(0);
        }

        private static Task allEvents(Events.Entities.Event arg)
        {
            Console.WriteLine("{0}: {1}",arg.Timestamp, arg.Type);

            return Task.FromResult(0);
        }
    }
}
