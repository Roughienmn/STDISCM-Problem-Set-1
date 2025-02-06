using System;
using System.Threading;
using System.IO;
using System.ComponentModel.DataAnnotations;
using System.Collections.Concurrent;
using System.Linq;

class Program
{
    static int x, y;
    static String print = "", division = "";
    static ConcurrentDictionary<int, (int threadID, DateTime timestamp)> primeMap = new ConcurrentDictionary<int, (int threadID, DateTime timestamp)>();

    static void Main(string[] args)
    {
        //read config file
        ReadConfig();

        Thread[] threads = new Thread[x];

        if (division == "straight")
        {
            int mainStart = 1;

            //max number of numbers to check per thread
            int maxperthread = (int)Math.Ceiling((double)y / x);

            //create threads
            for (int i = 0; i < x; i++)
            {
                //copy of variables for thread
                int threadID = i;
                int start = mainStart;
                int end = Math.Min(mainStart + maxperthread - 1, y);

                //create and start thread
                threads[i] = new Thread(() => StraightSearch(threadID, start, end));

                threads[i].Start();

                mainStart = end + 1; 
            }

            //wait for threads to finish
            foreach (Thread thread in threads)
            {
                thread.Join();
            }
        }

        if (division == "linear")
        {
            for (int j = 2; j <= y; j++)
            {
                ConcurrentBag<bool> linearSearchResults = new ConcurrentBag<bool>();

                for (int i = 0; i < x; i++)
                {
                    int threadID = i;
                    int numberToCheck = j;

                    threads[i] = new Thread(() => LinearSearch(threadID, numberToCheck, linearSearchResults));
                    threads[i].Start();
                }

                foreach (Thread thread in threads)
                {
                    thread.Join();
                }

                bool isPrime = linearSearchResults.All(result => result);
                if (isPrime)
                {
                    DateTime timestamp = DateTime.Now;
                    int assignedThread = j % x;

                    if (print == "immediate")
                    {
                        printPrimeLine(j, assignedThread, timestamp);
                    }
                    primeMap[j] = (assignedThread, timestamp);

                }
            }
        }

        //print primes at the end of set to wait
        if (print == "wait")
        {
            printPrimes();
        }

        if(primeMap.Count == 0)
        {
            Console.WriteLine("No primes found.");
        }
    }

    static void ReadConfig()
    {
        string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string filePath = Path.Combine(currentDirectory, @"..\..\..\config.txt");
        try
        {
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    string[] split = line.Split('=');
                    if (split.Length == 2)
                    {
                        if (split[0].Trim() == "x")
                        {
                            x = int.Parse(split[1].Trim());
                        }
                        else if (split[0].Trim() == "y")
                        {
                            y = int.Parse(split[1].Trim());
                        }
                        else if (split[0].Trim() == "print")
                        {
                            print = split[1].Trim();
                        }
                        else if (split[0].Trim() == "division")
                        {
                            division = split[1].Trim();
                        }
                    }
                }
                if (x < 1)
                {
                    Console.WriteLine("Invalid x value. Exiting program.");
                    Environment.Exit(0);
                }
                if (y < 1)
                {
                    Console.WriteLine("Invalid y value. Exiting program.");
                    Environment.Exit(0);
                }
                if (print != "immediate" && print != "wait")
                {
                    Console.WriteLine("Invalid print setting. Should be 'immediate' or 'wait' Exiting program.");
                    Environment.Exit(0);
                }
                if (division != "linear" && division != "straight")
                {
                    Console.WriteLine("Invalid division setting. Should be 'linear' or 'straight' Exiting program.");
                    Environment.Exit(0);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
    }

    //Linear Search (1 modulo operation per thread)
    static void LinearSearch(int threadID, int number, ConcurrentBag<bool> results)
    {
        if (number < 2)
        {
            results.Add(false);
            return;
        }

        bool isPrime = true;
        for (int i = 2 + threadID; i * i <= number; i += x)
        {
            if (number % i == 0)
            {
                isPrime = false;
                break;
            }
        }
        results.Add(isPrime);
    }

    //Straight Search (specified range search for a number)
    static void StraightSearch(int threadID, int start, int end)
    {
        for (int i = start; i <= end; i++)
        {
            if (isPrime(i))
            {
                DateTime timestamp = DateTime.Now;
                if (print =="immediate")
                {
                    printPrimeLine(i, threadID, timestamp);
                }
                primeMap[i] = (threadID, timestamp);
             }
        }
    }

    //prime search for straight division
    static bool isPrime(int num)
    {
        if (num <= 1) return false;
        for(int i = 2; i <= Math.Sqrt(num); i++)
        {
            if (num % i == 0) return false;
        }
        return true;
    }

    static void printPrimeLine(int prime, int threadID, DateTime timestamp)
    {
        Console.WriteLine($"{"Number:",-8} {prime,-10} {"Thread ID:",-12} {threadID,-4} {"Timestamp:",-12} {timestamp:yyyy-MM-dd HH:mm:ss.fff}");
    }

    static void printPrimes()
    {
        Console.WriteLine("Primes found:");

        //for each existing pair in the dictionary
        var sortedPrimes = primeMap.Keys.OrderBy(p => p).ToList();
        foreach (var prime in sortedPrimes)
        {
            var (threadID, timestamp) = primeMap[prime];
            printPrimeLine(prime, threadID, timestamp);
        }
    }
}