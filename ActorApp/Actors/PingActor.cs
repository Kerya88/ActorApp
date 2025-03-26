using ActorApp.Messages;
using Akka.Actor;

namespace ActorApp.Actors
{
    public class PingActor : ReceiveActor
    {
        private readonly IActorRef _pongActor;

        public PingActor(IActorRef pongActor)
        {
            _pongActor = pongActor;

            Receive<PingMessage>(_ =>
            {
                Console.WriteLine("Ping");
                _pongActor.Tell(new PongMessage());
            });

            Receive<PongMessage>(_ =>
            {
                Console.WriteLine("Pong получен -> Отправляем Ping снова...");
                Thread.Sleep(500);
                Self.Tell(new PingMessage());
            });
        }
    }
}
