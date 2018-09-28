using System;
using System.Drawing;
using SystemGraphics = System.Drawing.Graphics;
using System.Runtime.InteropServices;

namespace SunRise.CyberLock.ClientSide.Utils.Graphics
{
    public class DrawHandler
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetDC(IntPtr hWnd);
        
        //For refresh desktop
        [DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        
        private System.Drawing.Font timerFont;
        private SolidBrush timerBrush;

        private Bitmap offScreenBmp;
        private SystemGraphics offScreenDC;
        private SolidBrush backBrush;

        private int canvasWidth;
        private int canvasHeight;
        private int fontSize;

        public DrawHandler(int canvasWidth, int canvasHeight, int fontSize)
        {
            this.canvasWidth = canvasWidth;
            this.canvasHeight = canvasHeight;
            this.fontSize = fontSize;

            timerFont = new System.Drawing.Font(System.Drawing.FontFamily.GenericSansSerif, this.fontSize, System.Drawing.FontStyle.Bold);
            timerBrush = new SolidBrush(System.Drawing.Color.White);
            offScreenBmp = new Bitmap(this.canvasWidth, this.canvasHeight);
            offScreenDC = SystemGraphics.FromImage(this.offScreenBmp);
            backBrush = new SolidBrush(System.Drawing.Color.Black);

        }

        public void Draw(String message)
        {
            using (SystemGraphics g = SystemGraphics.FromHdc(GetDC(IntPtr.Zero)))
            {
                offScreenDC.FillRectangle(backBrush, 0, 0, canvasWidth, canvasHeight);
                offScreenDC.DrawString(message, timerFont, timerBrush, 0, 0);

                g.DrawImage(offScreenBmp, 0, 0);
                g.Dispose();
            }
        }

        public void CleanScreen()
        {
            using (SystemGraphics g = SystemGraphics.FromHdc(GetDC(IntPtr.Zero)))
            {
                SHChangeNotify(0x8000000, 0x1000, IntPtr.Zero, IntPtr.Zero);
                g.DrawLine(new System.Drawing.Pen(System.Drawing.Brushes.Transparent, canvasHeight), 0, 0, canvasWidth, 0);
                g.Dispose();
            }
        }
    }
}
