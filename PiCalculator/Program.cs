using System.Diagnostics;

namespace PiCalculator
{
    //программа для вычисления частичной суммы на платформе everest
    public class Program
    {
        static void Main(string[] args)
        {
            //получение из командной строки начала диапазона вычислений
            var start = int.Parse(args[0]);
            //получение из командной строки конца диапазона вычислений
            var finish = int.Parse(args[1]);
            
            //частичная сумма
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
