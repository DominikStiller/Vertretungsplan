package de.dominikstiller.vertretungsplan.converter;

import java.time.LocalDate;
import java.time.LocalDateTime;
import java.util.List;

class Vertretungsplan {

   // Metadata
   public LocalDate date;
   public int version;
   public LocalDateTime lastUpdated;
   public String studentNotes;
   public String teacherNotes;
   public String absentForms;
   public String absentTeachers;
   public String missingRooms;

   public List<Entry> entries;
}

class Entry {

   public String form;
   public int lesson;
   public String originalTeacher;
   public String originalSubject;
   public String substitutionTeacher;
   public String substitutionSubject;
   public String substitutionRoom;
   public String note;
}
