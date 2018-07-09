using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultyThreadingDemo_2
{
    class Program
    {
        private static volatile bool suspended = false;
        private static int threadCount = 0;
        private static bool finished = false;

        private static HookHelper hookHelper;

        static void Main(string[] args)
        {
            Worker worker = new Worker();

            //int x = 0;

            Thread helloThread = new Thread(Program.hWorldAction);
            Thread worldThread = new Thread(Program.hWorldAction);

            IDictionary<Keys, Object> dict1 =
                new Dictionary<Keys, Object>() {
                    { Keys.Worker, worker }
                    , { Keys.Id, 1 }
                };
            IDictionary<Keys, Object> dict2 =
                new Dictionary<Keys, Object>() {
                    { Keys.Worker, worker }
                    , { Keys.Id, 2 }
                };

            hookHelper = new HookHelper(() => {

                //Console.WriteLine("Trigger");
                if (!finished)
                {
                    suspended = !suspended;
                    if (!suspended)
                    {
                        if (helloThread.IsAlive)
                        {
                            helloThread.Resume();
                        }
                        if (worldThread.IsAlive)
                        {
                            worldThread.Resume();
                        }
                    }
                }
            });

            hookHelper.SetHook();
            
            helloThread.Start(dict1);
            worldThread.Start(dict2);

            Application.Run();

            /*while (!finished)
            {
                Console.ReadLine();
                suspended = !suspended;
                if (!suspended)
                {
                    if (helloThread.IsAlive)
                    {
                        helloThread.Resume();
                    }
                    if (worldThread.IsAlive)
                    {
                        worldThread.Resume();
                    }
                }
            }*/
        }

        private static void hWorldAction(Object args) {

            threadCount++;
            IDictionary<Keys, Object> dict =
                (Dictionary<Keys, Object>)args;

            Worker worker = (Worker)dict[Keys.Worker];
            int id = (int)dict[Keys.Id];
            for (int i = 0; i < 100; )
            {
                Monitor.Enter(worker);
                if (!suspended)
                {
                    while (id != worker.GetState())
                    {
                        Monitor.Wait(worker);
                    }
                    Thread.Sleep(50);
                    if (id == 1)
                    {
                        worker.sayHello();
                        Console.Write(i + " ");
                    }
                    else
                    {
                        Console.Write(" " + i);
                        worker.sayWorld();
                    }
                    
                    Monitor.PulseAll(worker);
                    Monitor.Exit(worker);
                    i++;
                }
                else
                {
                    Monitor.PulseAll(worker);
                    Monitor.Exit(worker);
                    Thread.CurrentThread.Suspend();
                }
            }
            threadCount--;
            OnThreadsFinished();
        }

        private static void OnThreadsFinished() {

            if (threadCount == 0)
            {
                finished = true;
                hookHelper.Unhook();
                Application.Exit();
            }
            
        }
    }
}
