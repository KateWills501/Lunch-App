﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lunch_App.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Lunch_App.Logic
{
    public static class FilterLogic
    {
        public static List<int> Filter(List<ResturantFilterModel> resturants, List<SurveyFilterModel> surveys, ApplicationDbContext db)
        {
            var surveyTotal = CombineSurveys(surveys, db);

            var passingResturants = resturants.Where(r =>
            RestaurantMeetsDietaryNeeds(surveyTotal, r)
            && CuisineWanted(r, surveyTotal)
            && ResturantOpen(r.HoursOfOperation, surveyTotal.LunchTime)
            && AcceptableLocation(r, surveyTotal)).ToList();


            if (passingResturants.Count == 0)
            {
                passingResturants = resturants.Where(r =>
                RestaurantMeetsDietaryNeeds(surveyTotal, r)
                && ResturantOpen(r.HoursOfOperation, surveyTotal.LunchTime)
                && AcceptableLocation(r, surveyTotal)).ToList();
            }

            if (passingResturants.Count == 0)
            {
                passingResturants = resturants.Where(x => x.Id == 1).ToList();
            }

            return Rank(passingResturants, surveyTotal);
        }

        private static bool AcceptableLocation(ResturantFilterModel r, SurveyTotal surveyTotal)
        {
            return surveyTotal.ZipCodes.Contains(r.LocationZip);
        }

        private static bool CuisineWanted(ResturantFilterModel r, SurveyTotal surveyTotal)
        {
            return !surveyTotal.NotWantedCuisines.Contains(r.CuisineType);
        }

        public static bool RestaurantMeetsDietaryNeeds(SurveyTotal surveyTotal, ResturantFilterModel r)
        {
            return ((surveyTotal.DietaryIssues & r.DietaryOptions) == surveyTotal.DietaryIssues);
        }

        private static List<int> Rank(List<ResturantFilterModel> resturants, SurveyTotal surveyTotal)
        {
            foreach (var r in resturants)
            {
                if (surveyTotal.SuggestedResturantIds.Contains(r.Id))
                {
                    r.Score += surveyTotal.SuggestedResturantIds.GroupBy(x => x, v => v).Count(p => p.Key == r.Id) * 50;
                }
                if (surveyTotal.WantedCuisines.Contains(r.CuisineType))
                {
                    r.Score += surveyTotal.WantedCuisines.GroupBy(x => x, v => v).Count(p => p.Key == r.CuisineType) * 25;
                }
                if (surveyTotal.BaseZips.Contains(r.LocationZip))
                {
                    r.Score += surveyTotal.BaseZips.GroupBy(x => x, v => v).Count(p => p.Key == r.LocationZip) * 15;
                }
            }

            var result = resturants.OrderByDescending(x => x.Score).Select(v => v.Id).ToList();

            return result;
        }

        public static bool ResturantOpen(string hoursOfOperation, DateTime lunchTime)
        {
            var dateRanges = BreakHoursToRanges(hoursOfOperation);

            return dateRanges.Where(dr => lunchTime.DayOfWeek == dr.DayOfWeek)
                .Any(dr => lunchTime.Hour >= dr.Open.Hour && lunchTime.AddHours(1).Hour <= dr.Close.Hour);
        }

        public static IEnumerable<HoursOfOperations> BreakHoursToRanges(string hoursOfOperation)
        {
            var parsed = hoursOfOperation.Split(',');

            return parsed.SelectMany(s => HoursOfOperations.Parse(s.TrimStart())).ToList();
        }


        public static SurveyTotal CombineSurveys(List<SurveyFilterModel> surveys, ApplicationDbContext db)
        {
            surveys.RemoveAll(x => x.IsComing == false);
            var result = new SurveyTotal { DietaryIssues = 0 };
            if (surveys.Count == 0)
            {
                return result;
            }

            foreach (var s in surveys)
            {
                result.PossibleZips.AddRange(FindZipCodes(s.ZipCode, s.ZipCodeRadius, db));
                result.NotWantedCuisines.Add(s.CuisineNotWanted);
                result.WantedCuisines.Add(s.CuisineWanted);
                result.SuggestedResturantIds.Add(s.SuggestedResturantId);
                result.DietaryIssues = result.DietaryIssues | s.DietaryIssues;
                result.BaseZips.Add(s.ZipCode);
            }

            result.ZipCodes = result.PossibleZips.GroupBy(z => z, z => z)
                .Where(g => g.Count() == surveys.Count()).Select(g => g.Key).ToList();




            //TODO: result.LunchTime complicated logic

            result.LunchTime = surveys.First().TimeAvailable;

            return result;
        }

        public static IEnumerable<string> FindZipCodes(string zipCode, ZipCodeRadiusOption zipCodeRadius, ApplicationDbContext db)
        {
            var cache = db.ZipCache.Where(x => x.Zip == zipCode && x.Radius == (int)zipCodeRadius);
            if (cache.Count() != 0)
            {
                var zipString = "";
                foreach (var c in cache)
                {
                    zipString += c.ZipsInRadius;
                }

                var zipsFromDB = zipString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                return zipsFromDB;
            }

            var zips = new List<string>();
            try
            {
                var client = new RestClient("http://www.zipcodeapi.com/rest/gVIuqtTOYNwxZcBKnRVDJtIZF6HAoOlFJ4aay4ZwRv4RHFB4xxIg0qfGwXgVLBBv");

                var request = new RestRequest(
                    $"/radius.json/{zipCode}/{(int)zipCodeRadius}/mile", Method.GET);

                var response = client.Execute(request);

                var content = (JObject)JsonConvert.DeserializeObject(response.Content);

                zips = content["zip_codes"].ToObject<List<ZipsFromAPI>>().Select(x => x.zip_code).ToList();

                var totalZipString = zips.Aggregate("", (current, z) => current + (z + " "));
                db.ZipCache.Add(new ZipCache() { Radius = (int)zipCodeRadius, Zip = zipCode, ZipsInRadius = totalZipString });
                db.SaveChanges();
            }
            catch (Exception)
            {
                zips.Add(zipCode);
            }


            return zips;
        }

        public class ZipsFromAPI
        {
            public string zip_code { get; set; }
        }
    }

}