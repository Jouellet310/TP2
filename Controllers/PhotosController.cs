using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UsersManager.Models;

namespace UsersManager.Controllers
{
    public class PhotosController : Controller
    {
        Models.UsersDBEntities DB = new Models.UsersDBEntities();

        // GET: Photos

        public void RenewPhotosSerialNumber()
        {
            HttpRuntime.Cache["imagesSerialNumber"] = Guid.NewGuid().ToString();
        }

        public string GetPhotosSerialNumber()
        {
            if (HttpRuntime.Cache["imagesSerialNumber"] == null)
            {
                RenewPhotosSerialNumber();
            }
            return (string)HttpRuntime.Cache["imagesSerialNumber"];
        }

        public void SetLocalPhotosSerialNumber()
        {
            Session["imagesSerialNumber"] = GetPhotosSerialNumber();
        }

        public bool IsPhotosUpToDate()
        {
            return ((string)Session["imagesSerialNumber"] == (string)HttpRuntime.Cache["imagesSerialNumber"]);
        }
        public ActionResult Index()
        {
            SetLocalPhotosSerialNumber();
            return View();
        }

        public ActionResult GetPhotos(bool forceRefresh = false, string sortBy = "calendar")
        {
            if (forceRefresh || !IsPhotosUpToDate())
            {
                // define the set to use based on the sorting the user wants
                IEnumerable<Photo> set;
                switch (sortBy)
                {
                    case "evaluation":
                        set = DB.Photos.OrderBy(ob => ob.Ratings).ToList();
                        break;

                    case "author":
                        set = DB.Photos.OrderBy(ob => ob.User.FirstName).ThenBy(ob => ob.User.LastName).ToList();
                        break;

                    default:
                        set = DB.Photos.OrderBy(ob => ob.CreationDate).ToList();
                        break;
                }

                SetLocalPhotosSerialNumber();
                return PartialView("_PhotosList", set);
            }
            return null;
        }
        public ActionResult Create()
        {
            ViewBag.Visibilities = SelectListItemConverter<PhotoVisibility>.Convert(DB.PhotoVisibilities.ToList());
            return View(new Photo());
        }

        [HttpPost]
        public ActionResult Create(Photo photo)
        {
            if (ModelState.IsValid)
            {
                DB.Add_Photo(photo);
                RenewPhotosSerialNumber();
                return RedirectToAction("Index");
            }
            ViewBag.Visibilities = SelectListItemConverter<PhotoVisibility>.Convert(DB.PhotoVisibilities.ToList());

            return View(photo);
        }

        public PartialViewResult PhotoForm(Photo photo)
        {
            SetLocalPhotosSerialNumber();
            return PartialView(photo);
        }

        public ActionResult Details(int id)
        {
            Photo photo = DB.Photos.Find(id);
            if (photo != null)
            {
                return View(photo);
            }
            return RedirectToAction("Index");
        }
        public ActionResult Edit(int id)
        {
            Photo photo = DB.Photos.Find(id);
            if (photo != null)
            {
                ViewBag.Visibilities = SelectListItemConverter<PhotoVisibility>.Convert(DB.PhotoVisibilities.ToList());
                return View(photo);
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult Edit(Photo photo)
        {
            if (ModelState.IsValid)
            {
                DB.Update_Photo(photo);
                RenewPhotosSerialNumber();
                return RedirectToAction("Index");
            }
            ViewBag.Visibilities = SelectListItemConverter<PhotoVisibility>.Convert(DB.PhotoVisibilities.ToList());

            return View(photo);
        }

        public ActionResult Delete(int id)
        {
            DB.Remove_Photo(id);
            RenewPhotosSerialNumber();
            return RedirectToAction("Index");
        }
    }
}