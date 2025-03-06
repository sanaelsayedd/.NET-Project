using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Universtiy_Management_System.Areas.Courses.Models;
using Universtiy_Management_System.Areas.Departments.Models;
using Universtiy_Management_System.Areas.Enrollment.Models;
using Universtiy_Management_System.Areas.Students.Models;
using Universtiy_Management_System.Data;

namespace Universtiy_Management_System.Areas.Students.Controllers
{

    [Area("Students"), Authorize]
    public class StudentsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public StudentsController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> GetUserByEmail()
        {
            var loggedInUser = await _userManager.GetUserAsync(User);

            if (loggedInUser == null)
            {
                return Unauthorized();
            }

            // Check if the user's email is confirmed
            if (!await _userManager.IsEmailConfirmedAsync(loggedInUser))
            {
                return Content("Email not confirmed");
            }

            // Get email from IdentityUser
            var loggedInEmail = loggedInUser.Email;

            // Query your custom users model to get more details based on the email
            var student = await _context.Students.FirstOrDefaultAsync(u => u.Email == loggedInEmail);

            if (student == null)
            {
                return NotFound("User not found in custom users table.");
            }

            // Pass the user details to the view
            return View(student);
        }
    
    // GET: Students/Students
    public async Task<IActionResult> Index()
        {
            return View(await _context.Students.ToListAsync());
        }

        // GET: Students/Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.StudentId == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Students/Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StudentId,FirstName,LastName,Email,DateOfBirth")] Student student)
        {
            // التحقق إذا كان البريد الإلكتروني للطالب موجود بالفعل
            var existingStudent = await _context.Students
                .AnyAsync(s => s.Email == student.Email);

            if (existingStudent)
            {
                // تخزين رسالة الخطأ في TempData
                TempData["ErrorMessage"] = "A student with this email already exists.";
                return RedirectToAction(nameof(Create));
            }

            if (ModelState.IsValid)
            {
                _context.Add(student);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Student was added successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(student);
        }


        // GET: Students/Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // POST: Students/Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StudentId,FirstName,LastName,Email,DateOfBirth")] Student student)
        {
            if (id != student.StudentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    TempData["SuccessMessage"] = "student was updated successfully!";
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.StudentId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Students/Students/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.StudentId == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Students/Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                var enrollments = await _context.Enrollments
                   .Where(e => e.StudentId ==student.StudentId)
                   .ToListAsync();

                // حذف جميع enrollments المرتبطة بالكورسات
                _context.Enrollments.RemoveRange(enrollments);
                _context.Students.Remove(student);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "student was Deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.StudentId == id);
        }
       
    }
}
