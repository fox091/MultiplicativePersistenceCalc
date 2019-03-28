using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

using Timer = System.Timers.Timer;

namespace MultiplicativePersistenceCalc
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Would you like to calculate from a starting length? (yes/no)");
            bool validAnswer = false;
            string yesno = Console.ReadLine();
            while (validAnswer == false)
            {
                if (yesno == "no")
                {
                    validAnswer = true;
                    bool shouldQuit = false;
                    while (shouldQuit == false)
                    {
                        Console.WriteLine("Enter a number to calulate the multiplicative persistence:");
                        BigInteger input;
                        while (BigInteger.TryParse(Console.ReadLine(), out input) == false)
                        {
                            Console.WriteLine("Invalid input.  Try again.");
                        }
                        int depth = Calculator.RecursiveCalc(input, 0);
                        Console.WriteLine(depth);
                        Console.WriteLine("Press esc to quick or any other key to do it again.");
                        ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                        if (keyInfo.Key == ConsoleKey.Escape)
                        {
                            shouldQuit = true;
                        }
                    }
                }
                else if (yesno == "yes")
                {
                    validAnswer = true;
                    Console.WriteLine("Enter the starting length:");
                    int startingLength = 0;
                    while(Int32.TryParse(Console.ReadLine(), out startingLength) == false)
                    {
                        Console.WriteLine("Invalid input.  Try again.");
                    }

                    var calculator = new Calculator(startingLength);

                    CancellationTokenSource cts = new CancellationTokenSource();
                    CancellationToken ct = cts.Token;

                    Task task = Task.Factory.StartNew(() => calculator.StartCalculating(ct), ct, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                    Console.WriteLine("Successfully started calculating.");

                    Console.WriteLine("Press ESC to exit.");

                    while(true)
                    {
                        if(Console.ReadKey(true).Key == ConsoleKey.Escape)
                        {
                            cts.Cancel();
                            break;
                        }
                    }

                    task.Wait();

                    Console.WriteLine("Successfully stopped all threads.  Press any key to quit.");
                    Console.ReadKey(true);

                }
                else
                {
                    Console.WriteLine("Invalid response. Try again.");
                }
            }

        }
    }
}
