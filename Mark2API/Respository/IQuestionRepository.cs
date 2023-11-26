using Mark2API.Models;
using System.Collections.Generic;

public interface IQuestionRepository
{
    IEnumerable<Question> GetQuestions();
    Question GetQuestionById(int id);
    void CreateQuestion(Question question);
    void UpdateQuestion(int id, Question question);
    void DeleteQuestion(int id);
    void DeleteAllQuestions();
    void DeleteQuestionsByCourseAndLevel(string course, string level);
    void CreateQuestionsBulk(List<Question> questions);
    bool QuestionExists(int id); // Add this method

}
