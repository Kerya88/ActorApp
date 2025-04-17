namespace ActorApp.Messages
{
    //сообщение о начале вычислений частичной суммы
    public class WorkMessage(int start, int end)
    {
        public int Start = start;
        public int End = end;
    }
}
