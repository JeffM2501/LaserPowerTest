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
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace LaserPowerTest
{
    public class OutlineFont
    {
        public class Glyph
        {
            public char Letter = char.MinValue;
            public class Move
            {
                public bool On = false;
                public float[] EP = new float[2];
            }

            public List<Move> Moves = new List<Move>();

            public void Add(float x, float y, bool on)
            {
                Moves.Add(new Move() { On = on, EP = new float[] { x, y } });
            }
        }

        public Dictionary<char, Glyph> Glyphs = new Dictionary<char, Glyph>();

        protected Glyph GlyphForCharacter(char c)
        {
            Glyph glyph = null;
            if (Glyphs.ContainsKey(c))
                glyph = Glyphs[c];
            else if (Glyphs.ContainsKey(c.ToString().ToUpperInvariant()[0]))
                glyph = Glyphs[c.ToString().ToUpperInvariant()[0]];

            if (glyph == null && Glyphs.ContainsKey(' '))
                glyph = Glyphs[' '];

            return glyph;
        }

        public StringBuilder DrawStringLaser(float scale, float startPointX, float startPointY, string text, string onCode, string offCode, float feedrate)
        {
            StringBuilder gcode = new StringBuilder();

            float x = startPointX;
            float y = startPointY;

            string floatFormat = "N3";

            bool feedStatus = false;

            gcode.AppendLine("G1 X" + x.ToString(floatFormat) + "Y" + y.ToString(floatFormat) + " F" + feedrate.ToString());

            foreach (char c in text)
            {
                Glyph glyph = GlyphForCharacter(c);
                if (glyph == null)
                    continue;

                float glyphStartX = x;
                float glyphStartY = y;

                bool onSatus = false;
                foreach (var move in glyph.Moves)
                {
                    if (move.On != onSatus)
                    {
                        if (move.On)
                            gcode.AppendLine(onCode);
                        else
                            gcode.AppendLine(offCode);
                    }
                    onSatus = move.On;
                    bool feed = move.On;

                    float epX = glyphStartX + move.EP[0];
                    float epY = glyphStartY + move.EP[1];

                    if (x != epX || y != epY)
                    {
                        if (feedStatus != feed)
                        {
                            if (!feed)
                                gcode.Append("G0 ");
                            else
                                gcode.Append("G1 ");
                        }

                        feedStatus = feed;

                        if (x != epX)
                        {
                            gcode.Append("X" + epX.ToString(floatFormat));
                        }
                        if (y != epY)
                        {
                            gcode.Append("Y" + epY.ToString(floatFormat));
                        }

                        x = epX;
                        y = epY;
                        gcode.AppendLine();
                    }
                }
            }
            gcode.Append(offCode);
            gcode.Append("G0 X" + startPointX.ToString(floatFormat) + "Y" + startPointY.ToString(floatFormat));

            return gcode;
        }
    }

    public class DigitalFont : OutlineFont
    {
        public static float Unit = 0.125f;
        public static float Width = 0.5f;
        public static float Height = 1.0f;
        public static float HalfHeight = 0.5f;
        public static float HalfWidth = 0.25f;

        public DigitalFont()
        {
            Glyphs.Add('0', Zero());
            Glyphs.Add('1', One());
            Glyphs.Add('2', Two());
            Glyphs.Add('3', Three());
            Glyphs.Add('4', Four());
            Glyphs.Add('5', Five());
            Glyphs.Add('6', Six());
            Glyphs.Add('7', Seven());
            Glyphs.Add('8', Eight());
            Glyphs.Add('9', Nine());
            Glyphs.Add(':', Colon());
            Glyphs.Add('-', Dash());
            Glyphs.Add('/', Slash());
            Glyphs.Add('\\', Backslash());

            Glyphs.Add(' ', Space());
            Glyphs.Add('\t', Tab());
        }

        protected Glyph Slash()
        {
            Glyph glyph = new Glyph();

            glyph.Add(0, 0, false);
            glyph.Add(Width, Height, true);

            glyph.Add(Width + Unit, 0, false);
            return glyph;
        }

        protected Glyph Backslash()
        {
            Glyph glyph = new Glyph();

            glyph.Add(Width, 0, false);
            glyph.Add(0, Height, true);

            glyph.Add(Width + Unit, 0, false);
            return glyph;
        }

        protected Glyph Dash()
        {
            Glyph glyph = new Glyph();

            glyph.Add(0, HalfHeight, false);
            glyph.Add(Width, HalfHeight, true);

            glyph.Add(Width + Unit, 0, false);
            return glyph;
        }

        protected Glyph Colon()
        {
            Glyph glyph = new Glyph();

            glyph.Add(HalfWidth, Unit, false);
            glyph.Add(HalfWidth, Unit * 3, true);

            glyph.Add(HalfWidth, Unit * 5, false);
            glyph.Add(HalfWidth, Unit * 7, true);

            glyph.Add(Width + Unit, 0, false);
            return glyph;
        }

        protected Glyph Space()
        {
            Glyph glyph = new Glyph();
            glyph.Add(Width, 0, false);
            return glyph;
        }

        protected Glyph Tab()
        {
            Glyph glyph = new Glyph();
            glyph.Add((Width + Unit)*2, 0, false);
            return glyph;
        }

        protected Glyph Zero()
        {
            Glyph glyph = new Glyph();

            glyph.Add(0, Unit, false);
            glyph.Add(0, Height - Unit, true);
            glyph.Add(Unit, Height, true);
            glyph.Add(Width - Unit, 1, true);
            glyph.Add(Width, Height - Unit, true);
            glyph.Add(Width, Unit, true);
            glyph.Add(Width - Unit, 0, true);
            glyph.Add(Unit, 0, true);
            glyph.Add(Width - Unit, Height, true);

            glyph.Add(Width + Unit, 0, false);
            return glyph;
        }

        protected Glyph One()
        {
            Glyph glyph = new Glyph();

            glyph.Add(Width, 0, false);
            glyph.Add(Width, Height , true);

            glyph.Add(Width + Unit, 0, false);
            return glyph;
        }

        protected Glyph Two()
        {
            Glyph glyph = new Glyph();

            glyph.Add(0, Height, false);
            glyph.Add(Width, Height, true);
            glyph.Add(Width, HalfHeight, true);
            glyph.Add(0, HalfHeight, true);
            glyph.Add(0, 0, true);
            glyph.Add(Width, 0, true);

            glyph.Add(Width + Unit, 0, false);
            return glyph;
        }

        protected Glyph Three()
        {
            Glyph glyph = new Glyph();

            glyph.Add(0, Height, false);
            glyph.Add(Width, Height, true);
            glyph.Add(Width, 0, true);
            glyph.Add(0, 0, true);
            glyph.Add(0, HalfHeight, false);
            glyph.Add(Width, HalfHeight, true);

            glyph.Add(Width + Unit, 0, false);
            return glyph;
        }

        protected Glyph Four()
        {
            Glyph glyph = new Glyph();

            glyph.Add(0, Height, false);
            glyph.Add(0, HalfHeight, true);
            glyph.Add(Width, HalfHeight, true);
            glyph.Add(Width, Height, false);
            glyph.Add(Width, 0, true);

            glyph.Add(Width + Unit, 0, false);
            return glyph;
        }

        protected Glyph Five()
        {
            Glyph glyph = new Glyph();

            glyph.Add(Width, Height, false);
            glyph.Add(0, Height, true);
            glyph.Add(0, HalfHeight, true);
            glyph.Add(Width, HalfHeight, true);
            glyph.Add(Width, 0, true);
            glyph.Add(0, 0, true);

            glyph.Add(Width + Unit, 0, false);
            return glyph;
        }

        protected Glyph Six()
        {
            Glyph glyph = new Glyph();

            glyph.Add(Width, Height, false);
            glyph.Add(0, Height, true);
            glyph.Add(0, HalfHeight, true);
            glyph.Add(Width, HalfHeight, true);
            glyph.Add(Width, 0, true);
            glyph.Add(0, 0, true);
            glyph.Add(0, HalfHeight, true);

            glyph.Add(Width + Unit, 0, false);
            return glyph;
        }

        protected Glyph Seven()
        {
            Glyph glyph = new Glyph();

            glyph.Add(0, Height, false);
            glyph.Add(Width, Height, true);
            glyph.Add(Width, 0, true);

            glyph.Add(Width + Unit, 0, false);
            return glyph;
        }

        protected Glyph Eight()
        {
            Glyph glyph = new Glyph();

            glyph.Add(0, Height, false);
            glyph.Add(Width, Height, true);
            glyph.Add(Width, 0, true);
            glyph.Add(0, 0, true);
            glyph.Add(0, Height, true);
            glyph.Add(0, HalfHeight, false);
            glyph.Add(Width, HalfHeight, true);

            glyph.Add(Width + Unit, 0, false);
            return glyph;
        }

        protected Glyph Nine()
        {
            Glyph glyph = new Glyph();

            glyph.Add(0, 0, false);
            glyph.Add(Width, 0, true);
            glyph.Add(Width, Height, true);
            glyph.Add(0, Height, true);
            glyph.Add(0, HalfHeight, true);
            glyph.Add(Width, HalfHeight, true);

            glyph.Add(Width + Unit, 0, false);
            return glyph;
        }
    }

}
