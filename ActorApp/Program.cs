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
            //количество элементов ряда
            const int numTerms = 10_000_000;
            //количество акторов рабочих
            const int numWorkers = 10;
            //расчет количества элементов в диапазоне для одного актора
            const int chunkSize = numTerms / numWorkers;
            //токен авторизации на everest
            const string token = "18k0cqv82y0jyec5y2y5t9fbbnsqoa10a056etlezxc8p89o0juv94wuej38v3gy";
            
            //инициализация everest клиента
            var everestClient = new EverestClient(token);
            
            Console.WriteLine("Запускаем вычисление на Everest...");
            
            var tasks = new List<Task<string>>();
            
            for (var i = 0; i < numWorkers; i++)
            {
                //начальное значение диапазона
                var start = i * chunkSize;
                //конечное значение диапазона
                var end = (i == numWorkers - 1) ? numTerms : start + chunkSize;

                //аргументы для приложения на платформе everest
                var arguments = new Dictionary<string, object>
                {
                    { "s", start.ToString() },
                    { "f", end.ToString() }
                };

                //var jodId =  everestClient.RunJob($"Job{i + 1}", ["67c6a7a61200001500ee152b"], arguments, "67d2993b1200001100ee1963");
                //var result = everestClient.CheckState(jodId);

                var i1 = i;
                //добавление задачи на вычисление частичной суммы
                tasks.Add(new Task<string>(() => everestClient.RunJob($"Job{i1 + 1}", ["67c6a7a61200001500ee152b"], arguments, "67d2993b1200001100ee1963")));
            }

            //одновременный запуск всех задач
            tasks.ForEach(x => x.Start());
            //ожидание выполнения всех задач
            await Task.WhenAll(tasks);

            //получение id работ
            var jodIds = tasks.Select(x => x.Result).ToList();
            
            //проверка статусов всех работ
            var results = jodIds.Select(x => everestClient.CheckState(x)).ToList();

            //суммирование результатов вычисления
            var piResult = results.Select(x => double.Parse(x.Item2.Split("&")[0], NumberStyles.Float, CultureInfo.InvariantCulture)).Sum() * 4;
            //суммирование времени выполнения
            var timeResult = results.Select(x => long.Parse(x.Item2.Split("&")[1])).Sum();
            
            Console.WriteLine($"Приближенное значение pi на Everest: {piResult}");
            Console.WriteLine($"Время выполнения на Everest: {timeResult} мс\n");
            
            Console.WriteLine("Запускаем вычисление без акторов...");
            var stopwatch = Stopwatch.StartNew();
            //вычисление числа pi в однопоточном режиме
            var piSingleThread = CalculatePiSingleThread(numTerms);
            stopwatch.Stop();
            Console.WriteLine($"Приближенное значение pi (без акторов): {piSingleThread}");
            Console.WriteLine($"Время выполнения (без акторов): {stopwatch.ElapsedMilliseconds} мс\n");

            Console.WriteLine("Запускаем вычисление с акторами...");
            //запуск систему акторов
            using (var system = ActorSystem.Create("PiCalculationSystem"))
            {
                //инициализации мастер актора
                var master = system.ActorOf(Props.Create(() => new MasterActor(numTerms, numWorkers, stopwatch)), "master");
                //передача сообщения о начале вычислений
                master.Tell(new StartCalculationMessage());

                //ожидание сигнала о выключении системы
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
