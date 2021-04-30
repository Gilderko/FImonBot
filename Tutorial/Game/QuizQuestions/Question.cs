using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FImonBot.Game.QuizQuestions
{
    [BsonIgnoreExtraElements]
    public class Question
    {
        public Question(ulong quesID,string question, string correctOption, int expReward, params string[] options)
        {
            QuestionID = quesID;
            QuestionValue = question;
            CorrectOption = correctOption;
            ExpReward = expReward;
            Options = options;

            if (!options.Contains(correctOption))
            {
                throw new Exception();
            }
        }

        [BsonId]
        public ulong QuestionID { get; private set; }

        [BsonElement("question")]
        public string QuestionValue { get; private set; }

        [BsonElement("all_options")]
        public string[] Options { get; private set; }

        [BsonElement("correct_option")]
        public string CorrectOption { get; private set; }

        [BsonElement("experience_reward")]
        public int ExpReward { get; private set; }
    }
}
