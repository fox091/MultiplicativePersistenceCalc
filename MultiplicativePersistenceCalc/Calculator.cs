using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace MultiplicativePersistenceCalc
{
    public class Calculator
    {
        #region Private Variables
        private readonly ConcurrentQueue<BigInteger> inputs = new ConcurrentQueue<BigInteger>();
        private BigInteger currentNumber;
        private readonly Timer populateTimer = new Timer();
        private readonly Timer calculateTimer = new Timer();
        private readonly object populateLocker = new object();
        private readonly object calculateLocker = new object();
        private bool isPopulateRunning = false;
        private bool isCalculateRunning = false;
        private bool requestPopulateStop = false;
        private bool requestCalculateStop = false;
        #endregion

        #region Constructors
        public Calculator(int startingLength)
        {
            currentNumber = GenerateStartingNumber(startingLength);
        }
        #endregion

        #region Helper Methods
        public static int RecursiveCalc(BigInteger input, int depth = 0)
        {
            if ((int)Math.Floor(BigInteger.Log10(input) + 1) == 1 || input == BigInteger.Zero)
            {
                return depth;
            }
            BigInteger newTotal = 1;
            while (input != BigInteger.Zero)
            {
                newTotal *= input % 10;
                input /= 10;
            }
            return RecursiveCalc(newTotal, depth + 1);
        }

        private BigInteger GenerateStartingNumber(int length)
        {
            if (length < 3)
            {
                return new BigInteger(26);
            }
            BigInteger multiplier = new BigInteger(10);
            BigInteger result = new BigInteger(6);

            for (int i = 2; i < length; i++)
            {
                result += 6 * multiplier;
                multiplier *= 10;
            }
            result += 2 * multiplier;
            return result;
        }

        private BigInteger GetNextNumber()
        {
            // For now this will just increment by 1.
            return currentNumber++;
            // TODO: Make this increment more intelligently.
        }
        #endregion

        #region Start
        public void StartCalculating(CancellationToken ct)
        {
            StartQueuePopulation();

            StartQueueProcessing();

            ct.WaitHandle.WaitOne();

            // TODO: Add logic to wait for current timer tasks to spin down
        }
        #endregion

        #region Queue Population
        private void StartQueuePopulation()
        {
            // TODO: Make interval configurable
            populateTimer.Interval = 500;
            populateTimer.AutoReset = false;
            populateTimer.Enabled = true;
            populateTimer.Elapsed += PopulateQueue;
            populateTimer.Start();
        }

        private void PopulateQueue(object sender, EventArgs e)
        {
            lock (populateLocker)
            {
                if (requestPopulateStop)
                {
                    return;
                }
                else
                {
                    populateTimer.Stop();
                    isPopulateRunning = true;
                }
            }

            while (inputs.Count < 500)
            {
                // I has dumb.  BigInteger is a value type and doesn't pass by reference.
                // So I can modify and enqueue just fine.
                inputs.Enqueue(GetNextNumber());
            }

            lock (populateLocker)
            {
                isPopulateRunning = false;
                if (requestPopulateStop)
                {
                    return;
                }
                else
                {
                    populateTimer.Start();
                }
            }
        }
        #endregion

        #region Queue Processing
        private void StartQueueProcessing()
        {
            // TODO: Make interval configurable
            calculateTimer.Interval = 500;
            calculateTimer.AutoReset = false;
            calculateTimer.Enabled = true;
            calculateTimer.Elapsed += ProcessQueue;
            calculateTimer.Start();
        }

        private void ProcessQueue(object sender, EventArgs e)
        {

            lock (calculateLocker)
            {
                if (requestCalculateStop)
                {
                    return;
                }
                else
                {
                    calculateTimer.Stop();
                    isCalculateRunning = true;
                }
            }

            // TODO: Make batch size configurable
            Parallel.For(0, Math.Min(500, inputs.Count), (i) => ProcessQueuedItem());

            lock (calculateLocker)
            {
                isCalculateRunning = false;
                if (requestCalculateStop)
                {
                    return;
                }
                else
                {
                    calculateTimer.Start();
                }
            }
        }

        private void ProcessQueuedItem()
        {
            if (inputs.TryDequeue(out BigInteger item))
            {
                int multiplicativePersistence = RecursiveCalc(item);
                // TODO: Make configurable
                if (multiplicativePersistence > 11)
                {
                    // Hooray!@!@!@!
                    Console.WriteLine("I found a thing!");
                    Console.WriteLine(item.ToString() + " has a multiplicative persistence of " + multiplicativePersistence);
                }
            }
        }
        #endregion
    }
}
