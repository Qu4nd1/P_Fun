using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatLib
{
    static public class Helpers
    {
        public static int? ParseNullableInt(string value)
        {
            return ((value is not "" ? Convert.ToInt32(value) : null));
        }
    }
}
