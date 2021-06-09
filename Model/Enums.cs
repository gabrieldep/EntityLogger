using System.ComponentModel.DataAnnotations;

namespace AppLogger.Model
{
    public class Enums
    {
        public enum EntityType
        {
            [Display(Name = "Old")]
            Old,
            [Display(Name = "New")]
            New
        }
    }
}
