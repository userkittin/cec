using System;
using System.Collections;

namespace cec
{

    public class DaoGpu
    {
        public string VGA { get; set; }
        public string Algo { get; set; }
        public string Hash { get; set; }
        public string Power { get; set; }

        public DaoGpu() { }
        public DaoGpu(string vga, string algo, string hash, string power)
        {
            VGA = vga;
            Algo = algo;
            Hash = hash;
            Power = power;
        }

        public class GpuCollect : ICollection
        {
            public string CollectionName;
            private ArrayList dataArray = new ArrayList();
            public DaoGpu this[int index]
            {
                get { return (DaoGpu)dataArray[index]; }
            }

            public void CopyTo(Array a, int index)
            {
                dataArray.CopyTo(a, index);
            }
            public int Count
            {
                get { return dataArray.Count; }
            }
            public object SyncRoot
            {
                get { return this; }
            }
            public bool IsSynchronized
            {
                get { return false; }
            }
            public IEnumerator GetEnumerator()
            {
                return dataArray.GetEnumerator();
            }

            public void Add(DaoGpu newgpu)
            {
                dataArray.Add(newgpu);
            }

            public DaoGpu Search(int index)
            {
             return (DaoGpu)dataArray[index]; 
            }

        }

    }
}