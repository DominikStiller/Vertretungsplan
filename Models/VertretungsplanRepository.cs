using System;
using System.Collections.Generic;
using System.Linq;

namespace DominikStiller.VertretungsplanServer.Models
{
    public class VertretungsplanRepository
    {
        List<Vertretungsplan> vertretungsplans;

        public VertretungsplanRepository()
        {
            vertretungsplans = new List<Vertretungsplan>();
        }

        public void Add(Vertretungsplan vertretungsplan)
        {
            Remove(vertretungsplan.Date);
            vertretungsplans.Add(vertretungsplan);
            vertretungsplans.Sort();
        }

        public void AddRange(IEnumerable<Vertretungsplan> collection)
        {
            vertretungsplans.AddRange(collection);
            vertretungsplans.Sort();
        }

        public IEnumerable<Vertretungsplan> GetAll()
        {
            return vertretungsplans;
        }

        public IEnumerable<DateTime> GetAllDates()
        {
            return vertretungsplans.Select(v => v.Date);
        }

        public Vertretungsplan Find(DateTime date)
        {
            return vertretungsplans.FirstOrDefault(v => v.Date == date.Date);
        }

        public Vertretungsplan FindNearest(DateTime date)
        {
            // Return today, nearest future date, nearest past date or null in this order
            return vertretungsplans.FirstOrDefault(v => v.Date >= date.Date) ?? vertretungsplans.LastOrDefault(v => v.Date < date.Date);
        }

        public Vertretungsplan GetPrevious(Vertretungsplan vertretungsplan)
        {
            int index = vertretungsplans.IndexOf(vertretungsplan);
            if (index == 0)
                return vertretungsplans.Last();
            else
                return vertretungsplans.ElementAt(index - 1);
        }

        public Vertretungsplan GetNext(Vertretungsplan vertretungsplan)
        {
            int index = vertretungsplans.IndexOf(vertretungsplan);
            if (index == vertretungsplans.Count - 1)
                return vertretungsplans.First();
            else
                return vertretungsplans.ElementAt(index + 1);
        }

        public Boolean Contains(DateTime date)
        {
            return vertretungsplans.Any(v => v.Date == date.Date);
        }

        public void Remove(DateTime date)
        {
            vertretungsplans.RemoveAll(v => v.Date == date.Date);
        }

        public void Clear()
        {
            vertretungsplans.Clear();
        }
    }
}
