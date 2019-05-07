using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIFGenerator
{
    class Program
    {
        private static byte[] GifAnimation = { 33, 255, 11, 78, 69, 84, 83, 67, 65, 80, 69, 50, 46, 48, 3, 1, 0, 0, 0 };



        static void Main(string[] args)
        {

            Console.WriteLine("Hi");

            string JpegFolder = @"";
            string GifFile = @"";
            string[] Files = Directory.GetFiles(JpegFolder, "*.png");
            MemoryStream memoryStream = new MemoryStream();
            BinaryReader binaryReader = new BinaryReader(memoryStream);
            BinaryWriter binaryWriter = new BinaryWriter(new FileStream(GifFile, FileMode.Create));
            Image.FromFile(Files[0]).Save(memoryStream, ImageFormat.Gif);
            byte[] msByteArray = memoryStream.ToArray();
            msByteArray[10] = (byte)(msByteArray[10] & 0X78); //No global color table.
            binaryWriter.Write(msByteArray, 0, 13);
            binaryWriter.Write(GifAnimation);
            WriteGifImg(msByteArray, binaryWriter);
            for (int i = 1; i < Files.Length; i++)
            {
                memoryStream.SetLength(0);

                Bitmap img = new Bitmap(Files[i]);
                //Color c = img.GetPixel(0, 0);
                //img.MakeTransparent(Color.White);
               
                
                img.Save(memoryStream, ImageFormat.Gif);
                //Image.FromFile(Files[i]).Save(memoryStream, ImageFormat.Gif);
                msByteArray = memoryStream.ToArray();
                WriteGifImg(msByteArray, binaryWriter);
            }
            binaryWriter.Write(msByteArray[msByteArray.Length - 1]);
            binaryWriter.Close();
            memoryStream.Dispose();
        }

        public static void WriteGifImg(byte[] byteArray, BinaryWriter binaryWriter)
        {
            //Adds delay
            byteArray[785] = 0;
            byteArray[786] = 0;
            byteArray[798] = (byte)(byteArray[798] | 0X87);
            binaryWriter.Write(byteArray, 781, 18);
            binaryWriter.Write(byteArray, 13, 768);
            binaryWriter.Write(byteArray, 799, byteArray.Length - 800);
        }



        public static Bitmap MakeTransparentGif(Bitmap bitmap, Color color)
        {
            byte R = color.R;
            byte G = color.G;
            byte B = color.B;
            MemoryStream fin = new MemoryStream();
            bitmap.Save(fin, System.Drawing.Imaging.ImageFormat.Gif);
            MemoryStream fout = new MemoryStream((int)fin.Length);
            int count = 0;
            byte[] buf = new byte[256];
            byte transparentIdx = 0;
            fin.Seek(0, SeekOrigin.Begin);
            //header  
            count = fin.Read(buf, 0, 13);
            if ((buf[0] != 71) || (buf[1] != 73) || (buf[2] != 70)) return null; //GIF  
            fout.Write(buf, 0, 13);
            int i = 0;
            if ((buf[10] & 0x80) > 0)
            {
                i = 1 << ((buf[10] & 7) + 1) == 256 ? 256 : 0;
            }
            for (; i != 0; i--)
            {
                fin.Read(buf, 0, 3);
                if ((buf[0] == R) && (buf[1] == G) && (buf[2] == B))
                {
                    transparentIdx = (byte)(256 - i);
                }
                fout.Write(buf, 0, 3);
            }
            bool gcePresent = false;
            while (true)
            {
                fin.Read(buf, 0, 1);
                fout.Write(buf, 0, 1);
                if (buf[0] != 0x21) break;
                fin.Read(buf, 0, 1);
                fout.Write(buf, 0, 1);
                gcePresent = (buf[0] == 0xf9);
                while (true)
                {
                    fin.Read(buf, 0, 1);
                    fout.Write(buf, 0, 1);
                    if (buf[0] == 0) break;
                    count = buf[0];
                    if (fin.Read(buf, 0, count) != count) return null;
                    if (gcePresent)
                    {
                        if (count == 4)
                        {
                            buf[0] |= 0x01;
                            buf[3] = transparentIdx;
                        }
                    }
                    fout.Write(buf, 0, count);
                }
            }
            while (count > 0)
            {
                count = fin.Read(buf, 0, 1);
                fout.Write(buf, 0, 1);
            }
            fin.Close();
            fout.Flush();
            return new Bitmap(fout);
        }

    }
}
