using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HostsEditor
{
    public class Item
    {
        public int Index { get; set; }
        public string IP { get; set; }
        public string Host { get; set; }
        public string Comments { get; set; }
        public bool IsComments { get; set; }
    }

    public class Items : List<Item>
    {
    }

    public class ItemsAdpter
    {
        private static string hostFile;

        public static string HOSTFILE
        {
            get
            {
                if (hostFile == null)
                {
                    string windir = string.Empty;
                    try
                    {
                        windir = Environment.GetEnvironmentVariable("windir");
                    }
                    catch (Exception)
                    {
                        // defaults C:\windows
                        windir = "C:\\windows";
                    }
                    string system = Path.Combine(windir, "system32");
                    hostFile = Path.Combine(system, Path.Combine("drivers", Path.Combine("etc", "hosts")));
                }

                return hostFile;
            }
        }

        private bool IsComment(string line)
        {
            return !string.IsNullOrEmpty(line) && line.StartsWith("#");
        }

        /// <summary>
        /// Load static DNS entries from the local hosts file
        /// </summary>
        /// <param name="file">the hosts file, defaults to load the local hosts file</param>
        /// <exception cref="FileNotFoundException">Not found the hosts file</exception>
        /// <returns></returns>
        public Items Load(string file = null)
        {
            //lookup the default host if the file parameter is empty or null
            if (string.IsNullOrEmpty(file))
                file = HOSTFILE;

            if (!File.Exists(file))
                throw new FileNotFoundException(string.Format("Not found {0}", file));

            //load content as string array
            string[] items = File.ReadAllLines(file);

            // init the object Items
            Items itemsObj = new Items();

            // Parse the text line to Item object and skip the comment lines
            for (int i = 0; i < items.Length; i++)
            {
                string line = items[i];

                if (line != null)
                    line = line.Trim();
                // skip the line if it's empty
                if (!string.IsNullOrEmpty(line))
                {
                    //init a Item object
                    Item itemObj = new Item();

                    // set index
                    itemObj.Index = i;

                    // It's comments if begins with "#"
                    if (this.IsComment(line))
                    {
                        itemObj.Comments = line;//.Substring(1,line.Length-1);
                        itemObj.IsComments = true;
                        // go to lookup next
                        itemsObj.Add(itemObj);
                        continue;
                    }

                    // don't use string.Split here because the comment probaly contains white space
                    int index = line.IndexOf(" ", StringComparison.Ordinal);

                    // it's an invliad line
                    if (index < 0) continue;

                    // set ip
                    itemObj.IP = line.Substring(0, index);

                    // lookup host name
                    line = line.Substring(index + 1, line.Length - index - 1).Trim();
                    index = line.IndexOf(" ", StringComparison.Ordinal);

                    // whole line is just the host name
                    if (index < 0)
                    {
                        // set host name
                        itemObj.Host = line;
                        itemsObj.Add(itemObj);
                        // there is comments so go to lookup next
                        continue;
                    }

                    // set host name
                    itemObj.Host = line.Substring(0, index);

                    // take the last part as commnets
                    string comments = line.Substring(index + 1, line.Length - index - 1).Trim();
                    //if (comments.StartsWith("#"))
                    //    comments = comments.Substring(1, comments.Length - 1);

                    itemObj.Comments = comments;

                    // add the item object to the items list
                    itemsObj.Add(itemObj);
                }
            }

            return itemsObj;
        }

        public void Save(Items itemsObj, string file = null)
        {
            if (string.IsNullOrEmpty(file))
                file = HOSTFILE;

            // build output content;
            StringBuilder sb = new StringBuilder();
            foreach (Item itemObj in itemsObj)
            {
                if (itemObj.IsComments)
                    // sb.AppendLine(string.Format("#{0}",itemObj.Comments));
                    sb.AppendLine(itemObj.Comments);
                else
                {
                    string line = string.Format("{0} {1}", itemObj.IP, itemObj.Host);
                    if (!string.IsNullOrEmpty(itemObj.Comments))
                    {
                        if (!itemObj.Comments.StartsWith("#"))
                            line += string.Format(" #{0}", itemObj.Comments);
                        else
                            line += string.Format(" {0}", itemObj.Comments);
                    }

                    sb.AppendLine(line);
                }
            }

            File.WriteAllText(file, sb.ToString());
        }
    }
}