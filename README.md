# Hacker News Scraper

A C# console app that scrapes the first 30 entries from [Hacker News](https://news.ycombinator.com/), shows each entry's **number, title, points and comment count**, and can filter them by title length. Every run is logged to a local SQLite database.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Git

Check your SDK version (it should start with `10.`):

```bash
dotnet --version
```

## Getting started

Fork the repository on GitHub, then clone your fork:

```bash
git clone https://github.com/juangranda/sb-web-scraper.git
cd sb-web-scraper
```

No separate install step is needed: the first time you run one of the `dotnet run` command below, it restores NuGet packages and builds the project.

## Usage

Run every command from the repository root.

| Command | Output |
|---|---|
| `dotnet run --project scraper-app` | All 30 entries |
| `dotnet run --project scraper-app -- long-titles` | Titles with **more than 5 words**, sorted by comments (highest first) |
| `dotnet run --project scraper-app -- short-titles` | Titles with **5 or fewer words**, sorted by points (highest first) |
| `dotnet run --project scraper-app -- history` | The last 20 recorded runs |

Example:

```

Filter: short-titles
  #  Points  Comments  Words  Title
--------------------------------------------------------------------------------
 20     949       528      2  PHP 8.2.35
 16     457       276      5  EA is shutting down BioWare
```

### How words are counted

A title is split on whitespace, and only tokens containing at least one letter or digit count as words. Tokens made only of symbols are ignored, so `This is - a self-explained example` has **5** words.

## Usage tracking

Each scrape (including failed ones, such as an unknown filter) is saved as a row in `data/usage.db`, created under the folder you run the command from.

| Column | Meaning |
|---|---|
| `timestamp_utc` | When the request was made |
| `filter` | Filter requested (`none`, `long-titles`, `short-titles`, or whatever was typed) |
| `success`, `error` | Whether the run succeeded, and the error message if not |
| `http_status` | HTTP status returned by Hacker News |
| `entries_scraped`, `entries_returned` | Entries parsed from the page, and entries left after filtering |
| `response_bytes` | Size of the downloaded page |
| `fetch_ms`, `duration_ms` | Download time and total run time |

View it with the `history` command, or with the `sqlite3` CLI:

```bash
sqlite3 -header -column data/usage.db "SELECT * FROM usage ORDER BY id DESC;"
```

Database files are git-ignored.

## Tests

```bash
dotnet test
```

The xUnit tests in `scraper-app.Tests/` cover a few services. They use hand-written fakes and never call the real website; the repository tests use a temporary SQLite file.

## Debugging in VS Code

Open the repository root in VS Code with the C# extension installed. `.vscode/launch.json` has one debug configuration per command (`Scraper: history`, `Scraper: long-titles`, ...): pick one in **Run and Debug** and press **F5**. `.vscode/tasks.json` provides `build` and `test` tasks.

## Project structure

```
scraper-app/
├── Program.cs        Composition root: registers services with dependency injection
├── Application/      ScraperApplication (runs a command)
├── CrawlServices/    Fetching the page (HttpClient) and parsing entries (uses HtmlAgilityPack)
├── Filters/          IEntryFilter implementations and the FilterCatalog that looks them up by name
├── Models/           Entry, ScrapeRecord and CrawlResult records
├── PrintServices/    Console output
├── Storage/          SQLite usage repository
└── TextServices/     Word counting
scraper-app.Tests/    xUnit tests, mirroring the folders above
```

Each service sits behind an interface, so new filters or storage backends can be added without changing the rest of the app. To add a filter, implement `IEntryFilter` and register it in `Program.cs`.
