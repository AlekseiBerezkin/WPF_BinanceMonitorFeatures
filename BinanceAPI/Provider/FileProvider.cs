using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BinanceAPI.Provider
{
    class FileProvider
    {
        public List<string> ReadFile()
        {
            List<string> data = new List<string>();
            using (StreamReader reader = new StreamReader("pairs.txt"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    data.Add(line);
                }
                    
            }
            return data;
        }

        public void WriteFile(List<string> data)
        {
            StringBuilder dataForWrite = new StringBuilder();

            foreach(string str in data)
            {
                dataForWrite.Append(str+"\n");
            }
            File.WriteAllText(@"pairs.txt", dataForWrite.ToString());
        }
    }
}
