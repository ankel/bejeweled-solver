using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BejeweledSolver
{
    class ARBG
    {
        public int A;
        public int R;
        public int B;
        public int G;
        public ARBG(int A, int R, int B, int G)
        {
            this.A = A;
            this.R = R;
            this.B = B;
            this.G = G;
        }
        public ARBG(int color)
        {
            A = (color >> 24) & 0x000000ff;
            R = (color & 0x00ff0000) >> 16;
            B = (color & 0x0000ff00) >> 8;
            G = (color & 0x000000ff);
        }
        public ARBG(System.Drawing.Color color)
        {
            this.A = color.A;
            this.R = color.R;
            this.B = color.B;
            this.G = color.G;
        }
        public static ARBG FromInt(int color)
        {
            int A = (color >> 24) & 0x000000ff,
                R = (color & 0x00ff0000) >> 16,
                B = (color & 0x0000ff00) >> 8,
                G = (color & 0x000000ff);
            return new ARBG(A, R, B, G);
        }
        public bool Similar(ARBG anotherColor)
        {
            if (Math.Abs(this.A - anotherColor.A) > 25)
                return false;
            if (Math.Abs(this.R - anotherColor.R) > 25)
                return false;
            if (Math.Abs(this.B - anotherColor.B) > 25)
                return false;
            if (Math.Abs(this.G - anotherColor.G) > 25)
                return false;
            return true;
        }
        public int ToInt()
        {
            return A << 24 + R << 16 + B << 8 + G;
        }
        public static ARBG Average(ARBG[] colors)
        {
            int avgA = 0, avgR = 0, avgB = 0, avgG = 0;
            foreach (ARBG color in colors)
            {
                avgA += color.A;
                avgR += color.R;
                avgB += color.B;
                avgG += color.G;
            }
            return new ARBG(avgA/colors.Length, avgR/colors.Length, avgB/colors.Length, avgG/colors.Length);
        }
    }
}
