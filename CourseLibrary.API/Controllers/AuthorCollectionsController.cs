﻿using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.Controllers {
    [ApiController]
    [Route("api/authorcollections")]
    public class AuthorCollectionsController : ControllerBase {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;

        public AuthorCollectionsController(ICourseLibraryRepository courseLibraryRepository,
    IMapper mapper) {
            _courseLibraryRepository = courseLibraryRepository ??
                throw new ArgumentNullException(nameof(courseLibraryRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }

        [HttpPost]
        public ActionResult<IEnumerable<AuthorDto>> CreateAuthorCollection(
            IEnumerable<AuthorForCreationDto> authorCollection) {
            var authorEntities = _mapper.Map<IEnumerable<Author>>(authorCollection);
            foreach (Author author in authorEntities) {
                _courseLibraryRepository.AddAuthor(author);
            }
            _courseLibraryRepository.Save();
            return Ok();
        }
    }
}
