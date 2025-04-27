# Department Management System

A comprehensive system for managing students, courses, rooms, and exam seating arrangements.

## Prerequisites

- .NET 7.0 SDK or later
- MySQL Server 8.0 or later
- Visual Studio 2022 or VS Code

## Setup Instructions

1. Clone the repository
2. Update the database connection string in `appsettings.json` with your MySQL server details:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=DepartmentManagementDB;User=your_username;Password=your_password;Port=3306;AllowPublicKeyRetrieval=true;SslMode=none;Convert Zero Datetime=True;Allow User Variables=True;ConnectionTimeout=30"
   }
   ```

3. Open terminal in the project directory and run:
   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```

4. The application will automatically:
   - Create the database if it doesn't exist
   - Apply migrations
   - Seed initial data

5. Access the application at:
   - Main application: https://localhost:5001
   - Swagger API docs: https://localhost:5001/swagger

## Features

- Student Management
  - Add, edit, delete students
  - Automatic roll number generation
  - Student details tracking

- Course Management
  - Add, edit, delete courses
  - Course code and credit hours tracking

- Room Management
  - Add, edit, delete rooms
  - Track room capacity and availability

- Exam Management
  - Arrange exam seating
  - Generate seating plans
  - Create attendance sheets
  - PDF report generation

## Troubleshooting

If you encounter any issues:

1. Ensure MySQL server is running
2. Verify the connection string in appsettings.json
3. Check if the port numbers match your setup
4. Make sure you have the required .NET SDK version

## Support

For any issues or questions, please create an issue in the repository. 