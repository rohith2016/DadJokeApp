namespace Domain.Entities
{
    public class SearchTerm
    {
        public Guid Id { get; set; }
        public string Term { get; set; }
        public DateTime LastSearchedAt { get; set; }
        public int SearchCount { get; set; }
    }  
}
