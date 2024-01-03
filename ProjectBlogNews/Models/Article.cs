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
    }
}
