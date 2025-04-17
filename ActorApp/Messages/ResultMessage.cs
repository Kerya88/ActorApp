namespace ActorApp.Messages
{
    //сообщение о результате вычисления частичной суммы
    public class ResultMessage(double partialSum)
    { 
        public double PartialSum = partialSum;
    }
}
