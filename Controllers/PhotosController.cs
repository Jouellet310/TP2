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
            HttpRuntime.Cache["photosSerialNumber"] = Guid.NewGuid().ToString();
        }

        public string GetPhotosSerialNumber()
        {
            if (HttpRuntime.Cache["photosSerialNumber"] == null)
            {
                RenewPhotosSerialNumber();
            }
            return (string)HttpRuntime.Cache["photosSerialNumber"];
        }

        public void SetLocalPhotosSerialNumber()
        {
            Session["photosSerialNumber"] = GetPhotosSerialNumber();
        }

        public bool IsPhotosUpToDate()
        {
            return ((string)Session["photosSerialNumber"] == (string)HttpRuntime.Cache["photosSerialNumber"]);
        }
        public ActionResult Index()
        {
            SetLocalPhotosSerialNumber();
            return View();
        }

        public ActionResult GetPhotos(bool forceRefresh = false, string sortBy = "calendar", char inverted = 'n')
        {
            if (forceRefresh || !IsPhotosUpToDate())
            {
                // define the data set to use based on the sorting the user wants
                IEnumerable<Photo> set;
                switch (sortBy)
                {
                    case "evaluation":
                        set = DB.Photos.OrderBy(ob => ob.Ratings);
                        break;

                    case "author":
                        set = DB.Photos.OrderBy(ob => ob.User.FirstName).ThenBy(ob => ob.User.LastName);
                        break;

                    default:
                        set = DB.Photos.OrderByDescending(ob => ob.CreationDate);
                        break;
                }

                if (inverted == 'y')
                    set = set.Reverse();

                SetLocalPhotosSerialNumber();
                return PartialView("_PhotosList", set.ToList());
            }
            return null;
        }

        public ActionResult GetPhotoDetails(int photoId, bool forceRefresh = false)
        {  
                var photo = DB.Photos.Find(photoId);
                SetLocalPhotosSerialNumber();
                return View(photo);
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
            return PartialView("PhotoForm", photo);
        }

        [HttpGet]
        public ActionResult Details(int? photoId)
        {
            Session["RatingFieldSortDir"] = true;
            Photo photo = DB.Photos.Find(photoId);
            if (photo != null)
            {
                return View("GetPhotoDetails", photo);
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

        public ActionResult UpdateCurrentUserRating(int photoId, int rating, string comment)
        {
            Photo photo = DB.Photos.Find(photoId);

            PhotoRating photoRating = new PhotoRating {
                PhotoId = photoId,
                UserId = OnlineUsers.GetSessionUser().Id,
                Rating = rating,
                Comment = comment,
                CreationDate = DateTime.Now
            };

            DB.AddPhotoRating(photoRating);

            DB.Update_Photo_Ratings();
            return View();
        }
        public ActionResult SortRatingsBy(string fieldToSort)
        {
            Session["RatingFieldToSort"] = fieldToSort;
            Session["RatingFieldSortDir"] = !(bool)Session["RatingFieldToSort"];
            return View();
        }
    }
}