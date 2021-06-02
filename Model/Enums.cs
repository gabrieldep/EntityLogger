using System.ComponentModel.DataAnnotations;

namespace AppLogger.Model
{
    public class Enums
    {
        public enum LogType
        {
            [Display(Name = "Create")]
            Create,
            [Display(Name = "Edit")]
            Edit,
            [Display(Name = "Delete")]
            Delete
        }

        public enum EntityType
        {
            [Display(Name = "Old")]
            Old,
            [Display(Name = "New")]
            New
        }
    }
}
