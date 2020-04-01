using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// This controller is used to create multiple resources at once, like the Authors here
namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authorcollections")]
    public class AuthorCollectionsController: ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ICourseLibraryRepository _clRepo;

        public AuthorCollectionsController(
            ICourseLibraryRepository clRepo, 
            IMapper mapper
            )
        {
            _mapper = mapper ?? 
                throw new ArgumentException(nameof(clRepo));
            _clRepo = clRepo ?? 
                throw new ArgumentException(nameof(mapper));
        }

        // we need to use custom model binding because we need .net core to parse all the ids in the route to 
            // an IEnumerable of those IDs.
        [HttpGet("({ids})", Name = "GetAuthorCollection")]  // getting a list of Ids in the URI / query strings
        public IActionResult GetAuthorCollection (
            [FromRoute] 
            [ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids // using custom Model binding.
            )
        {
            if (ids == null)
                return BadRequest(); // if the ids could not be parsed, the reqeust was bad.

            var authorEntities = _clRepo.GetAuthors(ids);

            if (ids.Count() != authorEntities.Count())
                return NotFound();

            var authorsToReturn = _mapper.Map<IEnumerable<AuthorDTO>>(authorEntities);

            return Ok(authorsToReturn);
        }

        [HttpPost]
        public ActionResult<IEnumerable<AuthorDTO>> CreateAuthorCollection (
            IEnumerable<AuthorForCreationDTO> authorCollection
            )
        {
            var authorEntities = _mapper.Map<IEnumerable<Author>>(authorCollection);

            foreach(var author in authorEntities)
            {
                _clRepo.AddAuthor(author);
            }

            _clRepo.Save();

            var authorCollectionToReturn = _mapper.Map<IEnumerable<AuthorDTO>>(authorEntities);
            var idsAsString = string.Join(",", authorCollectionToReturn.Select(a => a.Id));

            return CreatedAtRoute(
                "GetAuthorCollection",
                new { ids = idsAsString },
                authorCollectionToReturn
                );
        }
    }
}
