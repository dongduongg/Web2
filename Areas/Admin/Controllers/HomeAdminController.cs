using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLBN.Models;
using QLBN.Models.Authentication;
using X.PagedList;
using X.PagedList.Mvc.Core;
using Microsoft.AspNetCore.WebUtilities;

namespace QLBN.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin")]
    //[Route("admin/homeadmin")]
    
    //[Authorize(Roles ="admin")]
    public class HomeAdminController : Controller
    {
        QLBNContext db = new QLBNContext();
        
        public bool Access()
        {
            if (HttpContext.Session.GetString("Role") == "Admin")
                return true;
            else return false;
        }
        //[Route("")]
        [Route("index")] // co the se sua o day
        public IActionResult Index()
        {
            //if (HttpContext.Session.GetString("Role")=="Admin")
            //if(Access()) return View();
            //else return View("~/Views/Home/Index.cshtml");
            if (Access()==false) return View("~/Views/Home/Index.cshtml");
            return View();
        }
        [Route("danhmucbacsi")]
        public IActionResult DanhMucBacSi(int? page)
        {
            if (!Access()) return View("~/Views/Home/Index.cshtml");
           
            int pageSize = 8;
            //int pageNumber = pageSize == null || pageSize < 0 ? 1 : page.Value;
            int pageNumber = page ?? 1;
            var lstsanpham = db.Doctors.AsNoTracking().OrderBy(x => x.FacultyId);
            PagedList<Doctor> lst= new PagedList<Doctor>(lstsanpham,pageNumber,pageSize);

           // var lstBacSi= db.Doctors.ToList();
            return View(lst);
        }
        [Route("ThemBacSiMoi")]
        [HttpGet]
        public IActionResult ThemBacSiMoi()
        {
            if (!Access()) return View("~/Views/Home/Index.cshtml");
            ViewBag.FacultyId = new SelectList(db.Faculties.ToList(), "FacultyId", "FacultyName");
            ViewBag.RoomId = new SelectList(db.Rooms.ToList(), "RoomId", "RoomId" );
            ViewBag.ServiceId = new SelectList(db.Services.ToList(), "ServiceId", "ServiceName");

            return View();
        }
        [Route("ThemBacSiMoi")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThemBacSiMoi(Doctor doctor)
        {
            if (!Access()) return View("~/Views/Home/Index.cshtml");
            if (ModelState.IsValid)
            {
                db.Doctors.Add(doctor);
                db.SaveChanges();
                return RedirectToAction("DanhMucBacSi");
            }
            return View(doctor);
        }
       [Route("SuaBacSi")]
        [HttpGet]
        public IActionResult SuaBacSi(string doctorId)
        {
            if (!Access()) return View("~/Views/Home/Index.cshtml");
            ViewBag.FacultyId = new SelectList(db.Faculties.ToList(), "FacultyId", "FacultyName");
            ViewBag.RoomId = new SelectList(db.Rooms.ToList(), "RoomId", "RoomId");
            ViewBag.ServiceId = new SelectList(db.Services.ToList(), "ServiceId", "ServiceName");
            var BacSi = db.Doctors.Find(Convert.ToInt32(doctorId));
            return View(BacSi);
        }
        [Route("SuaBacSi")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SuaBacSi(Doctor doctor)
        {
            if (!Access()) return View("~/Views/Home/Index.cshtml");
            if (ModelState.IsValid)
            {
                db.Entry(doctor).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("DanhMucBacSi","HomeAdmin");
            }
            return View(doctor);
        }
        [Route("XoaBacSi")]
        [HttpGet]
        public IActionResult XoaBacSi(string doctorId)
        {

            if (!Access()) return View("~/Views/Home/Index.cshtml");
            var lichhen = db.Appointments
            .Where(x => x.DoctorId == Convert.ToInt32(doctorId) && x.AppointmentDate > DateTime.Now)
            .ToList();
            if (lichhen.Count()>0)
            {
                TempData["Message"] = " Không thể xóa vì còn lịch hẹn";
                return RedirectToAction("DanhMucBacSi", "HomeAdmin");
            }
            db.Remove(db.Doctors.Find(Convert.ToInt32(doctorId)));
            db.SaveChanges();
            TempData["Message"] = "Bác sĩ đã được xóa";
            return RedirectToAction("DanhMucBacSi", "HomeAdmin");
        }
        [Route("DanhMucLichHen")]
        public IActionResult DanhMucLichHen(int? page)
        {
            // Logic kiểm tra quyền truy cập ở đây (ví dụ: sử dụng AuthorizationAttribute)
            // ...
            if (!Access()) return View("~/Views/Home/Index.cshtml");
            int pageSize = 8;
            int pageNumber = page ?? 1;

            // Get appointments ordered by date (assuming 'AppointmentDate' exists in Appointment model)
            var appointments = db.Appointments.AsNoTracking().OrderBy(x => x.AppointmentId);
            PagedList<Appointment> lst = new PagedList<Appointment>(appointments, pageNumber, pageSize);
            return View(lst);
        }

        [Route("ThemLichHenMoi")]
        [HttpGet]
        public IActionResult ThemLichHenMoi()
        {
            var majors = new List<SelectListItem>();//cách 1
            foreach (var item in db.Services)
            {
                majors.Add(new SelectListItem { Text = item.ServiceId.ToString(), Value = item.ServiceId.ToString() });
            }
            if (!Access()) return View("~/Views/Home/Index.cshtml");
            ViewBag.ServiceId = majors;
            ViewBag.ServiceId = new SelectList(db.Services.ToList(), "ServiceId", "ServiceName");

            return View();
        }
        [Route("ThemLichHenMoi")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThemLichHenMoi(Appointment appointment)
        {
            if (!Access()) return View("~/Views/Home/Index.cshtml");

            // Kiểm tra nếu DoctorId tồn tại trong bảng Doctor
            if (!db.Doctors.Any(d => d.DoctorId == appointment.DoctorId))
            {
                ModelState.AddModelError("DoctorId", "Doctor does not exist.");
                ViewBag.ServiceId = new SelectList(db.Services.ToList(), "ServiceId", "ServiceName");
            }

            if (ModelState.IsValid == false)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                db.Appointments.Add(appointment);
                db.SaveChanges();
                return RedirectToAction("DanhMucLichHen");
            }

            return View(appointment);
        }


        [Route("SuaLichHen")]
        [HttpGet]
        public IActionResult SuaLichHen(int appointmentId)
        {
            if (appointmentId == 0 || db.Appointments == null)
            {
                return NotFound();
            }
            var LichHen = db.Appointments.Find(appointmentId);
            if (LichHen == null)
            {
                return NotFound();
            }

            if (!Access())
            {
                return View("~/Views/Home/Index.cshtml");
            }

            ViewBag.ServiceId = new SelectList(db.Services.ToList(), "ServiceId", "ServiceName");
            return View(LichHen);
        }
        [Route("SuaLichHen")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SuaLichHen([Bind("AppointmentId, AppointmentDate, PatientId, DoctorId, ServiceId, AppointmentStatus")] Appointment appointment)
        {
            if (!Access())
            {
                Console.WriteLine("Access denied.");
                return View("~/Views/Home/Index.cshtml");
            }

            if (ModelState.IsValid == false)
            {

                try
                {


                    db.Entry(appointment).State = EntityState.Modified;
                    db.SaveChanges();


                    return RedirectToAction("DanhMucLichHen", "HomeAdmin");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(appointment.AppointmentId))
                    {
                        Console.WriteLine("Appointment does not exist.");
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else
            {
                Console.WriteLine("Model state is not valid.");
            }

            ViewBag.ServiceId = new SelectList(db.Services, "ServiceId", "ServiceName", appointment.ServiceId);
            return View(appointment);
        }


        // Phương thức kiểm tra sự tồn tại của một Appointment
        private bool AppointmentExists(int id)
        {
            return db.Appointments.Any(e => e.AppointmentId == id);
        }


        [Route("XoaLichHen")]
        [HttpGet]
        public IActionResult XoaLichHen(string appointmentId)
        {
            if (!Access()) return View("~/Views/Home/Index.cshtml");

            if (!int.TryParse(appointmentId, out var id))
            {
                TempData["Message"] = "ID lịch hẹn không hợp lệ.";
                return RedirectToAction("DanhMucLichHen", "HomeAdmin");
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var appointment = db.Appointments.Find(id);
                    var record = db.Records.Find(id);

                    if (appointment == null && record == null)
                    {
                        TempData["Message"] = "Lịch hẹn không tồn tại.";
                        return RedirectToAction("DanhMucLichHen", "HomeAdmin");
                    }

                    if (record != null)
                    {
                        db.Records.Remove(record);
                    }

                    if (appointment != null)
                    {
                        db.Appointments.Remove(appointment);
                    }

                    db.SaveChanges();
                    transaction.Commit();
                    TempData["Message"] = "Lịch hẹn đã được xóa.";
                    return RedirectToAction("DanhMucLichhen", "HomeAdmin");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["Message"] = "Có lỗi xảy ra khi xóa lịch hẹn.";
                    return RedirectToAction("DanhMucLichHen", "HomeAdmin");
                }
            }
        }
    }
}
