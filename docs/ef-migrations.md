# Гайд по миграциям EF Core

Миграция — это сгенерированный класс, который описывает, как привести схему базы данных
в соответствие с текущей доменной моделью (создать таблицы, добавить колонки и т.д.).
Файлы миграций лежат в `src/Habeas.Infrastructure/Persistence/Migrations`.

## Как тут всё устроено

- **`--project`** (где живут миграции и `DbContext`) — `src/Habeas.Infrastructure`.
- **`--startup-project`** (откуда берутся конфигурация и строка подключения) — `src/Habeas.Bot`.

Оба параметра нужно указывать в каждой команде, потому что `DbContext` и точка входа
находятся в разных проектах.

Строка подключения берётся из `src/Habeas.Bot/appsettings.json` → `ConnectionStrings:Postgres`.

## 0. Установка инструмента (один раз)

```bash
dotnet tool install --global dotnet-ef
# обновить, если уже установлен:
dotnet tool update --global dotnet-ef
# проверить:
dotnet ef --version
```

## 1. Поднять базу данных

`add` (создание файла миграции) **не** требует запущенной базы.
`database update` (применение к базе) — требует. Поднять PostgreSQL можно через
Docker Compose (файл в корне репозитория):

```bash
docker compose up -d
```

Креды и имя базы (`habeas`/`habeas`/`habeas`, порт 5432) совпадают со строкой
подключения в `src/Habeas.Bot/appsettings.json`.

## 2. Создать миграцию

После любого изменения модели (новая сущность, новое поле, изменение конфигурации в
`Persistence/Configurations`) генерируем миграцию с понятным именем:

```bash
dotnet ef migrations add ИмяМиграции \
  --project src/Habeas.Infrastructure \
  --startup-project src/Habeas.Bot \
  --output-dir Persistence/Migrations
```

Пример: `dotnet ef migrations add AddWeightHistory`.

EF создаст три файла: `<timestamp>_ИмяМиграции.cs` (методы `Up`/`Down`),
`*.Designer.cs` и обновит `HabeasDbContextModelSnapshot.cs`. **Открой `Up()` и проверь,
что сгенерированный SQL соответствует ожиданиям**, прежде чем применять.

## 3. Применить миграцию к базе

```bash
dotnet ef database update \
  --project src/Habeas.Infrastructure \
  --startup-project src/Habeas.Bot
```

Применяет все ещё не применённые миграции. Команда идемпотентна: повторный запуск без
новых миграций ничего не делает.

## Откатить или переделать

```bash
# посмотреть список миграций (применённые помечены)
dotnet ef migrations list --project src/Habeas.Infrastructure --startup-project src/Habeas.Bot

# удалить ПОСЛЕДНЮЮ миграцию (только если она ещё НЕ применена к базе)
dotnet ef migrations remove --project src/Habeas.Infrastructure --startup-project src/Habeas.Bot

# откатить базу до конкретной миграции (применит Down у более поздних)
dotnet ef database update ИмяМиграции \
  --project src/Habeas.Infrastructure --startup-project src/Habeas.Bot

# откатить вообще все миграции (пустая схема)
dotnet ef database update 0 \
  --project src/Habeas.Infrastructure --startup-project src/Habeas.Bot
```

> Если миграция уже применена к базе, не удаляй её файл вручную — сначала откати базу
> через `database update <предыдущая>`, потом `migrations remove`.

## SQL-скрипт для продакшена

На проде обычно не запускают `dotnet ef` напрямую, а генерируют идемпотентный SQL и
прогоняют его через свой механизм деплоя:

```bash
dotnet ef migrations script --idempotent \
  --project src/Habeas.Infrastructure \
  --startup-project src/Habeas.Bot \
  --output migrate.sql
```

## Типичный цикл при изменении модели

1. Изменил сущность или конфигурацию в `Habeas.Infrastructure/Persistence/Configurations`.
2. `dotnet ef migrations add ОписаниеИзменения` (см. шаг 2).
3. Проверил сгенерированный `Up()`.
4. `dotnet ef database update` (см. шаг 3).
