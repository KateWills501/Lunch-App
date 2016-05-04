﻿using System;

namespace Lunch_App.Models
{
    public class Survey
    {
        public int Id { get; set; }
        public virtual LunchUser User { get; set; }
        public virtual Lunch Lunch { get; set; }
        public bool IsFinished { get; set; }
        public DateTime TimeAvailable { get; set; }
        public int MinutesAvailiable { get; set; }
        public int ZipCode { get; set; }
        public int ZipCodeRadius { get; set; }
        public Cuisine CuisineWanted { get; set; }
        public Cuisine CuisineNotWanted { get; set; }
        public Resturant SuggestedResturant { get; set; }
        public int DiataryIssues { get; set; }
    }

    public class SurveyFilterModel
    {

        public SurveyFilterModel(Survey s)
        {
            TimeAvailable = s.TimeAvailable;
            ZipCode = s.ZipCode;
            ZipCodeRadius = s.ZipCodeRadius;
            CuisineWanted = s.CuisineWanted;
            CuisineNotWanted = s.CuisineNotWanted;
            SuggestedResturantId = s.SuggestedResturant.Id;
            DiataryIssues = s.DiataryIssues;
        }

       public DateTime TimeAvailable { get; set; }
       // public int MinutesAvailiable { get; set; }
        public int ZipCode { get; set; }
        public int ZipCodeRadius { get; set; }
        public Cuisine CuisineWanted { get; set; }
        public Cuisine CuisineNotWanted { get; set; }
        public int SuggestedResturantId { get; set; }
        public int DiataryIssues { get; set; }
    }

    public enum DiataryIssues
    {
        Vegan = 1,
        Vegetarian = 2,
        GlutenFree = 4,
        NutAllergy = 8,
        ShellFishAllergy = 16,
        Kosher = 32,
        Halaal = 64,
        LactoseIntolerant = 128
    }

}