using Mark2API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

public class QuestionRepository : IQuestionRepository
{
    private readonly OnlineDbContext _context;

    public QuestionRepository(OnlineDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Question> GetQuestions()
    {
        return _context.Questions.Include(q => q.Course).ToList();
    }

    public Question GetQuestionById(int id)
    {
        return _context.Questions.Find(id);
    }

    public void CreateQuestion(Question question)
    {
        _context.Questions.Add(question);
        _context.SaveChanges();
    }

    public void UpdateQuestion(int id, Question question)
    {
        if (id != question.QuestionId)
        {
            throw new InvalidOperationException("Invalid question ID");
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
                throw new InvalidOperationException("Question not found");
            }
            else
            {
                throw;
            }
        }
    }

    public void DeleteQuestion(int id)
    {
        var question = _context.Questions.Find(id);
        if (question == null)
        {
            throw new InvalidOperationException("Question not found");
        }

        _context.Questions.Remove(question);
        _context.SaveChanges();
    }

    public void DeleteAllQuestions()
    {
        _context.Questions.RemoveRange(_context.Questions);
        _context.SaveChanges();
    }
    public bool QuestionExists(int id)
    {
        return _context.Questions.Any(e => e.QuestionId == id);
    }

    public void DeleteQuestionsByCourseAndLevel(string course, string level)
    {
        var questionsToDelete = _context.Questions
            .Where(q => q.Course.CourseName == course && q.Level == level)
            .ToList();

        if (questionsToDelete.Any())
        {
            _context.Questions.RemoveRange(questionsToDelete);
            _context.SaveChanges();
        }
    }

    public void CreateQuestionsBulk(List<Question> questions)
    {
        _context.Questions.AddRange(questions);
        _context.SaveChanges();
    }

}
