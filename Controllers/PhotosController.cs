using System;
using System.Collections.Generic;
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
        public ActionResult Index()
        {
            /*set local photo serial number*/
            return View();
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
                /*Renew photo serial number*/
                return RedirectToAction("Index");
            }
            ViewBag.Visibilities = SelectListItemConverter<PhotoVisibility>.Convert(DB.PhotoVisibilities.ToList());

            return View(photo);
        }

        public PartialViewResult PhotoForm(Photo photo)
        {
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
                /*Renew photo serial number*/
                return RedirectToAction("Index");
            }
            ViewBag.Visibilities = SelectListItemConverter<PhotoVisibility>.Convert(DB.PhotoVisibilities.ToList());

            return View(photo);
        }
    }
}