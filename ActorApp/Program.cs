using ActorApp.Actors;
using ActorApp.Messages;
using Akka.Actor;
using System.Diagnostics;

namespace ActorApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var numTerms = 10_000_000;
            var numWorkers = 10;
            var token = "46wkk3t8v3mdp5s7itukz1xilhey1dm5a1bnlbfpsbc2d3r8xyfnv7coi9mk9d8a";
            var everestClient = new EverestClient(token);

            //token = await everestClient.GetToken("Kerya88", "RUZ365gar31", "ActorApp");

            var chunkSize = numTerms / numWorkers;
            var tasks = new List<Task>();

            for (var i = 0; i < numWorkers; i++)
            {
                var start = i * chunkSize;
                var end = (i == numWorkers - 1) ? numTerms : start + chunkSize;

                var arguments = new Dictionary<string, object>
                {
                    { "s", start },
                    { "f", end }
                };

                await everestClient.RunJob($"Job {i + 1}", ["67c6a7a61200001500ee152b"], arguments, "67d2993b1200001100ee1963");

                tasks.Add(new Task(() => everestClient.RunJob($"Job {i + 1}", ["67c6a7a61200001500ee152b"], arguments, "")));
            }

            tasks.ForEach(x => x.Start());
            await Task.WhenAll(tasks);


            Console.WriteLine("Запускаем вычисление без акторов...");
            var stopwatch = Stopwatch.StartNew();
            double piSingleThread = CalculatePiSingleThread(numTerms);
            stopwatch.Stop();
            Console.WriteLine($"Приближенное значение pi (без акторов): {piSingleThread}");
            Console.WriteLine($"Время выполнения (без акторов): {stopwatch.ElapsedMilliseconds} мс\n");

            Console.WriteLine("Запускаем вычисление с акторами...");
            using (var system = ActorSystem.Create("PiCalculationSystem"))
            {
                var master = system.ActorOf(Props.Create(() => new MasterActor(numTerms, numWorkers, stopwatch)), "master");
                master.Tell(new StartCalculationMessage());

                system.WhenTerminated.Wait();
            }
        }

        static double CalculatePiSingleThread(int numTerms)
        {
            double sum = 0;
            for (int k = 0; k < numTerms; k++)
            {
                sum += (k % 2 == 0 ? 1.0 : -1.0) / (2 * k + 1);
            }
            return 4 * sum;
        }
    }
}
