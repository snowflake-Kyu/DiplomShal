# SchoolApi

Готовый ASP.NET Core Web API для базы данных `KP_2024_Shalamov` из SQL-файла.

## Что внутри

- ASP.NET Core Web API на .NET 8.
- Entity Framework Core + SQL Server.
- Swagger для проверки запросов в браузере.
- CRUD-эндпоинты: GET, POST, PUT, DELETE.
- Подключение к существующей базе SQL Server.

## Таблицы и маршруты

| Таблица | HTTP route |
|---|---|
| AcademicYear | `/api/academicyears` |
| Auditorium | `/api/auditoriums` |
| ClubActivity | `/api/clubactivities` |
| Discipline | `/api/disciplines` |
| EducationalProgram | `/api/educationalprograms` |
| GraduatingClass | `/api/graduatingclasses` |
| Rate | `/api/rates` |
| Role | `/api/roles` |
| Schedule | `/api/schedules` |
| SchoolClass | `/api/schoolclasses` |
| StudentGroup | `/api/studentgroups` |
| Teacher | `/api/teachers` |
| TeachingAssigmentHistory | `/api/teachingassigmenthistories` |
| TeachingAssignment | `/api/teachingassignments` |
| TimeSlot | `/api/timeslots` |
| User | `/api/users` |
| Vote | `/api/votes` |

## Как запустить

1. Открой SQL Server Management Studio.
2. Создай базу данных `KP_2024_Shalamov`, если её ещё нет.
3. Выполни файл `database.sql`.
4. Открой проект `SchoolApi` в Visual Studio или Rider.
5. Проверь строку подключения в `appsettings.json`.

Для Windows-аутентификации:

```json
"DefaultConnection": "Server=localhost;Database=KP_2024_Shalamov;Trusted_Connection=True;TrustServerCertificate=True;"
```

Для SQL Server-аутентификации:

```json
"DefaultConnection": "Server=localhost;Database=KP_2024_Shalamov;User Id=sa;Password=your_password;TrustServerCertificate=True;"
```

6. Запусти проект командой:

```bash
dotnet restore
dotnet run
```

7. Открой Swagger:

```text
http://localhost:5000/swagger
```

## Примеры запросов

Получить все записи:

```http
GET http://localhost:5000/api/teachers
```

Получить одну запись:

```http
GET http://localhost:5000/api/teachers/1
```

Добавить запись:

```http
POST http://localhost:5000/api/teachers
Content-Type: application/json

{
  "FullName": "Иванов Иван Иванович",
  "Email": "ivanov@example.com"
}
```

Изменить запись:

```http
PUT http://localhost:5000/api/teachers/1
Content-Type: application/json

{
  "TeacherId": 1,
  "FullName": "Иванов Иван Петрович",
  "Email": "ivanov.new@example.com"
}
```

Удалить запись:

```http
DELETE http://localhost:5000/api/teachers/1
```

## Важное замечание

В некоторых таблицах первичный ключ не является `IDENTITY`, например `AcademicYear`, `Rate`, `Role`, `User`, `GraduatingClass`, `TeachingAssigmentHistory`. Для таких таблиц при POST нужно передавать ID вручную, потому что база данных сама его не генерирует.
