namespace SnipperSnippets.Models
{
    public class Snippet
    {
        public int Id { get; set; } // primary key
        public required string Language { get; set; } // snippet language 
        public required string Code { get; set; } // snippet code    
    }
}
