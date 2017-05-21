using System;

namespace DominikStiller.VertretungsplanServer.Models
{
    public class VertretungsplanMetadata : IEquatable<VertretungsplanMetadata>
    {
        public DateTime Date { get; set; }
        public int Version { get; set; }
        public DateTime LastUpdated { get; set; }

        public VertretungsplanMetadata() { }

        public VertretungsplanMetadata(Vertretungsplan vp)
        {
            Date = vp.Date;
            Version = vp.Version;
            LastUpdated = vp.LastUpdated;
        }

        public bool Equals(VertretungsplanMetadata other)
        {
            return Date == other.Date
                && Version == other.Version
                && LastUpdated == other.LastUpdated;
        }
    }
}
