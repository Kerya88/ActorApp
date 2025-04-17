using ActorApp.Messages;
using Akka.Actor;
using System.Diagnostics;

namespace ActorApp.Actors
{
    //мастер актор
    public class MasterActor : ReceiveActor
    {
        //количество элементов ряда
        private readonly int _numTerms;
        //количество рабочих акторов
        private readonly int _numWorkers;
        //количество собранных ответов
        private int _responsesReceived;
        //аккумулятор частичных сумм
        private double _piSum;
        //таймер
        private readonly Stopwatch _stopwatch;

        public MasterActor(int numTerms, int numWorkers, Stopwatch stopwatch)
        {
            _numTerms = numTerms;
            _numWorkers = numWorkers;
            _stopwatch = stopwatch;

            //делегат обработки сообщения о начале вычислений
            Receive<StartCalculationMessage>(_ => StartCalculation());
            //делегат обработки сообщения о результате вычисления частичной суммы
            Receive<ResultMessage>(ProcessResult);
        }

        //обработка сообщения о начале вычислений
        private void StartCalculation()
        {
            _stopwatch.Restart();
            //расчет количества элементов в диапазоне для одного актора
            var chunkSize = _numTerms / _numWorkers;
            for (var i = 0; i < _numWorkers; i++)
            {
                //начальное значение диапазона
                var start = i * chunkSize;
                //конечное значение диапазона
                var end = (i == _numWorkers - 1) ? _numTerms : start + chunkSize;
                //инициализация нового актора рабочего
                var worker = Context.ActorOf<WorkerActor>();
                //передача актору рабочему сообщения о вычислении частичной суммы
                worker.Tell(new WorkMessage(start, end));
            }
        }

        //обработка сообщения о результате вычисления частичной суммы
        private void ProcessResult(ResultMessage result)
        {
            _piSum += result.PartialSum;
            _responsesReceived++;

            if (_responsesReceived == _numWorkers)
            {
                _stopwatch.Stop();
                Console.WriteLine($"Приближенное значение pi (с акторами): {4 * _piSum}");
                Console.WriteLine($"Время выполнения (с акторами): {_stopwatch.ElapsedMilliseconds} мс");
                //тушим систему акторов
                Context.System.Terminate();
            }
        }
    }
}
