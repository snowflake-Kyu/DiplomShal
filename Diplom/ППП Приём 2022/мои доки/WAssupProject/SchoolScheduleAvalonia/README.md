# SchoolScheduleAvalonia

Avalonia UI десктопное приложение для работы с расписанием через ASP.NET Core Web API.

## API

Перед запуском приложения должен быть запущен API:

```text
http://localhost:54040/api
```

Адрес API можно изменить в `appsettings.json`.

## Запуск из VS Code

1. Откройте папку `SchoolScheduleAvalonia` в VS Code.
2. Установите расширение C# Dev Kit или C#.
3. Откройте раздел Run and Debug.
4. В списке конфигураций выберите `Запустить Avalonia`.
5. Нажмите F5.

Конфигурация запуска находится в `.vscode/launch.json`, а задача сборки в `.vscode/tasks.json`.

## Запуск через терминал

```bash
dotnet restore
dotnet run
```
