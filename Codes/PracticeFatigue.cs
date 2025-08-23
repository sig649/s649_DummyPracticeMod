using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace s649_DummyPracticeMod.Codes
{
    internal class PracticeFatigue
    {
        public PracticeFatigue() { value = 0; }
        public PracticeFatigue(int num) { value = num; }
        public int value;
        public const int valueNext = 1000;
        public void Mod(int num) { value += num; }
    }
}
