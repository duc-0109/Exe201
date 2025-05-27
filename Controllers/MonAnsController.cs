using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartCookFinal.Models;
using X.PagedList.EntityFramework;
using X.PagedList.Extensions;
using X.PagedList;

namespace SmartCookFinal.Controllers
{
    public class MonAnsController : Controller
    {
        private readonly SmartCookContext _context;

        public MonAnsController(SmartCookContext context)
        {
            _context = context;
        }

		// GET: MonAns
		public IActionResult Index(int? page)
		{
			int pageSize = 12;
			int pageNumber = page ?? 1;

			var monAnList = _context.MonAns.Include(m => m.DanhMuc).OrderBy(m => m.Id);
            ViewBag.CurrentAction = "Index";

            return View(monAnList.ToPagedList(pageNumber, pageSize));
		}


		// GET: MonAns/Details/5
		public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var monAn = await _context.MonAns
                .Include(m => m.DanhMuc)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (monAn == null)
            {
                return NotFound();
            }

            return View(monAn);
        }

        // GET: MonAns/Create
        public IActionResult Create()
        {
            ViewData["DanhMucId"] = new SelectList(_context.DanhMucMonAns, "Id", "Id");
            return View();
        }

        // POST: MonAns/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TenMon,MoTa,LoaiBuaAn,ThoiGianNau,LuongCalo,Carbs,Protein,Fat,ChiPhiUocTinh,Chay,AnKeto,AnKhongGluten,NguyenLieuChinh,DinhDuongChiTiet,UrlHinhAnh,CachNau,TrangThai,DanhMucId")] MonAn monAn)
        {
            if (ModelState.IsValid)
            {
                _context.Add(monAn);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DanhMucId"] = new SelectList(_context.DanhMucMonAns, "Id", "Id", monAn.DanhMucId);
            return View(monAn);
        }

        // GET: MonAns/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var monAn = await _context.MonAns.FindAsync(id);
            if (monAn == null)
            {
                return NotFound();
            }
            ViewData["DanhMucId"] = new SelectList(_context.DanhMucMonAns, "Id", "Id", monAn.DanhMucId);
            return View(monAn);
        }

        // POST: MonAns/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TenMon,MoTa,LoaiBuaAn,ThoiGianNau,LuongCalo,Carbs,Protein,Fat,ChiPhiUocTinh,Chay,AnKeto,AnKhongGluten,NguyenLieuChinh,DinhDuongChiTiet,UrlHinhAnh,CachNau,TrangThai,DanhMucId")] MonAn monAn)
        {
            if (id != monAn.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(monAn);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MonAnExists(monAn.Id))
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
            ViewData["DanhMucId"] = new SelectList(_context.DanhMucMonAns, "Id", "Id", monAn.DanhMucId);
            return View(monAn);
        }

        // GET: MonAns/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var monAn = await _context.MonAns
                .Include(m => m.DanhMuc)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (monAn == null)
            {
                return NotFound();
            }

            return View(monAn);
        }

        // POST: MonAns/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var monAn = await _context.MonAns.FindAsync(id);
            if (monAn != null)
            {
                _context.MonAns.Remove(monAn);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MonAnExists(int id)
        {
            return _context.MonAns.Any(e => e.Id == id);
        }

		public async Task<IActionResult> SearchByName(string name, int? page)
		{
			int pageSize = 6;
			int pageNumber = page ?? 1;

			var query = _context.MonAns
				.Where(m => m.TenMon.Contains(name))
				.OrderBy(m => m.TenMon);

            ViewBag.SearchTerm = name;
            ViewBag.CurrentAction = "SearchByName";

            return View("Index", query.ToPagedList(pageNumber, pageSize));

        }

        public async Task<IActionResult> FilterByBuaAn(string loaiBuaAn, int? page)
		{
			int pageSize = 6;
			int pageNumber = page ?? 1;

			var query = _context.MonAns
				.Where(m => m.LoaiBuaAn != null && m.LoaiBuaAn.ToLower() == loaiBuaAn.ToLower())
				.OrderBy(m => m.TenMon);

            ViewBag.LoaiBuaAn = loaiBuaAn;
            ViewBag.CurrentAction = "FilterByBuaAn";

            return View("Index", query.ToPagedList(pageNumber, pageSize));

        }


    }
}
