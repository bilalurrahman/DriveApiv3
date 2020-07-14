using DriveApiv3.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace DriveApiv3
{
    public class GoogleAPI
    {
        public static string[] Scopes = { Google.Apis.Drive.v3.DriveService.Scope.DriveReadonly, "email" };
        public static Google.Apis.Drive.v3.DriveService GetService()
        {
            //get Credentials from client_secret.json file 
            var CSPath = System.Web.Hosting.HostingEnvironment.MapPath("~/");
            UserCredential credential;
            using (var stream = new FileStream(Path.Combine(CSPath, "client_secret.json"), FileMode.Open, FileAccess.Read))
            {
               // String FolderPath = @"D:\";
                String FilePath = Path.Combine(CSPath, "DriveServiceCredentials.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(FilePath, true)).Result;
            }

            //create Drive API service.
            Google.Apis.Drive.v3.DriveService service = new Google.Apis.Drive.v3.DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "GoogleDriveRestAPI-v3",
            });
            return service;
        }

        public static Google.Apis.Drive.v2.DriveService GetService_v2()
        {
            UserCredential credential;
            var CSPath = System.Web.Hosting.HostingEnvironment.MapPath("~/");

            using (var stream = new FileStream(Path.Combine(CSPath, "client_secret.json"), FileMode.Open, FileAccess.Read))
            {

                String FilePath = Path.Combine(CSPath, "DriveServiceCredentials.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(FilePath, true)).Result;
            }

            //Create Drive API service.    
            Google.Apis.Drive.v2.DriveService service = new Google.Apis.Drive.v2.DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "GoogleDriveRestAPI-v2",
            });
            return service;
        }

        //get all files from Google Drive.    
        public static List<GoogleDriveFiles> GetDriveFiles()
        {
            Google.Apis.Drive.v3.DriveService service = GetService();
           

            // Define parameters of request.    
            Google.Apis.Drive.v3.FilesResource.ListRequest FileListRequest = service.Files.List();
            // for getting folders only.    
            //FileListRequest.Q = "mimeType='application/vnd.google-apps.folder'";    
            FileListRequest.Fields = "nextPageToken, files(*)";

            // List files.    
            IList<Google.Apis.Drive.v3.Data.File> files = FileListRequest.Execute().Files;
            List<GoogleDriveFiles> FileList = new List<GoogleDriveFiles>();


            // For getting only folders    
            // files = files.Where(x => x.MimeType == "application/vnd.google-apps.folder").ToList();    


            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    GoogleDriveFiles File = new GoogleDriveFiles
                    {
                        Id = file.Id,
                        Name = file.Name,
                        Size = file.Size,
                        Version = file.Version,
                        CreatedTime = file.CreatedTime,
                       
                    };
                    FileList.Add(File);
                }
            }
            return FileList;
        }


        public static List<GoogleDriveFiles> GetDriveFilesCustom(string id)
        {
            Google.Apis.Drive.v3.DriveService service = GetService();


            // Define parameters of request.    
            Google.Apis.Drive.v3.FilesResource.ListRequest FileListRequest = service.Files.List();
            // for getting folders only.    
            FileListRequest.Q = "'"+ id + "' in parents and "+"mimeType!='application/vnd.google-apps.folder'"; //get the files under folder id
           
            FileListRequest.Fields = "nextPageToken, files(*)";

            // List files.    
            IList<Google.Apis.Drive.v3.Data.File> files = FileListRequest.Execute().Files;
            List<GoogleDriveFiles> FileList = new List<GoogleDriveFiles>();


            // For getting only folders    
           //  files = files.Where(x => x.MimeType != "application/vnd.google-apps.folder").ToList();    
            

            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    GoogleDriveFiles File = new GoogleDriveFiles
                    {
                        Id = file.Id,
                        Name = file.Name,
                        Size = file.Size,
                        Version = file.Version,
                        CreatedTime = file.CreatedTime,

                    };
                    FileList.Add(File);
                }
            }
            return FileList;
        }

        public static List<GoogleDriveFiles> GetContainsInFolder(String folderId)
        {
            List<string> ChildList = new List<string>();
            Google.Apis.Drive.v2.DriveService ServiceV2 = GetService_v2();
            ChildrenResource.ListRequest ChildrenIDsRequest = ServiceV2.Children.List(folderId);

            // for getting only folders    
          //  ChildrenIDsRequest.Q = "mimeType='application/vnd.google-apps.folder'";    
            do
            {
                var children = ChildrenIDsRequest.Execute();

                if (children.Items != null && children.Items.Count > 0)
                {
                    foreach (var file in children.Items)
                    {
                        ChildList.Add(file.Id);
                    }
                }
                ChildrenIDsRequest.PageToken = children.NextPageToken;

            } while (!String.IsNullOrEmpty(ChildrenIDsRequest.PageToken));

            //Get All File List    
            List<GoogleDriveFiles> AllFileList = GetDriveFiles();
            List<GoogleDriveFiles> Filter_FileList = new List<GoogleDriveFiles>();

            foreach (string Id in ChildList)
            {
                Filter_FileList.Add(AllFileList.Where(x => x.Id == Id).FirstOrDefault());
            }
            return Filter_FileList;
        }

        public static List<GoogleDriveFiles> GetContainsInFolderCustom(String folderId)
        {
            List<string> ChildList = new List<string>();
            Google.Apis.Drive.v2.DriveService ServiceV2 = GetService_v2();
            ChildrenResource.ListRequest ChildrenIDsRequest = ServiceV2.Children.List(folderId);

            // for getting only folders    
             ChildrenIDsRequest.Q = "mimeType!='application/vnd.google-apps.folder'";    //file catched by usiing ! sign
            ChildrenIDsRequest.Fields = "files(id,name)";
            do
            {
                var children = ChildrenIDsRequest.Execute();

                if (children.Items != null && children.Items.Count > 0)
                {
                    foreach (var file in children.Items)
                    {
                        ChildList.Add(file.Id);
                    }
                }
                ChildrenIDsRequest.PageToken = children.NextPageToken;

            } while (!String.IsNullOrEmpty(ChildrenIDsRequest.PageToken));

            //Get All File List    
          //  List<GoogleDriveFiles> AllFileList = GetDriveFiles();
            List<GoogleDriveFiles> Filter_FileList = new List<GoogleDriveFiles>();


            foreach (string Id in ChildList)
            {
                GoogleDriveFiles File = new GoogleDriveFiles
                {
                    Id = Id,
                    Name = "",
                    Size = 0,
                    Version = 0,
                    CreatedTime = null

                };
                Filter_FileList.Add(File);
            }
            return Filter_FileList;
        }


        //Download file from Google Drive by fileId.    
        public static string DownloadGoogleFile(string fileId)
        {
            Google.Apis.Drive.v3.DriveService service = GetService();

            string FolderPath = System.Web.HttpContext.Current.Server.MapPath("/GoogleDriveFiles/");
            Google.Apis.Drive.v3.FilesResource.GetRequest request = service.Files.Get(fileId);

            string FileName = request.Execute().Name;
            string FilePath = System.IO.Path.Combine(FolderPath, FileName);

            MemoryStream stream1 = new MemoryStream();

            // Add a handler which will be notified on progress changes.    
            // It will notify on each chunk download and when the    
            // download is completed or failed.    
            request.MediaDownloader.ProgressChanged += (Google.Apis.Download.IDownloadProgress progress) =>
            {
                switch (progress.Status)
                {
                    case DownloadStatus.Downloading:
                        {
                            Console.WriteLine(progress.BytesDownloaded);
                            break;
                        }
                    case DownloadStatus.Completed:
                        {
                            Console.WriteLine("Download complete.");
                            SaveStream(stream1, FilePath);
                            break;
                        }
                    case DownloadStatus.Failed:
                        {
                            Console.WriteLine("Download failed.");
                            break;
                        }
                }
            };
            request.Download(stream1);
            return FilePath;
        }

        private static void SaveStream(MemoryStream stream, string FilePath)
        {
            using (System.IO.FileStream file = new FileStream(FilePath, FileMode.Create, FileAccess.ReadWrite))
            {
                stream.WriteTo(file);
            }
        }
    }
}