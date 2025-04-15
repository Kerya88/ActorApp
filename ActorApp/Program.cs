using ActorApp.Actors;
using ActorApp.Messages;
using Akka.Actor;
using System.Diagnostics;
using System.Globalization;

namespace ActorApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            const int numTerms = 10_000_000;
            const int numWorkers = 10;
            const int chunkSize = numTerms / numWorkers;
            const string token = "18k0cqv82y0jyec5y2y5t9fbbnsqoa10a056etlezxc8p89o0juv94wuej38v3gy";
            
            var everestClient = new EverestClient(token);
            
            Console.WriteLine("Запускаем вычисление на Everest...");
            
            var tasks = new List<Task<string>>();
            
            for (var i = 0; i < numWorkers; i++)
            {
                var start = i * chunkSize;
                var end = (i == numWorkers - 1) ? numTerms : start + chunkSize;

                var arguments = new Dictionary<string, object>
                {
                    { "s", start.ToString() },
                    { "f", end.ToString() }
                };

                //var jodId =  everestClient.RunJob($"Job{i + 1}", ["67c6a7a61200001500ee152b"], arguments, "67d2993b1200001100ee1963");
                //var result = everestClient.CheckState(jodId);

                tasks.Add(new Task<string>(() => everestClient.RunJob($"Job{i + 1}", ["67c6a7a61200001500ee152b"], arguments, "67d2993b1200001100ee1963")));
            }

            tasks.ForEach(x => x.Start());
            await Task.WhenAll(tasks);

            var jodIds = tasks.Select(x => x.Result).ToList();
            
            var results = jodIds.Select(x => everestClient.CheckState(x)).ToList();

            if (results.Any(x => !x.Item1))
            {
                Console.WriteLine($"Everest вернул ошибку");
            }
            else
            {
                var piResult = results.Select(x => double.Parse(x.Item2.Split("&")[0], NumberStyles.Float, CultureInfo.InvariantCulture)).Sum() * 4;
                var timeResult = results.Select(x => long.Parse(x.Item2.Split("&")[1])).Sum();
            
                Console.WriteLine($"Приближенное значение pi на Everest: {piResult}");
                Console.WriteLine($"Время выполнения на Everest: {timeResult} мс\n");
            }
            
            Console.WriteLine("Запускаем вычисление без акторов...");
            var stopwatch = Stopwatch.StartNew();
            var piSingleThread = CalculatePiSingleThread(numTerms);
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

        private static double CalculatePiSingleThread(int numTerms)
        {
            double sum = 0;
            for (var k = 0; k < numTerms; k++)
            {
                sum += (k % 2 == 0 ? 1.0 : -1.0) / (2 * k + 1);
            }
            return 4 * sum;
        }
    }
}
