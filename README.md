# Film Rating Service (IMDb Clone)

This project is a web application developed using ASP.NET Core, designed to function as a movie rating and review service, inspired by IMDb. It allows users to browse popular movies, search for specific titles, view detailed information, and submit their own ratings and reviews. The application also includes a secure administration area for managing users and their submitted reviews.

This project was built to satisfy the final exam requirements for the "ASP.NET MVC" course.

## Features

### User-Facing Features
* **Homepage:** Displays a featured movie and a paginated list of popular movies fetched from The Movie Database (TMDB) API.
* **Movie Search:** Users can search for movies by title, with paginated search results.
* **Movie Details Page:** Displays detailed information about a specific movie, including its overview, poster, and average vote from TMDB.
* **User Reviews:**
    * Logged-in users can submit a rating (1-10) and a text review for any movie.
    * The movie details page displays all submitted user reviews.
    * The review list can be sorted by **Date (Newest/Oldest)** or by **Rating (Highest/Lowest)**.
* **Asynchronous Rating Summary:** The movie details page uses an AJAX call to asynchronously load and display a summary of user ratings (average rating and total number of reviews).
* **User Authentication:**
    * Users can register for a new account. Email confirmation is required.
    * Registered users can log in and out.
* **Custom Error Pages:** User-friendly pages for `404 Not Found`, `401 Unauthorized`, and `403 Forbidden` errors.
* **Responsive Design:** Styled with a custom dark theme based on Bootstrap for a responsive user experience.

### Administrator Features
* **Secure Admin Area:** A dedicated section of the site accessible only to users with the "Admin" role.
* **Admin Dashboard:** A central hub providing an overview of site statistics.
    * Displays total number of registered users.
    * Displays total number of submitted reviews.
* **User Management:**
    * Admins can view a list of all registered users in the system.
    * Admins can delete user profiles (with a safeguard to prevent self-deletion).
* **Review Management:**
    * Admins can view a list of all user-submitted reviews.
    * Admins can delete any review.

## Technologies Used

* **Backend:** ASP.NET Core 8.0, C#
* **Database:** Entity Framework Core 8 with Microsoft SQL Server (LocalDB)
* **Architecture:**
    * MVC (Model-View-Controller) design pattern
    * Service Layer pattern for business logic (`MovieService`, `ReviewService`)
    * Repository-like interaction via `DbContext`
    * Dependency Injection used throughout
* **Authentication & Authorization:** ASP.NET Core Identity with a custom `ApplicationUser` class and Roles.
* **Frontend:**
    * Razor Views
    * Bootstrap 5
    * jQuery & jQuery Unobtrusive Validation
    * AJAX for dynamic content loading
* **Testing:** NUnit and Moq for unit testing the service and controller layers.

## Setup and Installation

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
* [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) with the "ASP.NET and web development" workload
* SQL Server LocalDB (typically installed with Visual Studio)

### Configuration Steps
1.  Clone or download this repository.
2.  Open the solution file (`FilmRatingService.sln`) in Visual Studio.
3.  **Configure `appsettings.json`:**
    * Open the `appsettings.json` file in the `FilmRatingService` project.
    * **Connection String:** Ensure the `ConnectionStrings.ApplicationDbContextConnection` value is correct for your local SQL Server instance. The default is set up for LocalDB.
        ```
        "ConnectionStrings": {
          "ApplicationDbContextConnection": "Server=(localdb)\\mssqllocaldb;Database=FilmRatingService;Trusted_Connection=True;MultipleActiveResultSets=true"
        }
        ```
    * **TMDB API Key:** You need an API key from [The Movie Database (TMDB)](https://www.themoviedb.org/signup). Add your key to `appsettings.json`:
        ```
        "TMDBApiKey": "YOUR_TMDB_API_KEY_HERE"
        ```
    * **Admin Account:** The application will seed a default admin user on its first run. You must set a strong password for this user in `appsettings.json`. The password must meet complexity requirements (e.g., have an uppercase letter, a digit, and a non-alphanumeric character).
        ```
        "DefaultAdminCredentials": {
          "Name": "Administrator",
          "Email": "admin@filmratingservice.com",
          "Password": "YOUR_STRONG_ADMIN_PASSWORD_HERE!1"
        }
        ```

### Database Setup
1.  Open the **Package Manager Console** in Visual Studio (Tools > NuGet Package Manager > Package Manager Console).
2.  Ensure the "Default project" dropdown is set to `FilmRatingService`.
3.  Run the following command to apply the Entity Framework Core migrations and create the database schema:
    ```powershell
    Update-Database
    ```

### Running the Application
1.  Press `F5` or the "Start Debugging" button in Visual Studio to build and run the project.
2.  The first time the application runs, the `DbInitializer` will execute, creating the "Admin" and "User" roles and seeding the admin account specified in your `appsettings.json`.
3.  You can then register new users or log in as the default admin to access the admin area.

## Testing
The solution includes a separate unit test project (`FilmRatingService.Tests`) built with NUnit and Moq.
* **MovieServiceTests:** Contains tests for the service that interacts with the TMDB API, using mocked `HttpClient` responses.
* **ReviewServiceTests:** Contains tests for the service that manages user reviews, using an in-memory EF Core database for test isolation.
* **HomeControllerTests:** Contains basic unit tests for the `HomeController`, demonstrating how to test controller actions with mocked service dependencies.

To run the tests, open the **Test Explorer** in Visual Studio (Test > Test Explorer) and click "Run All Tests".
