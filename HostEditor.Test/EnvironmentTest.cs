using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HostEditor.Test
{
    [TestFixture]
    public class EnvironmentTest
    {
        [TestCase]
        public void Test()
        {
            Hashtable vars = (Hashtable)Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine);

           foreach (string key in vars.Keys)
           {
               Console.WriteLine("{0} = {1}", key, vars[key]);
           }
        }
    }
}
