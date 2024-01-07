using ProjectBlogNews.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProjectBlogNews.Models
{
    public class Article
    {
        public int? Id { get; set; }
        public string Title { get; set; }
        [DataType(DataType.Date)]
        public DateTime? ReleaseDate { get; set; }
        public string FreeContent { get; set; }
        public string PremiumContent { get; set; }
        [ForeignKey("ApplicationUser")]
        public string? AuthorId { get; set; }
        public virtual ApplicationUser? Author { get; set; }
        [NotMapped] 
        public IFormFile? ImageFile { get; set; }

        public string? ImageFileName { get; set; }
        public string? AltText { get; set; }


        public string GetAvailablePremiumContentForNonPremium()
        {
            if (string.IsNullOrWhiteSpace(PremiumContent))
            {
                return string.Empty;
            }

            // Split the PremiumContent into words
            string[] words = PremiumContent.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // Calculate the number of words to include (10% of total words)
            int wordsToInclude = (int)Math.Ceiling(words.Length * 0.25);

            // Take the first 10% of words
            string restrictedContent = string.Join(" ", words.Take(wordsToInclude));

            return restrictedContent;
        }
    }
}
