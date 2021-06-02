using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
