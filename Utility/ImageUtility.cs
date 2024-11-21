using System;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Senior_Project.Utility
{
    public static class ImageUtility
    {
        /// <summary>
        /// Converts an IFormFile (uploaded file) to a byte array.
        /// </summary>
        /// <param name="file">The uploaded image file.</param>
        /// <returns>The byte array representation of the file.</returns>
        public static byte[] ConvertToByteArray(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Invalid file.");
            }

            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Converts an image file from a local path to a byte array.
        /// </summary>
        /// <param name="filePath">The full path to the image file.</param>
        /// <returns>The byte array representation of the file.</returns>
        public static byte[] ConvertToByteArray(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("Invalid file path.");
            }

            return File.ReadAllBytes(filePath);
        }

        /// <summary>
        /// Determines the MIME type based on the file extension.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The MIME type of the file.</returns>
        public static string GetContentType(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLower();

            return extension switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                _ => throw new InvalidOperationException("Unsupported file type")
            };
        }
    }
}
