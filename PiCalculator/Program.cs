using System.Diagnostics;

namespace PiCalculator
{
    public class Program
    {
        static void Main(string[] args)
        {
            var start = int.Parse(args[0]);
            var finish = int.Parse(args[1]);
            
            var sum = 0d;
            
            var stopwatch = Stopwatch.StartNew();
            for (var k = start; k < finish; k++)
            {
                sum += (k % 2 == 0 ? 1.0 : -1.0) / (2 * k + 1);
            }
            stopwatch.Stop();
            
            Console.WriteLine($"{sum}&{stopwatch.ElapsedMilliseconds}");
        }
    }
}
