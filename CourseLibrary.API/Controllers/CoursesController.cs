﻿using AutoMapper;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.Controllers {
    [ApiController]
    [Route("api/authors/{authorId}/courses")]
    public class CoursesController : ControllerBase {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;

        public CoursesController(ICourseLibraryRepository courseLibraryRepository,
            IMapper mapper) {
            _courseLibraryRepository = courseLibraryRepository ??
                throw new ArgumentNullException(nameof(courseLibraryRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public ActionResult<IEnumerable<CourseDto>> GetCoursesForAuthor(Guid authorId) {
            if (!_courseLibraryRepository.AuthorExists(authorId)) {
                return NotFound();
            }
            var coursesForAuthorFromRepo = _courseLibraryRepository.GetCourses(authorId);
            return Ok(_mapper.Map<IEnumerable<CourseDto>>(coursesForAuthorFromRepo));
        }

        [HttpGet("{courseId}", Name = "GetCourseForAuthor")]
        public ActionResult<CourseDto> GetCourseForAuthor(Guid authorId, Guid courseId) {
            if (!_courseLibraryRepository.AuthorExists(authorId)) {
                return NotFound();
            }
            var courseFromRepo = _courseLibraryRepository.GetCourse(authorId, courseId);
            if (courseFromRepo == null) {
                return NotFound();
            }

            return Ok(_mapper.Map<CourseDto>(courseFromRepo));
        }

        [HttpPost]
        public ActionResult<CourseDto> CreateCourseForAuthor(Guid authorId, CourseForCreationDto course) {
            if (!_courseLibraryRepository.AuthorExists(authorId)) {
                return NotFound();
            }
            Entities.Course courseEntity = _mapper.Map<Entities.Course>(course);
            _courseLibraryRepository.AddCourse(authorId, courseEntity);
            _courseLibraryRepository.Save();

            CourseDto courseToReturn = _mapper.Map<CourseDto>(courseEntity);
            return CreatedAtRoute("GetCourseForAuthor", new { courseId = courseToReturn.Id }, courseToReturn);
        }

        [HttpPut("{courseId}")]
        public IActionResult UpdateCourseForAuthor(Guid authorId, 
            Guid courseId, CourseForUpdateDto course) {
            if (!_courseLibraryRepository.AuthorExists(authorId)) {
                return NotFound();
            }
            var courseFromRepo = _courseLibraryRepository.GetCourse(authorId, courseId);
            if (courseFromRepo == null) {
                Entities.Course courseToAdd = _mapper.Map<Entities.Course>(course);
                courseToAdd.Id = courseId;
                _courseLibraryRepository.AddCourse(authorId, courseToAdd);
                _courseLibraryRepository.Save();
                CourseDto courseToReturn = _mapper.Map<CourseDto>(courseToAdd);
                return CreatedAtRoute("GetCourseForAuthor", new { authorId, courseId = courseToReturn.Id }, courseToReturn);
            }

            // map entity to a CourseForUpdateDto
            // apply updated field values to that Dto
            // map the CourseForUpdateDto back to an entity
            _mapper.Map(course, courseFromRepo);

            _courseLibraryRepository.UpdateCourse(courseFromRepo);
            _courseLibraryRepository.Save();
            return NoContent();
        }

        [HttpPatch]

        public IActionResult PartiallyUpdateCourseForAuthor(Guid authorId, Guid courseId, 
            JsonPatchDocument<CourseForUpdateDto> patchDocument) {
            if (!_courseLibraryRepository.AuthorExists(authorId)) {
                return NotFound();
            }
            var courseFromRepo = _courseLibraryRepository.GetCourse(authorId, courseId);
            if (courseFromRepo == null) {
                return NotFound();
            }

            var courseToPatch = _mapper.Map<CourseForUpdateDto>(courseFromRepo);
            // add some validation here
            patchDocument.ApplyTo(courseToPatch);

            _mapper.Map(courseToPatch, courseFromRepo);

            _courseLibraryRepository.UpdateCourse(courseFromRepo);
            _courseLibraryRepository.Save();

            return NoContent();
        }
    }
}
