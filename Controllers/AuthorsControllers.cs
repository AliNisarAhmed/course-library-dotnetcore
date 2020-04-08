using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParameters;
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

        [HttpGet()]
        [HttpHead]  // This attribute adds support for HEAD requests for this route
        public ActionResult<IEnumerable<AuthorDTO>> GetAuthors(
            //[FromQuery] string mainCategory, // optional Data Binding attribute,
            //string searchQuery
            // we can also use separate "request" classes
            [FromQuery] AuthorsResourceParameters request
            )
        {
            var authors = _clRepo.GetAuthors(request);

            var result = _mapper.Map<IEnumerable<AuthorDTO>>(authors);

            return Ok(result);
        }

        [HttpGet("{authorId:guid}", Name = "GetAuthor")]
        public ActionResult<AuthorDTO> GetAuthor(Guid authorId)
        {
            var author = _clRepo.GetAuthor(authorId);
            if (author == null)
                return NotFound();
                
            return new JsonResult(_mapper.Map<AuthorDTO>(author));
        }

        [HttpPost] 
        public ActionResult<AuthorDTO> CreateAuthor(AuthorForCreationDTO author)
        {
            // null check is not needed for author
            // .net core does it for us automatically

            var authorEntity = _mapper.Map<Author>(author);
            _clRepo.AddAuthor(authorEntity);
            _clRepo.Save();

            // AddAuthor method adds an Id value to the passed value.

            var authorToReturn = _mapper.Map<AuthorDTO>(authorEntity);
            return CreatedAtRoute(
                "GetAuthor",
                new { authorId = authorToReturn.Id }, // route paramters
                authorToReturn
                );
        }

        // HTTP Options request allows the client to check which HTTP methods are available on a route
            // in this case /api/authors
        [HttpOptions]
        public IActionResult GetAuthorsOptions()
        {
            Response.Headers.Add("Allow", "GET,OPTIONS,POST");
            return Ok();
        }

        [HttpDelete("{authorId}")]
        public ActionResult DeleteAuthor(Guid authorId)
        {
            var authorFromRepo = _clRepo.GetAuthor(authorId);

            if (authorFromRepo == null)
                return NotFound();

            _clRepo.DeleteAuthor(authorFromRepo);

            _clRepo.Save();

            return NoContent();
        }
    }
}
