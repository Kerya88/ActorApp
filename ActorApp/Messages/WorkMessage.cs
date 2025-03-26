namespace ActorApp.Messages
{
    public class WorkMessage(int start, int end)
    {
        public int Start = start;
        public int End = end;
    }
}
