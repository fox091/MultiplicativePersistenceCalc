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
        static readonly ConcurrentQueue<BigInteger> inputs = new ConcurrentQueue<BigInteger>();
        static readonly Timer populateTimer = new Timer();
        static readonly Timer calculateTimer = new Timer();
        static BigInteger currentNumber;

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
                        int depth = RecursiveCalc(input, 0);
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
                    currentNumber = GenerateStartingNumber(startingLength);

                    var tasks = new List<Task>(2);

                    CancellationTokenSource cts = new CancellationTokenSource();
                    CancellationToken ct = cts.Token;

                    tasks.Add(Task.Factory.StartNew(() => StartQueuePopulation(ct), ct, TaskCreationOptions.LongRunning, TaskScheduler.Default));

                    Console.WriteLine("Successfully started threads to populate queue with numbers.");

                    tasks.Add(Task.Factory.StartNew(() => StartQueueProcessing(ct), ct, TaskCreationOptions.LongRunning, TaskScheduler.Default));

                    Console.WriteLine("Successfully started threads to calculate.");

                    Console.WriteLine("Press ESC to exit.");

                    while(true)
                    {
                        if(Console.ReadKey(true).Key == ConsoleKey.Escape)
                        {
                            cts.Cancel();
                            break;
                        }
                    }

                    Task.WaitAll(tasks.ToArray());

                    Console.WriteLine("Successfully stopped all threads.  Press any key to quit.");
                    Console.ReadKey(true);

                }
                else
                {
                    Console.WriteLine("Invalid response. Try again.");
                }
            }

        }

        static int RecursiveCalc(BigInteger input, int depth = 0)
        {
            if((int)Math.Floor(BigInteger.Log10(input) + 1) == 1 || input == BigInteger.Zero)
            {
                return depth;
            }
            BigInteger newTotal = 1;
            while(input != BigInteger.Zero)
            {
                newTotal *= input % 10;
                input /= 10;
            }
            return RecursiveCalc(newTotal, depth + 1);
        }

        static BigInteger GenerateStartingNumber(int length)
        {
            if(length < 3)
            {
                return new BigInteger(26);
            }
            BigInteger multiplier = new BigInteger(10);
            BigInteger result = new BigInteger(6);

            for(int i = 2; i < length; i++)
            {
                result += 6 * multiplier;
                multiplier *= 10;
            }
            result += 2 * multiplier;
            return result;
        }

        static void StartQueuePopulation(CancellationToken ct)
        {
            populateTimer.Interval = 500;
            populateTimer.AutoReset = false;
            populateTimer.Enabled = true;
            populateTimer.Elapsed += PopulateQueue;
            populateTimer.Start();

            ct.WaitHandle.WaitOne();

            // TODO: Add logic to wait for current timer tasks to spin down
        }

        static void PopulateQueue(object sender, EventArgs e)
        {
            // TODO: Lock and set bool flag that tells that this is running and stop the timer
            while (inputs.Count < 500)
            {
                // TODO: Evaluate whether you can keep modifying and enqueueing the same reference
                //       many times like thise
                inputs.Enqueue(currentNumber++);
            }
            // TODO: Lock and set bool flag that tells that this is done running and start the timer
        }

        static void StartQueueProcessing(CancellationToken ct)
        {
            calculateTimer.Interval = 500;
            calculateTimer.AutoReset = false;
            calculateTimer.Enabled = true;
            calculateTimer.Elapsed += ProcessQueue;
            calculateTimer.Start();

            ct.WaitHandle.WaitOne();

            // TODO: Add logic to wait for current timer tasks to spin down
        }

        static void ProcessQueue(object sender, EventArgs e)
        {
            // TODO: Lock and set bool flag that tells that this is running and stop the timer

            // TODO: Process in parallel

            // TODO: Lock and set bool flag that tells that this is done running and start the timer
        }
    }
}
