
using CourseLibrary.API.ValidationAttributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Models
{
    // Class level attribute
    [CourseTitleMustBeDifferentFromDescription(ErrorMessage = "Title must be different from description")]
    public class CourseForCreationDTO  // : IValidatableObject  not needed since we created a class level attribute
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(100, ErrorMessage = "The title shouln't have more than 100 characters.")]
        public string Title { get; set; }

        [MaxLength(1500, ErrorMessage = "The description should'nt have more than 1500 characters")]
        public string Description { get; set; }


        // In .Net Core, the Validate method is not executed if any data annotations above has already resulted in error
        
        
        /*
        Not needed since we now have a class level atttribute

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Title == Description)
            {
                yield return new ValidationResult(
                    "The provided description should be different from the title",
                    new[] { "CouurseForCreationDTO" } // the name of the class where validation error occurred
                    );
            }
        }
        */
    }
}
