using ActorApp.Messages;
using Akka.Actor;

namespace ActorApp.Actors
{
    public class WorkerActor : ReceiveActor
    {
        public WorkerActor()
        {
            Receive<WorkMessage>(work =>
            {
                double sum = 0;
                for (int k = work.Start; k < work.End; k++)
                {
                    sum += (k % 2 == 0 ? 1.0 : -1.0) / (2 * k + 1);
                }
                Sender.Tell(new ResultMessage(sum));
            });
        }
    }
}
