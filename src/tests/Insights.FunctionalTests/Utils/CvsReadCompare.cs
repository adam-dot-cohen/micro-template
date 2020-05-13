using System.Collections.Generic;
using System.IO;

namespace Laso.Insights.FunctionalTests.Utils
{
    public class CvsReadCompare
    {
        public List<string> GetCompareStreams(StreamReader actual, StreamReader expected)
        {
            StreamReader f1 = actual;
            StreamReader f2 = expected;

                List<string> differences = new List<string>();

                int lineNumber = 0;

                while (!f1.EndOfStream)
                {
                    if (f2.EndOfStream)
                    {
                        differences.Add("Differing number of lines - f2 has less.");
                        break;
                    }

                    lineNumber++;
                    var line1 = f1.ReadLine();
                    var line2 = f2.ReadLine();

                    if (line1 != line2)
                    {
                        differences.Add(string.Format("Line {0} differs. File Expected: {1}, File Actual: {2}", lineNumber, line1,
                            line2));
                    }
                }

                if (!f2.EndOfStream)
                {
                    differences.Add("Differing number of lines - f1 has less.");
                }

                return differences;

        }
        public void GetCompare(string path1, string path2)
        {
            using (StreamReader f1 = new StreamReader(path1))
            using (StreamReader f2 = new StreamReader(path2))
            {

                var differences = new List<string>();

                int lineNumber = 0;

                while (!f1.EndOfStream)
                {
                    if (f2.EndOfStream)
                    {
                        differences.Add("Differing number of lines - f2 has less.");
                        break;
                    }

                    lineNumber++;
                    var line1 = f1.ReadLine();
                    var line2 = f2.ReadLine();

                    if (line1 != line2)
                    {
                        differences.Add(string.Format("Line {0} differs. File 1: {1}, File 2: {2}", lineNumber, line1,
                            line2));
                    }
                }

                if (!f2.EndOfStream)
                {
                    differences.Add("Differing number of lines - f1 has less.");
                }
            }
        }
    }
}
