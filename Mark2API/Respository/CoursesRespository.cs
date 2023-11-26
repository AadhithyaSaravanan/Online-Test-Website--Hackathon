using Mark2API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

public class CoursesRepository : ICoursesRepository
{
    private readonly OnlineDbContext _context;

    public CoursesRepository(OnlineDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Course> GetCourses()
    {
        return _context.Courses.ToList();
    }

    public Course GetCourseById(int id)
    {
        return _context.Courses.Find(id);
    }

    public void UpdateCourse(int id, Course course)
    {
        if (id != course.CourseId)
        {
            throw new InvalidOperationException("Invalid course ID");
        }

        _context.Entry(course).State = EntityState.Modified;

        try
        {
            _context.SaveChanges();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CourseExists(id))
            {
                throw new InvalidOperationException("Course not found");
            }
            else
            {
                throw;
            }
        }
    }

    public void CreateCourse(Course course)
    {
        if (_context.Courses == null)
        {
            throw new InvalidOperationException("Entity set 'OnlineDbContext.Courses' is null.");
        }

        _context.Courses.Add(course);
        _context.SaveChanges();
    }

    public void DeleteCourse(int id)
    {
        var course = _context.Courses.Find(id);
        if (course == null)
        {
            throw new InvalidOperationException("Course not found");
        }

        var associatedQuestions = _context.Questions.Where(q => q.Course.CourseId == id);
        _context.Questions.RemoveRange(associatedQuestions);

        _context.Courses.Remove(course);
        _context.SaveChanges();
    }

    public bool CourseExists(int id)
    {
        return _context.Courses.Any(e => e.CourseId == id);
    }

    public IEnumerable<Question> GetLevelOneQuestions(string courseName)
    {
        return _context.Questions
            .Where(q => q.Level == "Level1" && q.Course.CourseName == courseName)
            .ToList();
    }

    public IEnumerable<Question> GetLevelTwoQuestions(string courseName)
    {
        return _context.Questions
            .Where(q => q.Level == "Level2" && q.Course.CourseName == courseName)
            .ToList();
    }

    public IEnumerable<Question> GetLevelThreeQuestions(string courseName)
    {
        return _context.Questions
            .Where(q => q.Level == "Level3" && q.Course.CourseName == courseName)
            .ToList();
    }
}
