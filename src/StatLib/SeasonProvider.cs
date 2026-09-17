using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace StatLib
{
    internal class SeasonProvider
    {
        private string GetResourceText(string fileName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(fileName);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
