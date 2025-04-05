using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BILET2
{
    public class TransportProblem
    {
        public int[] Supply { get; set; }
        public int[] Demand { get; set; }
        public int[,] Costs { get; set; }
        public int[,] Solution { get; set; }
        public int TotalCost { get; set; }
    }
}