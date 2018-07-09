using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultyThreadingDemo_2
{
    public class Worker
    {
        private int state = 1;

        public void sayHello() {

            Console.Write("Hello ");
            state = 2;
        }

        public void sayWorld()
        {

            Console.Write("World\n");
            state = 1;
        }

        public int GetState() { return state; }
    }
}
