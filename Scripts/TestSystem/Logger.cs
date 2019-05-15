using UnityEngine;
using System.IO;
using System;

public class Logger
{
    protected static int s_errorReportIndex = 0;

    public static void Log(string LogData, string fileName = "LogFile.txt")
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

        if (!File.Exists(filePath))
        {
            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(filePath))
            {
                sw.WriteLine("Log file created");
                sw.WriteLine("****************");
            }
        }

        using (StreamWriter sw = File.AppendText(filePath))
        {
            sw.WriteLine(LogData);
        }
    }


    public static void CreatePathFinderErrorReport(ref WayNode[,] bluprintOfPath, Point from, Point to, ref bool[,] freeSpaceMap)
    {
        string fileName = "PathFinderReport_";
        string logPath = @"E:\Game Projects\Logs";
        string filePath = logPath + @"\" + fileName + s_errorReportIndex.ToString() + DateTime.Now.ToString("MM_dd_yyyy_HH-mm") + ".txt";

        if (!File.Exists(filePath))
        {
            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(filePath))
            {
                sw.WriteLine("Path Finder Error Report");
                sw.WriteLine("We tryed to build path from: " + from.ToString() + " to: " + to.ToString());
                sw.WriteLine("************************");
            }
        }


        string headLine = "\t*";
        for (int x = 0; x < bluprintOfPath.GetLength(0); x++)
        {
            headLine += "\t" + x.ToString() + "#"; 
        }




        using (StreamWriter sw = File.AppendText(filePath))
        {
            sw.WriteLine(headLine);
        }

        for (int y = bluprintOfPath.GetLength(1) - 1; y >= 0; y--)
        {
            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.Write("\t" + y.ToString() + "#");
            }

            string str = "";
            for (int x = 0; x < bluprintOfPath.GetLength(0); x++)
            {
                if(freeSpaceMap[x,y])
                {
                    str += "\t" + "_";
                }
                else
                {
                    str += "\t" + "X";
                }
                
            }

            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.WriteLine(str);
            }
        }

        using (StreamWriter sw = File.AppendText(filePath))
        {
            sw.WriteLine(headLine);
        }


        using (StreamWriter sw = File.AppendText(filePath))
        {
            sw.WriteLine("************************");
        }




        using (StreamWriter sw = File.AppendText(filePath))
        {
            sw.WriteLine(headLine);
        }

        for (int y = bluprintOfPath.GetLength(1) - 1; y >= 0; y--)
        {
            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.Write("\t" + y.ToString() + "#");
            }

            string str = "";
            for (int x = 0; x < bluprintOfPath.GetLength(0); x++)
            {
                WayNode currentNode = bluprintOfPath[x, y];

                switch (currentNode.previusRoadDirection)
                {
                    case Base.DIREC.DOWN:
                        str += "\t↓";
                        break;
                    case Base.DIREC.UP:
                        str += "\t↑";
                        break;
                    case Base.DIREC.LEFT:
                        str += "\t←";
                        break;
                    case Base.DIREC.RIGHT:
                        str += "\t→";
                        break;
                    case Base.DIREC.NO_DIRECTION:
                        str += "\t•";
                        break;
                }
            }

            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.WriteLine(str);
            }
        }


        using (StreamWriter sw = File.AppendText(filePath))
        {
            sw.WriteLine(headLine);
        }

        using (StreamWriter sw = File.AppendText(filePath))
        {
            sw.WriteLine("************************");
        }


        using (StreamWriter sw = File.AppendText(filePath))
        {
            sw.WriteLine(headLine);
        }
        for (int y = bluprintOfPath.GetLength(1) - 1; y >= 0; y--)
        {
            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.Write("\t" + y.ToString() + "#");
            }

            string str = "";
            for (int x = 0; x < bluprintOfPath.GetLength(0); x++)
            {
                WayNode currentNode = bluprintOfPath[x, y];

                if (currentNode.wayCost == WayNode.UNREACHABLE)
                {
                    str += "\t*";
                }
                else
                {
                    str += "\t" + currentNode.wayCost / WayNode.GROUND_COST;
                }
            }

            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.WriteLine(str);
            }
        }

        using (StreamWriter sw = File.AppendText(filePath))
        {
            sw.WriteLine(headLine);
        }

        using (StreamWriter sw = File.AppendText(filePath))
        {
            sw.WriteLine("************************");
        }

        using (StreamWriter sw = File.AppendText(filePath))
        {
            sw.WriteLine(headLine);
        }
        for (int y = bluprintOfPath.GetLength(1) - 1; y >= 0; y--)
        {
            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.Write("\t" + y.ToString() + "#");
            }

            string str = "";
            for (int x = 0; x < bluprintOfPath.GetLength(0); x++)
            {
                Point pointToPrint = new Point(x, y);

                if (pointToPrint.IsSamePoint(from))
                {
                    str += "\tS";
                }
                else if (pointToPrint.IsSamePoint(to))
                {
                    str += "\tE";
                }
                else
                {
                    str += "\t•";
                }
            }

            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.WriteLine(str);
            }
        }

        using (StreamWriter sw = File.AppendText(filePath))
        {
            sw.WriteLine(headLine);
        }

        using (StreamWriter sw = File.AppendText(filePath))
        {
            sw.WriteLine("************************");
            sw.WriteLine("Target point cost: " + bluprintOfPath[to.x, to.y].wayCost.ToString());
        }

        s_errorReportIndex++;
    }
}