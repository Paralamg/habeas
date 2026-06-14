# Работа с секретами локально

Секреты (токены, пароли к БД) **никогда не должны попадать в `appsettings.json`**.
Для локальной разработки используется механизм **User Secrets** — встроенный в .NET способ
хранить чувствительные значения вне репозитория.

Секреты хранятся в `~/.microsoft/usersecrets/<guid>/secrets.json` и автоматически
подмешиваются поверх `appsettings.json` при запуске в режиме `Development`.

## 0. Инициализация (один раз на проект)

```bash
dotnet user-secrets init --project src/Habeas.Bot
```

Команда добавляет `<UserSecretsId>` в `Habeas.Bot.csproj`. Коммитить это можно — это просто GUID-идентификатор, не сам секрет.

## 1. Добавить секреты

```bash
dotnet user-secrets set "Database:Password" "<пароль>" --project src/Habeas.Bot
dotnet user-secrets set "Database:Username" "<пользователь>" --project src/Habeas.Bot
dotnet user-secrets set "Telegram:BotToken" "<токен>" --project src/Habeas.Bot
```

## 2. Проверить сохранённые секреты

```bash
dotnet user-secrets list --project src/Habeas.Bot
```

## 3. Что должно быть в appsettings.json

Только безопасные дефолты без реальных значений:

```json
{
  "Database": {
    "Host": "localhost",
    "Port": 5432,
    "Name": "habeas",
    "Username": "",
    "Password": ""
  },
  "Telegram": {
    "BotToken": ""
  }
}
```