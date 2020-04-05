using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
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
        public ActionResult UpdateCourseForAuthor(Guid authorId, Guid courseId, CourseForUpdateDTO newCourse)
        {
            if (!_clRepo.AuthorExists(authorId))
                return NotFound();

            var courseForAuthorFromRepo = _clRepo.GetCourse(authorId, courseId);

            if (courseForAuthorFromRepo == null)
                return NotFound();

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
    }
}
