### Key Features of This TaskMenger_Api:
1. **Clear Structure** - Separates authentication, company, employee, and project/task endpoints
2. **Visual Badges** - Includes Swagger UI badge for quick recognition
3. **Model Documentation** - Shows key entity structures
4. **Step-by-Step Setup** - From cloning to running migrations
5. **Example Requests** - Ready-to-use HTTP snippets
6. **Authentication Flow** - Clear JWT usage instructions

 Authentication Flow
 
Register user → /api/Auth/register

Login → /api/Auth/login (returns JWT token)

Use token in Authorization: Bearer [token] header

Refresh token → /api/Auth/refresh-token

Installation

 Clone the repository:
   1: git clone https://github.com/husseinyar/TaskMenger_Api.git
    
 2: Configure the database connection in appsettings.json
 
 Run migrations:
  * dotnet ef database update

  Run the API:
  * dotnet run
  *
