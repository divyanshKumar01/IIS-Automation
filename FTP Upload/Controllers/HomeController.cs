using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FTP_Upload.Models;
using FluentFTP;
using System.Net;
using System.Web;
using System.IO;
using System.Xml;
using System.Configuration;
namespace FTP_Upload.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            bool IsSuccess = BeginFtp();
          

            return View();
        }

        FtpClient source_client = new FtpClient("ftp://45.249.111.219/");
        FtpClient dest_client = new FtpClient("ftp://45.249.111.219/");

        public bool BeginFtp()
        {
            string ThemePath = @"C:\Windows\System32\inetsrv\config\applicationHost.Config";

            string Domain = "divyansh9803.com";
            string WebConfigPath= @"C:\Windows\System32\inetsrv\config\applicationHost.Config";


            Ftp_Init(ThemePath , WebConfigPath,Domain);
          
            return true;
        }
        public void Ftp_Init(string SourceFolder, string WebConfig, string Domain)
        {


            var IISFTPUser = "ftpUseradmin ";
            var IISFTPPassword = "ftppasswordadmin";

            //create folder

            var WebsiteFTPUser = "WebsiteFTPUser";
            var WebsiteFTPPassword = "WebsiteFTPPassword";
            FtpWebRequest reqFTP = null;
            Stream ftpStream = null;

            //string[] subDirs = _domain.DomainName.Split('/');

            string currentDir = "ftp://divyanshkumar@45.249.111.219:23/";

            //foreach (string subDir in subDirs)
            //{
            try
            {
                currentDir = currentDir + "/" + Domain;
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(currentDir);
                reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(WebsiteFTPUser, WebsiteFTPPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                ftpStream = response.GetResponseStream();
                ftpStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                //directory already exist I know that is weak but there is no way to check if a folder exist on ftp...
            }
            //}

            //hosting mappingjbd

            string filePath = "~/" + "Docs/applicationHost.Config"; //Server path
            WebClient client = new WebClient();
            client.Credentials = new NetworkCredential(IISFTPUser, IISFTPPassword);
            client.DownloadFile("ftp://ftpuser@45.249.111.219/applicationHost.config", filePath);
            // Write
            string path = filePath;

            XmlDocument xDoc = new XmlDocument();

            xDoc.Load(path);


            XmlNodeList nodeList = xDoc.GetElementsByTagName("sites");



            XmlNode newNode = xDoc.CreateElement("site");
            nodeList[0].AppendChild(newNode);
            int id = 0;
            XmlNodeList nodeAppSettings = nodeList[0].ChildNodes;
            for (int i = 0; i < nodeList[0].ChildNodes.Count - 1; i++)
            {
                if (nodeList[0].ChildNodes[i].Name.ToLower() == "site".ToLower())
                {
                    if (Convert.ToInt32(nodeList[0].ChildNodes[i].Attributes[1].Value) > id)
                    {
                        id = Convert.ToInt32(nodeList[0].ChildNodes[i].Attributes[1].Value);
                    }
                }
            }

            id = id + 1;
            int COUNT = nodeAppSettings.Count - 1;
            XmlNode node = nodeAppSettings[COUNT];
            XmlAttribute att = xDoc.CreateAttribute("name");
            att.InnerText = Domain;


            node.Attributes.Append(att);
            XmlAttribute att2 = xDoc.CreateAttribute("id");
            att2.InnerText = id.ToString();


            node.Attributes.Append(att2);


            node.InnerXml = @"<application path='/' applicationPool='DefaultAppPool'>
            <virtualDirectory path = '/' physicalPath = '" + "publish Code path" + @"' />
            </application>
            <bindings>
           <binding protocol ='http' bindingInformation= '*:80:" + Domain + @"' />
         </bindings> ";

            xDoc.Save(path); // saves the web.config file



            //Upload 
            client.Credentials = new NetworkCredential(IISFTPUser, IISFTPPassword);
            client.UploadFile("ftp://ftpuser@45.249.111.219/applicationHost.config", filePath);


        }

        public void Read_File(FtpListItem item, string SourceFolder, string DestFolder)
        {
            
        dest_client.UploadFile(@"C:\Ftp\" + item.Name, item.FullName.Replace(SourceFolder, DestFolder), FtpRemoteExists.Overwrite); //Returns Bool
         
        }

      
        public void Read_Directory(FtpListItem temp, string SourceFolder, string DestFolder)
        {

            dest_client.CreateDirectory(temp.FullName.Replace(SourceFolder, DestFolder));
            foreach (FtpListItem item in source_client.GetListing(temp.FullName))
            {
                if (item.Type == FtpFileSystemObjectType.File)
                {
                    Read_File(item, SourceFolder, DestFolder);
                }

                if (item.Type == FtpFileSystemObjectType.Directory)
                {
                    Read_Directory(item, SourceFolder, DestFolder);
                }
            }


        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
