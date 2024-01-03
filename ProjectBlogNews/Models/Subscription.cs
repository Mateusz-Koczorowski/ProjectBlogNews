using ProjectBlogNews.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectBlogNews.Models
{
    public class Subscription
    {
        public int? Id { get; set; }
        public DateTime? SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        [ForeignKey("ApplicationUser")]
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}
