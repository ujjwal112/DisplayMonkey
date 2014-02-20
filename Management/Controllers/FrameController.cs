﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DisplayMonkey.Models;

namespace DisplayMonkey.Controllers
{
    public class FrameController : Controller
    {
        private DisplayMonkeyEntities db = new DisplayMonkeyEntities();

        public const string SelectorFrameKey = "_selectorFrame";

        //
        // GET: /Frame/

        public ActionResult Index(int canvasId = 0, int panelId = 0, string frameType = "")
        {
            Navigation.SaveCurrent();

            var list = db.Frames
                .Include(f => f.Panel)
                .Include(f => f.Panel.Canvas)
                .Include(f => f.News)
                .Include(f => f.Clock)
                .Include(f => f.Weather)
                .Include(f => f.Memo)
                .Include(f => f.Report)
                .Include(f => f.Picture)
                .Include(f => f.Video)
                ;

            if (canvasId > 0)
            {
                list = list.Where(f => f.Panel.CanvasId == canvasId);
            }

            if (panelId > 0)
            {
                list = list.Where(f => f.PanelId == panelId);
            }

            if (frameType != "")
            {
                list = list.Where(Frame.FilterByFrameType(frameType));
            }

            FillCanvasesSelectList(canvasId);
            FillPanelsSelectList(panelId, canvasId);
            FillFrameTypeSelectList(frameType);
             
            return View(list.ToList());
        }

        private void FillCanvasesSelectList(object selected = null)
        {
            var query = from c in db.Canvases
                        orderby c.Name
                        select c;
            ViewBag.CanvasId = new SelectList(query, "CanvasId", "Name", selected);
        }

        private void FillPanelsSelectList(object selected = null, int canvasId = 0)
        {
            if (canvasId > 0)
            {
                var query = db.Panels
                    .Where(p => p.CanvasId == canvasId)
                    .Select(p => new { 
                        PanelId = p.PanelId, 
                        Name = p.Name 
                    })
                    .OrderBy(p => p.Name)
                    .ToList()
                    ;

                ViewBag.PanelId = new SelectList(query, "PanelId", "Name", selected);
            }
            else
            {
                var query = db.Panels
                    .Include(p => p.Canvas)
                    .Select(p => new {
                            PanelId = p.PanelId, 
                            Name = p.Canvas.Name + " : " + p.Name
                    })
                    .OrderBy(p => p.Name)
                    .ToList()
                    ;

                ViewBag.PanelId = new SelectList(query, "PanelId", "Name", selected);
            }
        }

        private void FillFrameTypeSelectList(object selected = null)
        {
            ViewBag.FrameType = new SelectList(
                Frame.FrameTypes,
                "FrameType",
                "FrameType",
                selected
            );
        }

        //
        // GET: /Frame/Create

        public ActionResult Create(int canvasId = 0, int panelId = 0, string frameType = "")
        {
            if (panelId == 0)
            {
                if (canvasId == 0)
                    return RedirectToAction("ForCanvas");
                else
                    return RedirectToAction("ForPanel", new { canvasId = canvasId });
            }
            
            else if (TempData[SelectorFrameKey] == null)
            {
                Panel panel = db.Panels
                    .Include(p => p.Canvas)
                    .First(p => p.PanelId == panelId)
                    ;

                FrameSelector selector = new FrameSelector()
                {
                    Panel = panel,
                    PanelId = panel.PanelId,
                };

                TempData[SelectorFrameKey] = selector;
            }

            if (string.IsNullOrEmpty(frameType))
            {
                return RedirectToAction("ForFrameType", new { panelId = panelId });
            }

            return RedirectToAction("Create", frameType);
        }

        public ActionResult ForCanvas()
        {
            FillCanvasesSelectList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForCanvas(Canvas canvas)
        {
            if (canvas.CanvasId > 0)
            {
                return RedirectToAction("ForPanel", new { canvasId = canvas.CanvasId });
            }

            return RedirectToAction("ForCanvas");
        }

        public ActionResult ForPanel(int canvasId)
        {
            Panel panel = db.Panels
                .Include(p => p.Canvas)
                .First(p => p.CanvasId == canvasId)
                ;

            FillPanelsSelectList(canvasId: canvasId);
            return View(panel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForPanel(Panel panel)
        {
            if (panel.PanelId > 0)
            {
                return RedirectToAction("ForFrameType", new { panelId = panel.PanelId });
            }

            return RedirectToAction("ForPanel", new { canvasId = panel.Canvas.CanvasId });
        }

        public ActionResult ForFrameType(int panelId)
        {
            Panel panel = db.Panels
                .Include(p => p.Canvas)
                .First(p => p.PanelId == panelId)
                ;

            FrameSelector selector = new FrameSelector() 
            { 
                Panel = panel,
                PanelId = panel.PanelId,
            };

            FillFrameTypeSelectList();
            return View(selector);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForFrameType(FrameSelector selector)
        {
            if (selector.FrameType != "")
            {
                TempData[SelectorFrameKey] = selector;
                return RedirectToAction("Create", selector.FrameType);
            }

            return RedirectToAction("ForFrameType", new { panelId = selector.PanelId });
        }

        //
        // GET: /Frame/Details/5

        public ActionResult Details(int id = 0)
        {
            Frame frame = db.Frames
                .Where( f => f.FrameId == id)
                .Include(f => f.Panel)
                .Include(f => f.Panel.Canvas)
                .Include(f => f.News)
                .Include(f => f.Clock)
                .Include(f => f.Weather)
                .Include(f => f.Memo)
                .Include(f => f.Report)
                .Include(f => f.Picture)
                .Include(f => f.Video)
                .FirstOrDefault()
                ;

            if (frame == null)
            {
                return HttpNotFound();
            }

            return RedirectToAction("Details", frame.FrameType, new { id = id });
        }

        //
        // GET: /Frame/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Frame frame = db.Frames
                .Where(f => f.FrameId == id)
                .Include(f => f.Panel)
                .Include(f => f.Panel.Canvas)
                .Include(f => f.News)
                .Include(f => f.Clock)
                .Include(f => f.Weather)
                .Include(f => f.Memo)
                .Include(f => f.Report)
                .Include(f => f.Picture)
                .Include(f => f.Video)
                .FirstOrDefault()
                ;

            if (frame == null)
            {
                return HttpNotFound();
            }

            return RedirectToAction("Edit", frame.FrameType, new { id = id });
        }

        //
        // GET: /Frame/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Frame frame = db.Frames
                .Where(f => f.FrameId == id)
                .Include(f => f.Panel)
                .Include(f => f.Panel.Canvas)
                .Include(f => f.News)
                .Include(f => f.Clock)
                .Include(f => f.Weather)
                .Include(f => f.Memo)
                .Include(f => f.Report)
                .Include(f => f.Picture)
                .Include(f => f.Video)
                .FirstOrDefault()
                ;

            if (frame == null)
            {
                return HttpNotFound();
            }

            return RedirectToAction("Delete", frame.FrameType, new { id = id });
        }

        //
        // GET: /Frame/Attach/5

        public ActionResult Attach(int id = 0)
        {
            Frame frame = db.Frames.Find(id);
            if (frame == null)
            {
                return HttpNotFound();
            }

            LocationSelector selector = new LocationSelector
            {
                FrameId = id,
            };

            var locations = db.Locations
                .Where(l => !db.Frames
                    .FirstOrDefault(f => f.FrameId == selector.FrameId)
                    .Locations.Any(fl => fl.LocationId == l.LocationId))
                    .Include(l => l.Level)
                    .Select(l => new
                    {
                        LocationId = l.LocationId,
                        Name = l.Level.Name + " : " + l.Name
                    })
                    .OrderBy(l => l.Name)
                    .ToList()
                ;
            ViewBag.Locations = new SelectList(locations, "LocationId", "Name");

            return View(selector);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Attach(LocationSelector selector)
        {
            Frame frame = db.Frames.Find(selector.FrameId);
            if (frame == null)
            {
                return HttpNotFound();
            }

            if (selector.LocationId > 0)
            {
                Location location = db.Locations.Find(selector.LocationId);
                if (location == null)
                {
                    return HttpNotFound();
                }
                frame.Locations.Add(location);
                db.SaveChanges();
                Navigation.Restore();
                return RedirectToAction("Index");
            }

            IEnumerable<Location> locations = db.Locations
                .Where(l => !db.Frames
                    .FirstOrDefault(f => f.FrameId == selector.FrameId)
                    .Locations.Any(fl => fl.LocationId == l.LocationId))
                ;
            ViewBag.Locations = new SelectList(db.Locations, "LocationId", "Name");

            return View(selector);
        }

        //
        // GET: /Frame/Detach/5

        public ActionResult Detach(int id = 0, int locationId = 0)
        {
            Frame frame = db.Frames.Find(id);
            if (frame == null)
            {
                return HttpNotFound();
            }

            LocationSelector selector = new LocationSelector
            {
                FrameId = id,
                LocationId = locationId,
                LocationName = db.Locations.Find(locationId).Name,
            };

            return View(selector);
        }

        [HttpPost, ActionName("Detach")]
        [ValidateAntiForgeryToken]
        public ActionResult DetachConfirmed(int id, int locationId)
        {
            Frame frame = db.Frames.Find(id);
            Location location = db.Locations.Find(locationId);
            frame.Locations.Remove(location);
            db.SaveChanges();

            Navigation.Restore();
            return RedirectToAction("Index", "Frame");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}