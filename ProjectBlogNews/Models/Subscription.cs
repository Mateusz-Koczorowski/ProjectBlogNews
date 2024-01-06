using ProjectBlogNews.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectBlogNews.Models
{

    
    public class Subscription
    {
        public int? Id { get; set; }
        [DataType(DataType.Date)]
        public DateTime? SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        [ForeignKey("ApplicationUser")]
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        
        
        
        public decimal Price { get; set; }
        public bool IsActive 
        {
            get {
                var date = new DateTime();
                if (SubscriptionStartDate != null)
                {
                    if (SubscriptionStartDate > date)
                    {
                        return false;
                    }
                }
                if (SubscriptionEndDate != null)
                {
                    if (SubscriptionEndDate < date)
                    {
                        return false;
                    }
                }
                return true;
                
                }
        }

        


    }
}
