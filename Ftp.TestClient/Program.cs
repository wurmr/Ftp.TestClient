﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using System.Configuration;

namespace Ftp.TestClient
{
    class Program
    {
        static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            var testFrequencyInSeconds = int.Parse(ConfigurationManager.AppSettings["TestFrequencyInSeconds"]);
            var timer = new System.Threading.Timer(Execute, null, TimeSpan.Zero, TimeSpan.FromSeconds(testFrequencyInSeconds));

            // Wait forever, the timer will handle everything else.
            (new ManualResetEventSlim()).Wait();
        }

        private static void Execute(object state)
        {
            try
            {
                var sourceFileName = ConfigurationManager.AppSettings["SourceFile"];
                var file = DownloadFile(sourceFileName);
                if (file.Length == 0) throw new ApplicationException("Downloaded File Size is Zero");

                var fileName = String.Format("{0}.{1}.txt", sourceFileName.Substring(0, sourceFileName.IndexOf('.')), DateTime.UtcNow.Ticks);
                UploadFile(fileName, file);
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }

        private static void UploadFile(string fileName, byte [] fileContents)
        {
            logger.Info("Uploading file named {0}", fileName);

            var serverAddress = ConfigurationManager.AppSettings["FtpServerAddress"];
            var uploadDirectory = ConfigurationManager.AppSettings["UploadDirectory"];

            // Get the object used to communicate with the server.
            var request = (FtpWebRequest)WebRequest.Create(String.Format("{0}{1}/{2}", serverAddress, uploadDirectory, fileName));
            request.Method = WebRequestMethods.Ftp.UploadFile;          
            request.KeepAlive = false;
            request.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["FtpUserName"], ConfigurationManager.AppSettings["FtpPassword"]);
            
            request.ContentLength = fileContents.Length;

            Stream requestStream = request.GetRequestStream();
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
    
            logger.Info("Upload File Complete, status {0}", response.StatusDescription);
            response.Close();
        }

        private static byte [] DownloadFile(string fileName)
        {
            logger.Info("Downloading file named {0}", fileName);
            var serverAddress = ConfigurationManager.AppSettings["FtpServerAddress"];
            var uploadDirectory = ConfigurationManager.AppSettings["DownloadDirectory"];

            // Get the object used to communicate with the server.
            var request = (FtpWebRequest)WebRequest.Create(String.Format("{0}{1}/{2}", serverAddress, uploadDirectory, fileName));
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.KeepAlive = false;
            request.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["FtpUserName"], ConfigurationManager.AppSettings["FtpPassword"]);
            var response = (FtpWebResponse)request.GetResponse();

            var responseStream = response.GetResponseStream();
            var memoryStream = new MemoryStream();
            responseStream.CopyTo(memoryStream);

            logger.Info("Download Complete, status {0}", response.StatusDescription);

            response.Close();
            var fileContents = memoryStream.ToArray();
            memoryStream.Close();
            return fileContents;
        }
    }
}
