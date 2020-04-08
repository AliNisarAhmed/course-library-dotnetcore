using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authors/{authorId}/courses")]
    public class CoursesController: ControllerBase
    {
        private readonly ICourseLibraryRepository _clRepo;
        private readonly IMapper _mapper;

        public CoursesController(ICourseLibraryRepository clRepo, IMapper mapper)
        {
            _clRepo = clRepo ??
                throw new ArgumentException(nameof(clRepo));
           _mapper = mapper ??
                throw new ArgumentException(nameof(mapper));
        }

        [HttpGet]
        public ActionResult<IEnumerable<CourseDTO>> GetCoursesFromAuthor(Guid authorId)
        {
            if (!_clRepo.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courses = _clRepo.GetCourses(authorId);
            var result = _mapper.Map<IEnumerable<CourseDTO>>(courses);
            return Ok(result);
        }

        [HttpPost] 
        public ActionResult<CourseDTO> CreateCouse(
            Guid authorId, CourseForCreationDTO course
            )
        {
            if (_clRepo.AuthorExists(authorId))
                return NotFound();

            var courseEntity = _mapper.Map<Course>(course);
            _clRepo.AddCourse(authorId, courseEntity);
            _clRepo.Save();

            var courseToReturn = _mapper.Map<CourseDTO>(courseEntity);
            return CreatedAtRoute(
                "GetCourseForAuthor",
                new { 
                    authorId = courseToReturn.AuthorId, 
                    courseId = courseToReturn.Id 
                },
                courseToReturn
                );
        }

        [HttpGet("/{courseId}", Name="GetCourseForAuthor")]
        public ActionResult<CourseDTO> GetCourseForAuthor(Guid authorId, Guid courseId)
        {
            if (!_clRepo.AuthorExists(authorId))
                return NotFound();

            var course = _clRepo.GetCourse(authorId, courseId);

            if (course == null)
                return NotFound();

            var result = _mapper.Map<CourseDTO>(course);

            return Ok(result);
        }

        [HttpPut("{courseId}")]
        public IActionResult UpdateCourseForAuthor(Guid authorId, Guid courseId, CourseForUpdateDTO newCourse)
        {
            if (!_clRepo.AuthorExists(authorId))
                return NotFound();

            var courseForAuthorFromRepo = _clRepo.GetCourse(authorId, courseId);

            if (courseForAuthorFromRepo == null)
            {
                // The line below is used for POST request, below this the logic for UPSERTING is implemented
                //return NotFound();

                // Upserting is when the client is allowed to create resources, even providing the unique identifier to be 
                // set up in the DB
                // if the DB uses int as identifier, than the next one needs to be chosen, but here we use GUIDs so upserting works.
                // the first time the resorce is created, and the second time it is updated.
                // UPSERTING is done by PUT requests, hence it is idempotent.

                var courseToAdd = _mapper.Map<Course>(newCourse);
                courseToAdd.Id = courseId;

                _clRepo.AddCourse(authorId, courseToAdd);

                _clRepo.Save();

                var courseToReturn = _mapper.Map<CourseDTO>(courseToAdd);

                return CreatedAtRoute(
                    "GetCourseForAuthor",
                    new { authorId, courseId = courseToReturn.Id },
                    courseToReturn
                    );

            }

            // map the entity to a CourseForUpdateDTO
            // apply the update field values to the DTO
            // map the CourseForUpdateDTO back to an entity
            // the statement below carries out all the above 3 for us
            _mapper.Map(newCourse, courseForAuthorFromRepo);

            // The above statement copied the updates from the incoming object to the entity.
            // But, we need to update the resourse, an entity is just a representation of the resource

            _clRepo.UpdateCourse(courseForAuthorFromRepo);

            _clRepo.Save();

            return NoContent();  // can also return 200 Ok with updated resource
        }

        [HttpPatch("{courseId}")]
        public ActionResult PartiallyUpdateCourseForAuthor(
            Guid authorId, 
            Guid courseId,
            JsonPatchDocument<CourseForUpdateDTO> patchDocument
            )
        {
            if (_clRepo.AuthorExists(authorId))
                return NotFound();

            var courseForAuthorFromRepo = _clRepo.GetCourse(authorId, courseId);

            if (courseForAuthorFromRepo == null)
                return NotFound();

            var courseToPatch = _mapper.Map<CourseForUpdateDTO>(courseForAuthorFromRepo);
            
            // patch document needs validation
            patchDocument.ApplyTo(courseToPatch, ModelState);

            if (!TryValidateModel(courseToPatch))
                return ValidationProblem(ModelState);

            _mapper.Map(courseToPatch, courseForAuthorFromRepo);

            _clRepo.UpdateCourse(courseForAuthorFromRepo);

            _clRepo.Save();

            return NoContent();
        }

        [HttpDelete("{courseId}")]
        public ActionResult DeleteCourseForAuthor(Guid authorId, Guid courseId)
        {
            if (!_clRepo.AuthorExists(authorId))
                return NotFound();

            var courseForAuthorFromRepo =
                _clRepo.GetCourse(authorId, courseId);

            if (courseForAuthorFromRepo == null)
                return NotFound();

            _clRepo.DeleteCourse(courseForAuthorFromRepo);
            _clRepo.Save();

            return NoContent();
        }

        // making our own custom invalid model state response
        public override ActionResult ValidationProblem(
            [ActionResultObjectValue] ModelStateDictionary modelStateDictionary
            )
        {
            var options =
                HttpContext
                .RequestServices
                .GetRequiredService<IOptions<ApiBehaviorOptions>>();

            return (ActionResult)options.Value.InvalidModelStateResponseFactory(ControllerContext);
        }
    }
}
