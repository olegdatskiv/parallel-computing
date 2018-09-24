using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Task3
{
    class Program
    {
        static void Main(string[] args)
        {            
            int n = 999;
            int[,] matrixA = new int[n, n];
            RandomFillMatrix(matrixA, n);
            int[] b = new int[n];
            RandomFillFreeMembers(b, n);

            Stopwatch sequentialWatch = new Stopwatch();
            sequentialWatch.Start();
            double[] roots = GaussMethodForSingleThread(matrixA, b, n);
            sequentialWatch.Stop();

            Console.WriteLine($"Time for sequential calculation: {sequentialWatch.Elapsed.ToString()}");

            Stopwatch parallelWatch = new Stopwatch();

            parallelWatch.Start();
            double[] rootsByParallel = GaussMethodForManyThreads(matrixA, b, n, 5);
            parallelWatch.Stop();
            Console.WriteLine($"Time for parallel calculation with {5} threads : {parallelWatch.Elapsed.ToString()}");
            
            parallelWatch.Restart();
            double[] rootsByParallel1 = GaussMethodForManyThreads(matrixA, b, n, 10);
            parallelWatch.Stop();
            Console.WriteLine($"Time for parallel calculation with {10} threads : {parallelWatch.Elapsed.ToString()}");
            
            parallelWatch.Restart();
            double[] rootsByParallel2 = GaussMethodForManyThreads(matrixA, b, n, 20);
            parallelWatch.Stop();
            Console.WriteLine($"Time for parallel calculation with {20} threads : {parallelWatch.Elapsed.ToString()}");


            Console.Read();
        }

        static void RandomFillMatrix(int[,] mat, int n)
        {
            Random rand = new Random();
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    mat[i, j] = rand.Next(100);
                }
            }
        }

        static void RandomFillFreeMembers(int[] b, int n)
        {
            Random rand = new Random();

            for (int i = 0; i < n; i++)
            {
                b[i] = rand.Next(100);
            }
        }

        static double[] GaussMethodForSingleThread(int[,] linearSystem, int[] freeMembeers, int dimension)
        {
            double[] result = new double[dimension];

            double temp = 0;

            try {
                for (int k = 0; k < dimension - 1; k++)
                {
                    for (int i = k + 1; i < dimension; i++)
                    {
                        for (int j = k + 1; j < dimension; j++)
                        {
                            linearSystem[i, j] = linearSystem[i, j] - linearSystem[k, j] * (linearSystem[i, k] / linearSystem[k, k]);
                        }
                        freeMembeers[i] = freeMembeers[i] - freeMembeers[k] * linearSystem[i, k] / linearSystem[k, k];
                    }
                }
            }
            catch (DivideByZeroException er)
            {
                 Console.WriteLine(er.Message);
            }

            for (int k = dimension - 1; k >= 0; k--)
            {
                temp = 0;
                for (int j = k + 1; j < dimension; j++)
                {
                    temp = temp + linearSystem[k, j] * result[j];
                    result[k] = (freeMembeers[k] - temp) / linearSystem[k, k];
                }                    
            }
            
            return result;
        }

        static double[] GaussMethodForManyThreads(int[,] linearSystem, int[] freeMembeers, int dimension, int threadsNumber)
        {
            double[] result = new double[dimension];
            Task[] threads = new Task[threadsNumber];

            for(int i = 0; i < dimension - 1; i++)
            {
                int rowForThread = (int)Math.Ceiling((double)(dimension - (i + 1)) / threadsNumber);

                Func<object, int> act = (object processRow) =>
                {
                    int rowForProcess = (int)processRow;

                    for (int j = rowForProcess; j < rowForProcess + rowForThread; ++j)
                    {
                        if (j < dimension)
                        {
                            if (linearSystem[i, i] != 0)
                            {
                                int elem = linearSystem[j,i] / linearSystem[i,i];

                                for (int k = i; k < dimension; k++)
                                {
                                    linearSystem[j, k] -= linearSystem[i, k] * elem;
                                }

                                freeMembeers[j] -= freeMembeers[i] * elem;
                            }
                        }
                    }
                    return 1;
                };

                int c = 0;

                for (var j = i + 1; j < dimension; j += rowForThread)
                {
                    threads[c] = Task<int>.Factory.StartNew(act, j);
                    ++c;
                }

                Task.WaitAll(threads);
            }
            
            return result;
        }
    }
}
