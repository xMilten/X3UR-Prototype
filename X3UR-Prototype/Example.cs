using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X3UR_Prototype {
    public class Example {
        public static void Tester() {
            byte[] byteArray = new byte[] { 2, 3, 2 };
            byte firstItem = byteArray[0];
            bool allEqual = byteArray.Skip(1)
              .All(s => Equals(firstItem, s));
        }
    }
}