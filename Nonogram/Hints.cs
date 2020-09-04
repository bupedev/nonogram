using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nonogram
{
    [Serializable]
    class Hint
    {
        private int[] blockLengths;

        public int Length 
        {
            get 
            {
                return blockLengths.Length;
            }
        }

        public int this[int i]
        {
            get
            {
                return blockLengths[i];
            }
        }

        public Hint(int[] blockLengths)
        {
            this.blockLengths = blockLengths;
        }

        public int Occupation(int p = 0)
        {
            int sum = 0;
            for (int i = p; i < blockLengths.Length; i++)
            {
                sum += blockLengths[i];
            }
            return sum + (blockLengths.Length - 1 - p);
        }
    }

    [Serializable]
    class HintSet
    {
        Hint[] hintArray;

        public int Length => hintArray.Length; 

        public Hint this[int i]
        {
            get
            {
                return hintArray[i];
            }
        }

        public HintSet(Hint[] hintArray)
        {
            this.hintArray = hintArray;
        }
    }
}
