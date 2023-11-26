using Mark2API.Models;
using System;
using System.Collections.Generic;

public interface ICoursesRepository
{
    IEnumerable<Course> GetCourses();
    Course GetCourseById(int id);
    void UpdateCourse(int id, Course course);
    void CreateCourse(Course course);
    void DeleteCourse(int id);
    bool CourseExists(int id);
    IEnumerable<Question> GetLevelOneQuestions(string courseName);
    IEnumerable<Question> GetLevelTwoQuestions(string courseName);
    IEnumerable<Question> GetLevelThreeQuestions(string courseName);
}
