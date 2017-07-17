using SQLite;

namespace DailyContact.Models
{
    public class Message
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public string Location { get; set; }

        public string PhoneNumbers { get; set; }

        public string Situation { get; set; }

        public string DateSent { get; set; }
    }
}