using Mark2API.Models;
using Mark2API.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Mark2API.Controllers
{
    [Route("api/courses")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly ICoursesRepository _coursesRepository;

        public CoursesController(ICoursesRepository coursesRepository)
        {
            _coursesRepository = coursesRepository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Course>> GetCourses()
        {
            var courses = _coursesRepository.GetCourses();
            return Ok(courses);
        }

        [HttpGet("{id}")]
        public ActionResult<Course> GetCourse(int id)
        {
            var course = _coursesRepository.GetCourseById(id);
            if (course == null)
            {
                return NotFound();
            }
            return Ok(course);
        }

        [HttpPost]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult CreateCourse([FromBody] Course course)
        {
            if (course == null)
            {
                return BadRequest();
            }

            _coursesRepository.CreateCourse(course);
            return CreatedAtAction("GetCourse", new { id = course.CourseId }, course);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult UpdateCourse(int id, [FromBody] Course course)
        {
            if (course == null || id != course.CourseId)
            {
                return BadRequest();
            }

            try
            {
                _coursesRepository.UpdateCourse(id, course);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult DeleteCourse(int id)
        {
            if (!_coursesRepository.CourseExists(id))
            {
                return NotFound();
            }

            _coursesRepository.DeleteCourse(id);
            return NoContent();
        }

        [HttpGet("{courseName}/levelonequestions")]
        public ActionResult<IEnumerable<Question>> GetLevelOneQuestions(string courseName)
        {
            var levelOneQuestions = _coursesRepository.GetLevelOneQuestions(courseName);
            if (levelOneQuestions == null)
            {
                return NotFound();
            }
            return Ok(levelOneQuestions);
        }

        [HttpGet("{courseName}/leveltwoquestions")]
        public ActionResult<IEnumerable<Question>> GetLevelTwoQuestions(string courseName)
        {
            var levelTwoQuestions = _coursesRepository.GetLevelTwoQuestions(courseName);
            if (levelTwoQuestions == null)
            {
                return NotFound();
            }
            return Ok(levelTwoQuestions);
        }

        [HttpGet("{courseName}/levelthreequestions")]
        public ActionResult<IEnumerable<Question>> GetLevelThreeQuestions(string courseName)
        {
            var levelThreeQuestions = _coursesRepository.GetLevelThreeQuestions(courseName);
            if (levelThreeQuestions == null)
            {
                return NotFound();
            }
            return Ok(levelThreeQuestions);
        }
    }
}
