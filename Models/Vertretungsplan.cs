using System;
using System.Collections.Generic;

namespace DominikStiller.VertretungsplanServer.Models
{
    public class Vertretungsplan : IComparable<Vertretungsplan>
    {
        public DateTime Date { get; set; }
        public int Version { get; set; }
        public DateTime LastUpdated { get; set; }
        public String StudentNotes { get; set; }
        public String TeacherNotes { get; set; }
        public String AbsentForms { get; set; }
        public String AbsentTeachers { get; set; }
        public String MissingRooms { get; set; }

        public List<Entry> Entries { get; set; }

        public int CompareTo(Vertretungsplan other)
        {
            return this.Date.CompareTo(other.Date);
        }
    }

    public class Entry
    {
        public String Form { get; set; }
        public int Lesson { get; set; }
        public String OriginalTeacher { get; set; }
        public String OriginalSubject { get; set; }
        public String SubstitutionTeacher { get; set; }
        public String SubstitutionSubject { get; set; }
        public String SubstitutionRoom { get; set; }
        public String Note { get; set; }

        public Entry Clone()
        {
            return (Entry)MemberwiseClone();
        }
    }


    public enum VertretungsplanType
    {
        STUDENTS, TEACHERS
    }
}
