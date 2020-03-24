using AutoMapper;
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

        [HttpGet("/{courseId}")]
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
    }
}
