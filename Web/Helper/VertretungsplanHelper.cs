using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;

using DominikStiller.VertretungsplanServer.Models;
using DominikStiller.VertretungsplanServer.Web.Controllers;

namespace DominikStiller.VertretungsplanServer.Web.Helper
{
    class VertretungsplanHelper
    {
        const string DATEFORMAT_INTERNAL = "yyyy-MM-dd";
        const string DATEFORMAT_PUBLIC = "dddd, dd.MM";

        public static VertretungsplanViewModel GenerateViewModel(VertretungsplanRepository vertretungsplanRepository, VertretungsplanType type, DateTime? date = null)
        {
            var model = new VertretungsplanViewModel();
            var vertretungsplan = vertretungsplanRepository.FindNearest(date ?? DateTime.Now);

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

                model.LastUpdatedInWords = TimespanInWords(DateTime.Now - model.Vertretungsplan.LastUpdated);

                model.Dates = vertretungsplanRepository.GetAllDates().Select(d => d.ToString(DATEFORMAT_INTERNAL)).ToList();
                model.PreviousDate = vertretungsplanRepository.GetPrevious(vertretungsplan).Date;
                model.NextDate = vertretungsplanRepository.GetNext(vertretungsplan).Date;
                model.DateSelectorItems = vertretungsplanRepository.GetAllDates().Select(dateOption =>
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

        static string TimespanInWords(TimeSpan timespan)
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

        static string FormatTimespan(double quantity, string singular, string plural)
        {
            return String.Format("{0} {1}", Math.Floor(quantity), quantity < 2 ? singular : plural);
        }
    }
}
