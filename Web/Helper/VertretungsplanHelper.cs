using System;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

using Microsoft.AspNetCore.Mvc.Rendering;

using DominikStiller.VertretungsplanServer.Models;
using DominikStiller.VertretungsplanServer.Web.Controllers;
using DominikStiller.VertretungsplanServer.Helper;

namespace DominikStiller.VertretungsplanServer.Web.Helper
{
    public class VertretungsplanHelper
    {
        const string DATEFORMAT_INTERNAL = "yyyy-MM-dd";
        const string DATEFORMAT_PUBLIC = "dddd, dd.MM.";

        readonly VertretungsplanRepository cache;

        public VertretungsplanHelper(VertretungsplanRepository cache)
        {
            this.cache = cache;
        }

        public VertretungsplanViewModel GenerateViewModel(VertretungsplanType type, Vertretungsplan vertretungsplan)
        {
            var model = new VertretungsplanViewModel();

            model.Vertretungsplan = vertretungsplan;

            if (vertretungsplan != null)
            {
                if (type == VertretungsplanType.Teachers)
                {
                    model.Entries = vertretungsplan.Entries
                        // Only show substitutions that are assigned to a teacher
                        .FindAll(e => e.SubstitutionTeacher != "entfällt")
                        .FindAll(e => e.SubstitutionTeacher != "—")
                        // Change order of salutation and name
                        .Select(e =>
                        {
                            Entry newEntry = e.Clone();
                            if (e.SubstitutionTeacher.StartsWith("Frau "))
                            {
                                newEntry.SubstitutionTeacher = e.SubstitutionTeacher.Substring(5) + ", Frau";
                            }
                            else if (e.SubstitutionTeacher.StartsWith("Herr "))
                            {
                                newEntry.SubstitutionTeacher = e.SubstitutionTeacher.Substring(5) + ", Herr";
                            }
                            return newEntry;
                        })
                        .OrderBy(e => e.SubstitutionTeacher)
                        .ThenBy(e => e.Lesson)
                        .ToList();
                }

                model.LastUpdatedInWords = TimespanInWords(VertretungsplanTime.Now - model.Vertretungsplan.LastUpdated);

                model.Dates = cache.GetAllDates().Where(e => e.Date != vertretungsplan.Date).Select(d => d.ToString(DATEFORMAT_INTERNAL)).ToList();
                model.PreviousDate = cache.GetPrevious(vertretungsplan).Date;
                model.NextDate = cache.GetNext(vertretungsplan).Date;
                model.DateSelectorItems = cache.GetAllDates().Select(dateOption =>
                {
                    return new SelectListItem()
                    {
                        Text = dateOption.ToString(DATEFORMAT_PUBLIC),
                        Value = dateOption.ToString(DATEFORMAT_INTERNAL),
                        Selected = dateOption == model.Vertretungsplan.Date
                    };
                }).ToList();

                model.DateformatInternal = DATEFORMAT_INTERNAL;
                model.DateformatPublic = DATEFORMAT_PUBLIC;
            }

            return model;
        }

        string TimespanInWords(TimeSpan timespan)
        {
            if (timespan.TotalDays >= 1)
            {
                return FormatTimespan(timespan.TotalDays, "Tag", "Tagen");
            }
            else if (timespan.TotalHours >= 1)
            {
                return FormatTimespan(timespan.Hours, "Stunde", "Stunden");
            }
            else
            {
                return FormatTimespan(timespan.TotalMinutes, "Minute", "Minuten");
            }
        }

        string FormatTimespan(double quantity, string singular, string plural)
        {
            return String.Format("{0} {1}", Math.Floor(quantity), quantity < 2 ? singular : plural);
        }

        // Use md5-hashed date, last updated and version of all dates as ETag content
        public string GenerateETagAll()
        {
            var tag = cache.GetAll().Aggregate("", (text, vp) =>
            {
                return text + GenerateETagSingle(vp, false);
            });
            return Hash(tag);
        }

        public string GenerateETagSingle(Vertretungsplan vp, bool hash)
        {
            var tag = vp.Date.ToString(DATEFORMAT_INTERNAL) + vp.LastUpdated.ToString() + TimespanInWords(VertretungsplanTime.Now - vp.LastUpdated) + vp.Version;
            return hash ? Hash(tag) : tag;
        }

        private string Hash(string text)
        {
            var hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(text));

            StringBuilder builder = new StringBuilder();
            foreach (byte b in hash)
                builder.Append(b.ToString("x2").ToLower());

            return builder.ToString();
        }
    }
}
