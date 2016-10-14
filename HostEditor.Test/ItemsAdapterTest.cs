using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using HostsEditor;
using System.IO;
using System.Diagnostics;

namespace HostEditor.Test
{
    [TestFixture]
    public class ItemsAdapterTest
    {
        [TestCase]
        public void LoadTest()
        {
            ItemsAdpter adpter = new ItemsAdpter();
            Items itemsObj= adpter.Load();
            string temp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp_host.txt");
            adpter.Save(itemsObj, temp);
            Process.Start(temp);
            //File.Delete(temp);
        }
    }
}
