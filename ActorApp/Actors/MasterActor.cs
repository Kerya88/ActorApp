using ActorApp.Messages;
using Akka.Actor;
using System.Diagnostics;

namespace ActorApp.Actors
{
    public class MasterActor : ReceiveActor
    {
        private readonly int _numTerms;
        private readonly int _numWorkers;
        private int _responsesReceived = 0;
        private double _piSum = 0;
        private readonly Stopwatch _stopwatch;

        public MasterActor(int numTerms, int numWorkers, Stopwatch stopwatch)
        {
            _numTerms = numTerms;
            _numWorkers = numWorkers;
            _stopwatch = stopwatch;

            Receive<StartCalculationMessage>(_ => StartCalculation());
            Receive<ResultMessage>(result => ProcessResult(result));
        }

        private void StartCalculation()
        {
            _stopwatch.Restart();
            int chunkSize = _numTerms / _numWorkers;
            for (int i = 0; i < _numWorkers; i++)
            {
                int start = i * chunkSize;
                int end = (i == _numWorkers - 1) ? _numTerms : start + chunkSize;
                var worker = Context.ActorOf<WorkerActor>();
                worker.Tell(new WorkMessage(start, end));
            }
        }

        private void ProcessResult(ResultMessage result)
        {
            _piSum += result.PartialSum;
            _responsesReceived++;

            if (_responsesReceived == _numWorkers)
            {
                _stopwatch.Stop();
                Console.WriteLine($"Приближенное значение pi (с акторами): {4 * _piSum}");
                Console.WriteLine($"Время выполнения (с акторами): {_stopwatch.ElapsedMilliseconds} мс");
                Context.System.Terminate();
            }
        }
    }
}
