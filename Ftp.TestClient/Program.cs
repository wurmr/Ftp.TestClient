using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
 


namespace Ftp.TestClient
{
    class Program
    {
        static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            var timer = new System.Threading.Timer(Execute, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            // Wait for input to exit.
            Console.ReadLine();
        }

        private static void Execute(object state)
        {
            try
            {
                DownloadFile();
                UploadFile();
                //ListContents();
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }

        private static void ListContents()
        {
            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://localhost/");
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential("anonymous", "a@b.com");

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            logger.Debug(reader.ReadToEnd());

            logger.Info("Directory List Complete, status {0}", response.StatusDescription);
            //Console.WriteLine("Directory List Complete, status {0}", response.StatusDescription);

            reader.Close();
            response.Close();
        }

        private static void UploadFile()
        {
            var random = new Random();
            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(String.Format("ftp://localhost/test.{0}.upload.txt", random.Next().ToString()));
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential ("anonymous","a@b.c");
            
            // Copy the contents of the file to the request stream.
            StreamReader sourceStream = new StreamReader(@"C:\ftp\test.txt");
            byte [] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
            sourceStream.Close();
            request.ContentLength = fileContents.Length;

            Stream requestStream = request.GetRequestStream();
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
    
            logger.Info("Upload File Complete, status {0}", response.StatusDescription);
    
            response.Close();
        }

        private static void DownloadFile()
        {
            // Get the object used to communicate with the server.
            var request = (FtpWebRequest)WebRequest.Create("ftp://localhost/test.txt");
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential("anonymous", "a@b.c");

            var response = (FtpWebResponse)request.GetResponse();

            var responseStream = response.GetResponseStream();
            var reader = new StreamReader(responseStream);
            Console.WriteLine(reader.ReadToEnd());

            logger.Info("Download Complete, status {0}", response.StatusDescription);

            reader.Close();
            response.Close();
        }
    }
}
