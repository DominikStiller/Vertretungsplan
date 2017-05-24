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

        public VertretungsplanViewModel GenerateViewModel(VertretungsplanType type, DateTime? date)
        {
            var model = new VertretungsplanViewModel();

            var vertretungsplan = cache.Find(date.GetValueOrDefault());

            model.Vertretungsplan = vertretungsplan;

            if (vertretungsplan != null)
            {
                if (type == VertretungsplanType.TEACHERS)
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

                model.Dates = cache.GetAllDates().Where(vp => vp.Date != vertretungsplan.Date).Select(d => d.ToString(DATEFORMAT_INTERNAL)).ToList();
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
        public string GenerateETag()
        {
            var tag = cache.GetAll().Aggregate("", (text, vp) =>
            {
                return text + vp.Date.ToString(DATEFORMAT_INTERNAL) + vp.LastUpdated.ToString() + vp.Version;
            });
            var hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(tag));

            StringBuilder builder = new StringBuilder();
            foreach (byte b in hash)
                builder.Append(b.ToString("x2").ToLower());

            return builder.ToString();
        }

        public DateTime GenerateLastModified(DateTime date)
        {
            return VertretungsplanTime.ConvertVPTimeToUTC(cache.Find(date).LastUpdated);
        }
    }
}
