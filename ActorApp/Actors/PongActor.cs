using ActorApp.Messages;
using Akka.Actor;

namespace ActorApp.Actors
{
    public class PongActor : ReceiveActor
    {
        public PongActor()
        {
            Receive<PongMessage>(_ =>
            {
                Console.WriteLine("Pong");
                Sender.Tell(new PongMessage());
            });
        }
    }
}
