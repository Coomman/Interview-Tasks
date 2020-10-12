using System;
using System.IO;
using Newtonsoft.Json;

namespace ImageParser
{
    public class ImageParser : IImageParser
    {
        private class ImageInfo
        {
            public int Height { get; set; }
            public int Width { get; set; }
            public string Format { get; set; }
            public long Size { get; set; }
        }

        private static void GetPngSize(Stream stream, ImageInfo ii)
        {
            var bytes = new byte[4];
            stream.Seek(16, SeekOrigin.Begin);

            stream.Read(bytes, 0, 4);
            Array.Reverse(bytes);
            ii.Width = BitConverter.ToInt32(bytes, 0);

            stream.Read(bytes, 0, 4);
            Array.Reverse(bytes);
            ii.Height = BitConverter.ToInt32(bytes, 0);
        }
        private static void GetGifSize(Stream stream, ImageInfo ii)
        {
            var bytes = new byte[2];
            stream.Seek(6, SeekOrigin.Begin);

            stream.Read(bytes, 0, 2);
            ii.Width = BitConverter.ToInt16(bytes, 0);

            stream.Read(bytes, 0, 2);
            ii.Height = BitConverter.ToInt16(bytes, 0);
        }
        private static void GetBmpSize(Stream stream, ImageInfo ii)
        {
            var bytes = new byte[4];
            stream.Seek(18, SeekOrigin.Begin);

            stream.Read(bytes, 0, 4);
            ii.Width = BitConverter.ToInt32(bytes, 0);

            stream.Read(bytes, 0, 4);
            ii.Height = BitConverter.ToInt32(bytes, 0);
        }

        public string GetImageInfo(Stream stream)
        {
            var ii = new ImageInfo();
            switch (stream.ReadByte())
            {
                case 137:
                    GetPngSize(stream, ii);
                    ii.Format = "Png";
                    break;
                case 66:
                    GetBmpSize(stream, ii);
                    ii.Format = "Bmp";
                    break;
                case 71:
                    GetGifSize(stream, ii);
                    ii.Format = "Gif";
                    break;
                default:
                    throw new ArgumentException("Wrong image format");
            }
            ii.Size = stream.Seek(0, SeekOrigin.End);

            return JsonConvert.SerializeObject(ii, Formatting.Indented);
        }
    }
}