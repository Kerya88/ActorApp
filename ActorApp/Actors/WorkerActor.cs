using ActorApp.Messages;
using Akka.Actor;

namespace ActorApp.Actors
{
    //актор рабочий
    public class WorkerActor : ReceiveActor
    {
        public WorkerActor()
        {
            //обработка сообщения о вычислении частичной суммы
            Receive<WorkMessage>(work =>
            {
                double sum = 0;
                for (var k = work.Start; k < work.End; k++)
                {
                    sum += (k % 2 == 0 ? 1.0 : -1.0) / (2 * k + 1);
                }
                //отправка сообщения с результатом вычисления
                Sender.Tell(new ResultMessage(sum));
            });
        }
    }
}
