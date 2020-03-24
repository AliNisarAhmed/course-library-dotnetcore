using AutoMapper;
using CourseLibrary.API.Helpers;
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
    [Route("/api/authors")]
    public class AuthorsControllers: ControllerBase 
    {
        private readonly ICourseLibraryRepository _clRepo;
        private readonly IMapper _mapper;

        public AuthorsControllers(ICourseLibraryRepository clRepo, IMapper mapper)
        {
            _clRepo = clRepo ?? 
                throw new ArgumentException(nameof(clRepo));
            _mapper = mapper ??
                throw new ArgumentException(nameof(mapper));
        }

        [HttpGet("/api/authors")]
        public ActionResult<IEnumerable<AuthorDTO>> GetAuthors()
        {
            var authors = _clRepo.GetAuthors();

            var result = _mapper.Map<IEnumerable<AuthorDTO>>(authors);

            return Ok(result);
        }

        [HttpGet("{authorId:guid}")]
        public ActionResult<AuthorDTO> GetAuthor(Guid authorId)
        {
            var author = _clRepo.GetAuthor(authorId);
            if (author == null)
                return NotFound();
                
            return new JsonResult(_mapper.Map<AuthorDTO>(author));
        }
    }
}
