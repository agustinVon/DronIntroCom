using System;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

namespace DJISDKDemo
{
    class VideoOperator
    {
        public void ProcessVideoToImage(byte[] bytes, int height, int width)
        {
            WriteableBitmap bitmap = new WriteableBitmap(height, width);
            _ = bitmap.FromByteArray(bytes);
            SoftwareBitmap outputBitmap = SoftwareBitmap.CreateCopyFromBuffer(
            bitmap.PixelBuffer,
            BitmapPixelFormat.Bgra8,
            bitmap.PixelWidth,
            bitmap.PixelHeight);

            byte[] data = EncodedBytes(outputBitmap, BitmapEncoder.JpegEncoderId).Result;
            string base64String = Convert.ToBase64String(data);
            PostImage(base64String);
        }

        private async Task<byte[]> EncodedBytes(SoftwareBitmap soft, Guid encoderId)
        {
            byte[] array = null;

            // First: Use an encoder to copy from SoftwareBitmap to an in-mem stream (FlushAsync)
            // Next:  Use ReadAsync on the in-mem stream to get byte[] array

            using (InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream())
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(encoderId, ms);
                encoder.SetSoftwareBitmap(soft);

                try
                {
                    await encoder.FlushAsync();
                }
                catch (Exception ex) { return new byte[0]; }

                array = new byte[ms.Size];
                _ = await ms.ReadAsync(array.AsBuffer(), (uint)ms.Size, InputStreamOptions.None);
            }
            return array;
        }

        private async void PostImage(String image)
        {
            try
            {
                // Construct the HttpClient and Uri. This endpoint is for test purposes only.
                HttpClient httpClient = new HttpClient();
                Uri uri = new Uri("https://www.mauArmaElBack.com/post");

                // Construct the JSON to post.
                HttpStringContent content = new HttpStringContent(
                    "{" + $" \"image\": \"{image} \" " + "}",
                    UnicodeEncoding.Utf8,
                    "application/json");

                // Post the JSON and wait for a response.
                HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(
                    uri,
                    content);

                // Make sure the post succeeded, and write out the response.
                _ = httpResponseMessage.EnsureSuccessStatusCode();
                string httpResponseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                Debug.WriteLine(httpResponseBody);
            }
            catch (Exception ex)
            {
                // Write out any exceptions.
                Debug.WriteLine(ex);
            }
        }
    }
}
