using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DriveApiv3.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View(GoogleAPI.GetDriveFilesCustom("0B4MIqD5-aiTKY3N0QmU3UUNVaFk"));
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

      /*  public void DownloadFile(string id)
        {
            string FilePath = GoogleAPI.DownloadGoogleFile(id);

            Response.ContentType = "application/zip";
            Response.AddHeader("Content-Disposition", "attachment; filename=" + Path.GetFileName(FilePath));
            Response.WriteFile(System.Web.HttpContext.Current.Server.MapPath("~/GoogleDriveFiles/" + Path.GetFileName(FilePath)));
            Response.End();
            Response.Flush();
        }

        [HttpGet]
        public JsonResult FolderLists()
        {
            List<GoogleDriveFile> AllFolders = GoogleAPI.GetDriveFolders();
            List<DDLOptions> obj = new List<DDLOptions>();

            foreach (GoogleDriveFile EachFolder in AllFolders)
            {
                obj.Add(new DDLOptions { Id = EachFolder.Id, Name = EachFolder.Name });
            }
            return Json(obj, JsonRequestBehavior.AllowGet);
        } */
    }
}