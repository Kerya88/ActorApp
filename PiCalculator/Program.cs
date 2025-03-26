namespace PiCalculator
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2 || !int.TryParse(args[0], out var start) || !int.TryParse(args[1], out var finish) || start > finish)
            {
                Console.WriteLine("Должно быть два числовых аргумента");
            }
            else
            {
                var sum = 0d;
                for (var k = start; k < finish; k++)
                {
                    sum += (k % 2 == 0 ? 1.0 : -1.0) / (2 * k + 1);
                }

                Console.WriteLine(sum);
            }
        }
    }
}
