using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScheduleParser
{
    class Program
    {
        static void Main(string[] args)
        {
            HtmlDocument html = GetHtmlDoc();
            var table = html.DocumentNode.SelectSingleNode("//table[@class='session']/tbody");
            List<SessionInfo> sessions = new List<SessionInfo>();
            List<string> movieNames = new List<string>();

            foreach (var tr in table.ChildNodes)
            {
                string text = tr.InnerHtml;
                if (String.IsNullOrWhiteSpace(text))
                    continue;

                SessionInfo commonSession = new SessionInfo();
                string movieName = tr.SelectSingleNode("td[@class='session-col-1']").InnerText;
                if (movieName.EndsWith("(3D)"))
                {
                    movieName = movieName.Substring(0, movieName.Length - 5);
                    commonSession.Is3D = true;
                }
                commonSession.MovieName = movieName;

                AddMovieName(movieNames, movieName);

                string placeName = tr.SelectSingleNode("td[@class='session-col-2-3']").InnerText;
                commonSession.PlaceName = CorrectPlaceName(placeName);

                var timesList = tr.SelectSingleNode("td[@class='session-col-4']/ul");
                foreach (var time in timesList.ChildNodes)
                {
                    if (String.IsNullOrWhiteSpace(time.InnerText))
                        continue;

                    string[] timeParts = time.InnerText.Split(':');

                    SessionInfo session = commonSession.Clone();
                    session.Time = new DateTime(2000, 1, 1, Int32.Parse(timeParts[0]), Int32.Parse(timeParts[1]), 0);
                    sessions.Add(session);
                }
            }

            File.Delete(@"D:\1.csv");
            PrintSessions(sessions, movieNames);

            Console.ReadKey();
        }

        private static void PrintSessions(List<SessionInfo> sessions, List<string> movieNames)
        {
            foreach (var movieName in movieNames)
            {
                var movieSessions = sessions.Where(s => s.MovieName == movieName).OrderBy(s => s.Time).GroupBy(s => s.Time).Select(group => new { Time = group.Key, Places = group.ToList() });

                Console.WriteLine("*{0}*", movieName);
                File.AppendAllText(@"D:\1.csv", String.Format("*{0}*", movieName) + Environment.NewLine);

                foreach (var session in movieSessions)
                {
                    Console.WriteLine("{0}\t{1}", session.Time.ToString("HH:mm"), GetPlacesString(session.Places));
                    File.AppendAllText(@"D:\1.csv", String.Format("{0};{1}", session.Time.ToString("HH:mm"), GetPlacesString(session.Places)) + Environment.NewLine);
                }
            }
        }

        private static string GetPlacesString(List<SessionInfo> sessions)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var session in sessions.OrderBy(s => s.PlaceName))
            {
                sb.Append(session.PlaceName);

                if (session.Is3D)
                    sb.Append("(3D)");

                sb.Append(", ");
            }

            return sb.ToString().Substring(0, sb.Length - 2);
        }

        private static void AddMovieName(List<string> movieNames, string movieName)
        {
            if (movieNames.Contains(movieName))
                return;

            movieNames.Add(movieName);
        }

        private static string CorrectPlaceName(string placeName)
        {
            switch (placeName)
            {
                case "Синема Стар":
                    return "21 век";
                case "Синема Стар Рио":
                    return "РИО";
                default:
                    return placeName;
            }
        }

        private static HtmlDocument GetHtmlDoc()
        {
            string text = File.ReadAllText(@"D:\1.html", Encoding.UTF8);
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(text);
            return html;
        }
    }
}
