using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace BundestagMine.Utility
{
    public class ImageHelper
    {
        /// <summary>
        /// Writes a base64 encoded image to disc
        /// </summary>
        /// <param name="base64"></param>
        /// <param name="path"></param>
        public static void WriteBase64ToDisc(string base64, string path)
        {
            var bytes = Convert.FromBase64String(base64);
            using (var imageFile = new FileStream(path, FileMode.Create))
            {
                imageFile.Write(bytes, 0, bytes.Length);
                imageFile.Flush();
            }
        }

    }
}
