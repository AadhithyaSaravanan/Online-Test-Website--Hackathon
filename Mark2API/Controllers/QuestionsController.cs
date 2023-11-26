using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mark2API.Models;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.AspNetCore.Authorization;
using OfficeOpenXml;

namespace Mark2API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly OnlineDbContext _context;

        public QuestionsController(OnlineDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpGet]
        public ActionResult<IEnumerable<Question>> GetQuestions()
        {
            if (_context.Questions == null)
            {
                return NotFound();
            }
            var questions = _context.Questions.Include(q => q.Course).ToList();
            return Ok(questions);
        }

        [HttpGet("{id}")]//get by id exception
        public ActionResult<Question> GetQuestion(int id)
        {
            if (_context.Questions == null)
            {
                return NotFound();
            }
            var question = _context.Questions.Find(id);

            if (question == null)
            {
                return NotFound();
            }

            return question;
        }

        [HttpPut("{id}")]//edit exception
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult PutQuestion(int id, Question question)
        {
            if (id != question.QuestionId)
            {
                return BadRequest();
            }

            _context.Entry(question).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuestionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult PostQuestion(Question question)
        {
            if (_context.Questions == null)
            {
                return Problem("Entity set 'OnlineDbContext.Questions'  is null.");
            }
            _context.Questions.Add(question);
            _context.SaveChanges();

            return CreatedAtAction("GetQuestion", new { id = question.QuestionId }, question);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult DeleteQuestion(int id)
        {
            if (_context.Questions == null)
            {
                return NotFound();
            }
            var question = _context.Questions.Find(id);
            if (question == null)
            {
                return NotFound();
            }

            _context.Questions.Remove(question);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpDelete("all")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult DeleteAllQuestions()
        {
            _context.Questions.RemoveRange(_context.Questions);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpDelete("byCourseAndLevel/{course}/{level}")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult DeleteQuestionsByCourseAndLevel(string course, string level)
        {
            // Retrieve questions from the database based on the specified course and level
            var questionsToDelete = _context.Questions
                .Where(q => q.Course.CourseName == course && q.Level == level)
                .ToList();
            // Check if there are any questions to delete
            if (questionsToDelete.Any())
            {
                // Remove the selected questions from the database
                _context.Questions.RemoveRange(questionsToDelete);
                // Save changes to the database
                _context.SaveChanges();
                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }

        private bool QuestionExists(int id)
        {
            return (_context.Questions?.Any(e => e.QuestionId == id)).GetValueOrDefault();
        }

        [HttpPost("upload-file")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult UploadFile(IFormFile fileinput)
        {
            // Check if no file or an empty file is uploaded
            if (fileinput == null || fileinput.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                // Use the EPPlus library to work with Excel files
                using (var package = new ExcelPackage(fileinput.OpenReadStream()))
                {
                  

                    var worksheet = package.Workbook.Worksheets[0]; // Assuming the data is in the first sheet

                    var _questionList = new List<Question>();// Create a list to store Question objects

                    for (int row = 2; row <= worksheet.Dimension.Rows; row++)
                    {
                        // Fetch CourseId based on CourseName
                        string courseName = worksheet.Cells[row, 1].Text;
                        Course course = _context.Courses.FirstOrDefault(c => c.CourseName == courseName);//refers the table of the course
                        if (course == null)
                        {
                            // Handle the case where the CourseName is not found
                            return BadRequest($"Course with name '{courseName}' not found.");
                        }
                        // Create a new Question object with data from the Excel sheet
                        var newRecord = new Question
                        {
                            // Skip QuestionId, as it's an identity column and will be generated by the database

                            CourseId = course.CourseId, // Use the fetched CourseId
                            Questions = worksheet.Cells[row, 2].Text,
                            Option1 = worksheet.Cells[row, 3].Text,
                            Option2 = worksheet.Cells[row, 4].Text,
                            Option3 = worksheet.Cells[row, 5].Text,
                            Option4 = worksheet.Cells[row, 6].Text,
                            CorrectAnswer = worksheet.Cells[row, 7].Text,
                            Level = worksheet.Cells[row, 8].Text,

                        };

                        _questionList.Add(newRecord);
                    }

                    // Save records to the database
                    _context.Questions.AddRange(_questionList);//storing it
                    _context.SaveChanges();//saving it

                    return Ok("Excel file uploaded and data saved to the database.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
