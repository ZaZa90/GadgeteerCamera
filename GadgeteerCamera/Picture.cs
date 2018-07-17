using System;
using Microsoft.SPOT;
using GT = Gadgeteer;
using System.IO;
using GHI.Utilities;
using Gadgeteer.Networking;

namespace GadgeteerCamera
{
    class Picture
    {
        private string base64code;
        GT.Picture picture;
        private GT.Modules.GHIElectronics.MulticolorLED multicolorLED2;

        public Picture(GT.Modules.GHIElectronics.MulticolorLED multicolorLED2)
        {
            // TODO: Complete member initialization
            this.multicolorLED2 = multicolorLED2;
        }

        public void setPicture(GT.Picture p) { this.picture = p; }

        public byte[] PictureToBytes()
        {
            Bitmap bitmap = picture.MakeBitmap();
            var width = bitmap.Width;
            var height = bitmap.Height;
            var size = width * height * 3 + 54;
            var bmpBuffer = new byte[size];
            GHI.Utilities.Bitmaps.ConvertToFile(bitmap, bmpBuffer);
            return bmpBuffer;
        }

       /* public void sendPictureHTTP()
        {
  
            //Convert the bitmap to a windows-compatible BMP
            base64code = "";
            Bitmap bitmap = picture.MakeBitmap();
            var width = bitmap.Width;
            var height = bitmap.Height;
            var size = width * height * 3 + 54;
            var bmpBuffer = new byte[size];
            GHI.Utilities.Bitmaps.ConvertToFile(bitmap, bmpBuffer);

            var chunkSize = 2478;
            //Create a byte array to hold the chunks prior to streaming
            var sendArray = new byte[chunkSize];
            var iterations = size / chunkSize;
            Debug.Print("Send Picture, size: " + iterations  + "*" + chunkSize );

            for (var i = 0; i < iterations; ++i)
            {
                //Copy the nect chunk
                Array.Copy(bmpBuffer, i * chunkSize, sendArray, 0, chunkSize);
                Convert.UseRFC4648Encoding = true;
                string base64codePart = Convert.ToBase64String(sendArray);
              //  base64code = base64code + base64codePart;
                client.SendPictureHTTP(i.ToString(),(iterations-1).ToString(), base64codePart);
                //Debug.Print(base64codePart);
            }

            Debug.Print("Picture Sent");
            
        }*/

    }
}
