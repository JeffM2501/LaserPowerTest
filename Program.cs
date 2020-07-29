/*
Copyright (c) 2020 Jeffery Myers

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Xml.Linq;

namespace LaserPowerTest
{
    class Program
    {

        static int GetValue(string label, int defaultValue)
        {
            Console.WriteLine(label + " (" + defaultValue + ")");
            string t = Console.ReadLine();

            int v = defaultValue;
            if (int.TryParse(t, out v))
                return v;

            return defaultValue;
        }
        static void Main(string[] args)
        {
            StringBuilder sb = new StringBuilder();

            Console.WriteLine("Laser Power Test File Generator");
            Console.WriteLine("Copyright 2020 Jeffery Myers, MIT License");

            Console.WriteLine();
            Console.WriteLine("Enter Parameters (or enter for default)");


            int startFeed = GetValue("Starting Feedrate",50);
            int endFeed = GetValue("Ending Feedrate", 250);
            int feedIncrement = GetValue("Feedrate Increment",50);

            int minPower = GetValue("Starting Laser Power", 64);
            int maxPower = GetValue("Ending Laser Power", 255);
            int powerStep = GetValue("Laser Power Increment", 3);


            int strokeLenght = GetValue("Stroke Length", 5);
            int yOffset = GetValue("Distance Between Rows", 1);
            int xOffset = GetValue("Distance Between Strokes", 1);

            Console.WriteLine("Use M4? (Y/N)");

            bool useM4 = Console.ReadLine().ToLower() == "y";

            string LaserOnCode = useM4 ? "M4" : "M3";

            Console.WriteLine("Generating File");

            sb.AppendLine("G90");
            sb.AppendLine("G92X0Y0");

            int lineY = 0;

            int maxX = ((maxPower - minPower) / powerStep) + 1;
            maxX *= xOffset;

            SortedDictionary<int, int> feedLabels = new SortedDictionary<int, int>();

            for (int feed = startFeed; feed <= endFeed; feed += feedIncrement)
            {
                Console.WriteLine("Feed " + feed);
                sb.AppendLine("G1X0Y" + lineY + " F" + feed);

                feedLabels.Add(lineY, feed);

                for (int currX = 0; currX <= ((maxPower-minPower)/powerStep) + 1; currX++)
                {
                    int spX = currX * xOffset;

                    sb.AppendLine("G0 X" + spX + "Y" + lineY);
                    int power = minPower + (currX * powerStep);

                    sb.AppendLine(LaserOnCode + " S" + power);
                    sb.AppendLine("G1 Y" + (lineY + strokeLenght));
                  
                    sb.AppendLine("M5");
                }

                lineY += strokeLenght + yOffset;

            }

            int outlinePower = (minPower + ((maxPower - minPower) / 2));
            // outer border
            sb.AppendLine("G0X-1Y-1");
            sb.AppendLine("M4 S" + outlinePower);
            sb.AppendLine("G1 X" + (maxX + 1) + "F100");
            sb.AppendLine("G1 Y" + (lineY + 1));
            sb.AppendLine("G1 X-1");
            sb.AppendLine("G1 Y-1");
            sb.AppendLine("M5");

            int quaterX = maxX / 4;
            int halfX = maxX / 2;
            int threeQuarterX = halfX + quaterX;


            // lower lines
            sb.AppendLine("G0 X" + quaterX + "Y-1");
            sb.AppendLine("M4 S" + outlinePower);
            sb.AppendLine("G1 Y0");
            sb.AppendLine("M5");

            sb.AppendLine("G0 X" + halfX + "Y-1");
            sb.AppendLine("M4 S" + outlinePower);
            sb.AppendLine("G1 Y0");
            sb.AppendLine("M5");

            sb.AppendLine("G0 X" + threeQuarterX + "Y-1");
            sb.AppendLine("M4 S" + outlinePower);
            sb.AppendLine("G1 Y0");
            sb.AppendLine("M5");

            // upper lines
            sb.AppendLine("G0 X" + quaterX + "Y" + (lineY + 1));
            sb.AppendLine("M4 S" + outlinePower);
            sb.AppendLine("G1 Y" + lineY);
            sb.AppendLine("M5");

            sb.AppendLine("G0X" + halfX + "Y" + (lineY + 1));
            sb.AppendLine("M4 S" + outlinePower);
            sb.AppendLine("G1 Y" + lineY);
            sb.AppendLine("M5");

            sb.AppendLine("G0 X" + threeQuarterX + "Y" + (lineY + 1));
            sb.AppendLine("M4 S" + outlinePower);
            sb.AppendLine("G1 Y" + lineY);
            sb.AppendLine("M5");

            DigitalFont font = new DigitalFont();

            string drawingSpeed = "M4 S128";
            float drawingFeed = 100;

            sb.AppendLine(font.DrawStringLaser(1, 0, -2.5f, minPower.ToString(), drawingSpeed, "M5", drawingFeed).ToString());

            int quarterPower = ((maxPower - minPower) / 4) + minPower;
            sb.AppendLine(font.DrawStringLaser(1, quaterX, -2.5f, quarterPower.ToString(), drawingSpeed, "M5", drawingFeed).ToString());

            int halfPower = ((maxPower - minPower) / 2) + minPower;
            sb.AppendLine(font.DrawStringLaser(1, halfX, -2.5f, halfPower.ToString(), drawingSpeed, "M5", drawingFeed).ToString());

            int threeQuarterPower = ((maxPower - minPower) / 2) + quarterPower;
            sb.AppendLine(font.DrawStringLaser(1, threeQuarterX, -2.5f, threeQuarterPower.ToString(), drawingSpeed, "M5", drawingFeed).ToString());

            // full power
            sb.AppendLine(font.DrawStringLaser(1, maxX, -2.5f, maxPower.ToString(), drawingSpeed, "M5", drawingFeed).ToString());

            // feed labels
            foreach (var feed in feedLabels)
            {
                sb.AppendLine(font.DrawStringLaser(1, maxX + 2, feed.Key, "F" + feed.Value.ToString(), drawingSpeed, "M5", drawingFeed).ToString());
            }


            sb.AppendLine(font.DrawStringLaser(1, 0, -4, DateTime.Now.ToString(), drawingSpeed, "M5", drawingFeed).ToString());

            File.WriteAllText("laser_test.nc", sb.ToString());

            Console.WriteLine("File complete laser_test.nc");

            Console.WriteLine("Press Enter to Exit");
            Console.ReadLine();
        }
    }
}
