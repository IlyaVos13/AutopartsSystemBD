# AutopartsSystemBD

Информационная система учета закупок автозапчастей.

Используемые технологии

- C#
- Windows Forms
- PostgreSQL
- Npgsql

Как запустить проект

1. Установить PostgreSQL.
2. Создать базу данных с названием `autoparts_db`.
3. Выполнить SQL-запросы из файла `database.sql`.
4. Открыть проект в Visual Studio.
5. В файле `DataStore.cs` указать свой пароль от PostgreSQL в строке подключения:

```csharp
Host=localhost;Port=5432;Database=autoparts_db;Username=postgres;Password=your_password
