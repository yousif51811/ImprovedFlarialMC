using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flarial.Services
{
    static class ConvertValue
    {
        public static double ToGB(long bytes)
        {
            return Math.Round(bytes / (1024.0 * 1024 * 1024), 2);
        }

    }
}
