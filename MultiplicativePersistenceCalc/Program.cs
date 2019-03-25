using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace MultiplicativePersistenceCalc
{
    class Program
    {
        static readonly ConcurrentQueue<BigInteger> inputs = new ConcurrentQueue<BigInteger>();

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
                    BigInteger startingNumber = GenerateStartingNumber(startingLength);

                    while(true)
                    {
                        inputs.Enqueue(startingNumber++);
                    }
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

        static void PopulateQueue(BigInteger startingNumber, CancellationToken ct)
        {
            while(true)
            {
                if(ct.IsCancellationRequested)
                {
                    break;
                }
                if(inputs.Count < 500)
                {
                    inputs.Enqueue(startingNumber++);
                }
            }
        }

        static void ProcessQueue(CancellationToken ct)
        {

        }
    }
}
