using FImonBot.Game.QuizQuestions;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FImonBot.Game
{
    public class QuizManager
    {
        private static ConcurrentDictionary<ulong, Question> mapping = new ConcurrentDictionary<ulong, Question>();
        private static IMongoCollection<Question> collection = null;
        private const string collectionName = "Questions";

        public void LoadQuestions()
        {
            if (collection == null) { return; }
           
            var questionsReceived = collection.Find(s => true).ToList();
            Console.WriteLine("got questions");

            foreach (var question in questionsReceived)
            {
                mapping.AddOrUpdate(question.QuestionID, question, (ID, question) => question);
            }
        }

        public void AddQuestion(ulong quesID, string question, string correctOption, int expReward, params string[] options)
        {
            if (collection == null)
            {
                Console.WriteLine("no database");
                return;
            }

            if (mapping.ContainsKey(quesID))
            {
                Console.WriteLine("already have this ID");
                return;
            }

            var newQuestion = new Question(quesID, question, correctOption, expReward, options);

            Console.WriteLine("Adding Question");

            mapping.AddOrUpdate(newQuestion.QuestionID, newQuestion, (ID, question) => question);

            collection.InsertOne(newQuestion);
            Console.WriteLine("Question added");
        }

        public static Question GetRandomQuestion()
        {
            if (mapping.Count() == 0)
            {
                return null;
            }

            Random rand = new Random();
            var questionIndex = rand.Next(0, mapping.Count() - 1);
            return mapping.Values.ElementAt(questionIndex);
        }

        internal static void SetCollection(IMongoDatabase database)
        {
            collection = database.GetCollection<Question>(collectionName);
        }
    }
}
