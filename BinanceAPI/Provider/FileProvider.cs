using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BinanceAPI.Provider
{
    static class  FileProvider
    {
        
        static public List<string> ReadFile()
        {
            
            try
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
            catch(Exception ex)
            {
                return null;
            }

        }

        static public int WriteFile(string data)
        {

            List<string> listReadData = ReadFile();

            if (listReadData.Contains(data))
                return -1;

                File.AppendAllText(@"pairs.txt", data.ToString() + "\n");
            return 1;
        }

        static public void deleteFromFile(string data)
        {

            var Lines = File.ReadAllLines(@"pairs.txt");
            if (Lines.Contains(data))
            {
                var newLines = Lines.Where(line => !line.Contains(data));
                File.WriteAllLines(@"pairs.txt", newLines);
                
            }
            //File.WriteAllText(@"pairs.txt", sbdata.ToString());
            
        }
    }
}
