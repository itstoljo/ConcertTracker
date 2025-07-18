using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spectre.Console;

namespace ConcertTracker
{
    // Represents a song played during the concert
    public class Song
    {
        public string Title { get; }
        public string Artist { get; }

        public Song(string title, string artist)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title cannot be null or empty.", nameof(title));
            if (string.IsNullOrWhiteSpace(artist)) throw new ArgumentException("Artist cannot be null or empty.", nameof(artist));
            Title = title;
            Artist = artist;
        }

        public override string ToString() => $"{Title} by {Artist}";
    }

    // Represents a concert event
    public class Concert
    {
        private readonly List<Song> _setlist = new List<Song>();
        private readonly List<string> _notes = new List<string>();

        public string VenueName { get; }
        public DateTime Date { get; }
        public IReadOnlyList<Song> Setlist => _setlist.AsReadOnly();
        public IReadOnlyList<string> Notes => _notes.AsReadOnly();

        public Concert(string venueName, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(venueName)) throw new ArgumentException("Venue name cannot be null or empty.", nameof(venueName));
            VenueName = venueName;
            Date = date;
        }

        public void AddSong(Song song)
        {
            if (song == null) throw new ArgumentNullException(nameof(song));
            _setlist.Add(song);
        }

        public void AddNote(string note)
        {
            if (string.IsNullOrWhiteSpace(note)) throw new ArgumentException("Note cannot be null or empty.", nameof(note));
            _notes.Add(note);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Concert at {VenueName} on {Date:yyyy-MM-dd}");
            sb.AppendLine("Setlist:");
            sb.AppendLine(_setlist.Count > 0
                ? string.Join("\n", _setlist.Select(s => $"  - {s}"))
                : "  (No songs)");
            sb.AppendLine("Notes:");
            sb.AppendLine(_notes.Count > 0
                ? string.Join("\n", _notes.Select(n => $"  - {n}"))
                : "  (No notes)");
            return sb.ToString();
        }
    }

    // Represents a user of the app
    public class User
    {
        private readonly List<Concert> _concertsAttended = new List<Concert>();

        public string Username { get; }
        public IReadOnlyList<Concert> ConcertsAttended => _concertsAttended.AsReadOnly();

        public User(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username cannot be null or empty.", nameof(username));
            Username = username;
        }

        public void AddConcert(Concert concert)
        {
            if (concert == null) throw new ArgumentNullException(nameof(concert));
            _concertsAttended.Add(concert);
        }

        public override string ToString()
        {
            return $"User: {Username}\nConcerts Attended: {_concertsAttended.Count}";
        }
    }

    class Program
    {
        static void Main()
        {
            AnsiConsole.Write(
                new FigletText("Concert Tracker")
                    .Centered()
                    .Color(Color.Cyan1)
            );

            AnsiConsole.MarkupLine("[grey]Track your concert experiences with ease![/]\n");

            User user = null;
            while (user == null)
            {
                var username = AnsiConsole.Ask<string>("Enter your [green]username[/]:");
                try
                {
                    user = new User(username);
                }
                catch (ArgumentException e)
                {
                    AnsiConsole.MarkupLine($"[red]{e.Message}[/]");
                }
            }

            bool exit = false;
            while (!exit)
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(
                    new FigletText("Concert Tracker")
                        .Centered()
                        .Color(Color.Cyan1)
                );
                AnsiConsole.MarkupLine($"[bold yellow]Welcome, {user.Username}![/]\n");

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select an option:")
                        .PageSize(4)
                        .AddChoices(new[]
                        {
                            "Add concert",
                            "View concerts",
                            "Exit"
                        })
                );

                switch (choice)
                {
                    case "Add concert":
                        AddConcert(user);
                        break;
                    case "View concerts":
                        ViewConcerts(user);
                        break;
                    case "Exit":
                        exit = true;
                        break;
                }
            }
        }

        static void AddConcert(User user)
        {
            string venue = AnsiConsole.Ask<string>("Enter [green]venue name[/]:");
            DateTime date = AnsiConsole.Ask<DateTime>("Enter the [green]date[/] (yyyy-mm-dd):");

            try
            {
                var concert = new Concert(venue, date);
                AddSetlistAndNotes(concert);
                user.AddConcert(concert);
                AnsiConsole.MarkupLine("[green]Concert added![/]");
                AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
                Console.ReadKey(true);
            }
            catch (ArgumentException ex)
            {
                AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
                AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
                Console.ReadKey(true);
            }
        }

        static void AddSetlistAndNotes(Concert concert)
        {
            AnsiConsole.MarkupLine("[bold underline]Adding songs to setlist:[/]");
            while (true)
            {
                string title = AnsiConsole.Prompt(
                    new TextPrompt<string>("Enter song [green]title[/] ([grey]leave blank to finish[/]):")
                        .AllowEmpty()
                ).Trim();

                if (string.IsNullOrWhiteSpace(title))
                    break;

                string artist;
                while (true)
                {
                    artist = AnsiConsole.Prompt(
                        new TextPrompt<string>("Enter [green]artist[/]:")
                            .AllowEmpty()
                    ).Trim();

                    if (!string.IsNullOrWhiteSpace(artist))
                        break;

                    AnsiConsole.MarkupLine("[red]Artist name cannot be empty. Please enter a valid artist.[/]");
                }

                try
                {
                    concert.AddSong(new Song(title, artist));
                    AnsiConsole.MarkupLine($"[green]Added song:[/] \"{title}\" by {artist}\n");
                }
                catch (ArgumentException ex)
                {
                    AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
                }
            }

            AnsiConsole.MarkupLine("[bold underline]\nAdd notes for this concert:[/]");
            while (true)
            {
                string note = AnsiConsole.Prompt(
                    new TextPrompt<string>("Enter [green]note[/] ([grey]leave blank to finish[/]):")
                        .AllowEmpty()
                ).Trim();

                if (string.IsNullOrWhiteSpace(note))
                    break;
                concert.AddNote(note);
                AnsiConsole.MarkupLine("[green]Note added![/]\n");
            }
       
        }

        static void ViewConcerts(User user)
        {
            if (user.ConcertsAttended.Count == 0)
            {
                AnsiConsole.MarkupLine("[italic grey]No concerts recorded yet.[/]");
                AnsiConsole.MarkupLine("[grey]Press any key to return to menu...[/]");
                Console.ReadKey(true);
                return;
            }

            var choices = user.ConcertsAttended
                .Select((c, i) => $"{i + 1}. {c.VenueName} ({c.Date:yyyy-MM-dd})")
                .Append("< Return to menu >")
                .ToList();

            string selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select a concert to [green]view[/green]:")
                    .PageSize(10)
                    .AddChoices(choices)
            );

            int idx = selected.StartsWith("<") ? -1 : choices.IndexOf(selected);

            if (idx == -1 || idx >= user.ConcertsAttended.Count)
                return;

            var selectedConcert = user.ConcertsAttended[idx];

            AnsiConsole.Clear();
            AnsiConsole.Write(
                new Panel($"[bold]{selectedConcert.VenueName}[/]\n[grey]{selectedConcert.Date:yyyy-MM-dd}[/]")
                    .Header("Concert Details", Justify.Center)
                    .Border(BoxBorder.Rounded)
                    .BorderStyle(new Style(Color.Cyan1))
                    .Padding(1, 1)
            );

            var table = new Table()
                .Title("[yellow]Setlist[/]")
                .AddColumn("Title")
                .AddColumn("Artist");

            if (selectedConcert.Setlist.Count > 0)
            {
                foreach (var song in selectedConcert.Setlist)
                    table.AddRow(song.Title, song.Artist);
            }
            else
            {
                table.AddRow("[grey](No songs)[/]", "");
            }
            AnsiConsole.Write(table);

            var notesPanel = selectedConcert.Notes.Count > 0
                ? "[italic]" + string.Join("\n", selectedConcert.Notes.Select(n => $"[blue]•[/] {n}")) + "[/]"
                : "[grey](No notes)[/]";

            AnsiConsole.Write(new Panel(notesPanel)
                .Header("Notes", Justify.Left)
                .Border(BoxBorder.Ascii)
                .BorderStyle(new Style(Color.Grey)));

            AnsiConsole.MarkupLine("[grey]\nPress any key to return...[/]");
            Console.ReadKey(true);
        }
    }
}
