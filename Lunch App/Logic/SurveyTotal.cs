using System;
using System.Collections.Generic;

namespace Lunch_App.Logic
{
    public class SurveyTotal
    {
        public DateTime LunchTime { get; set; }
        public List<int> ZipCodes { get; set; } = new List<int>();
        public List<Cuisine> WantedCuisines { get; set; } = new List<Cuisine>();
        public List<Cuisine> NotWantedCuisines { get; set; } = new List<Cuisine>();
        public List<int> SuggestedResturantIds { get; set; } = new List<int>();
        public int DiataryIssues { get; set; }
    }
}