# Habeas

> **Work in progress.** Habeas is a personal health and activity tracking assistant. It is designed to collect data about a person's physical activity, nutrition, body metrics, and other lifestyle indicators — and make that data available to LLM-based assistants for analysis, insights, and personalized recommendations.

Built with .NET 10, clean architecture (DDD), PostgreSQL via EF Core, and a Telegram bot interface.

## Database & Migrations

PostgreSQL schema management via EF Core is covered in a separate guide:
[docs/ef-migrations.md](./docs/ef-migrations.md).

## Running

```bash
# set bot token: src/Habeas.Bot/appsettings.json -> Telegram:BotToken
dotnet build Habeas.sln
dotnet test  Habeas.sln
dotnet run --project src/Habeas.Bot
```

## Bot Commands

`/start` — register · `/body <height_cm> <weight_kg>` — log height and weight ·
`/me` — show profile and BMI · `/help`

## License

[Apache License 2.0](./LICENSE).
