 using LegalAndGeneralConsultantCRM.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalAndGeneralConsultantCRM.Models.Universiies
{
    public class UniversityCourse // New class to represent the many-to-many relationship
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UniversityCourseId { get; set; }

        public int? UniversityId { get; set; }
        public University University { get; set; }

        public int? CourseId { get; set; }
        public Course Course { get; set; }

        public decimal? TuitionFee { get; set; }
    }
}
