# BookStore

BookStore is an ASP.NET Core MVC web application that simulates a real-world online bookstore. Users can browse books, manage a shopping cart, place orders, and complete payments through Paymob. The project was built to practice backend development concepts such as layered architecture, authentication, authorization, Entity Framework Core, and payment gateway integration.

## Features

- User registration and authentication using ASP.NET Core Identity
- Role-based authorization (Admin & Customer)
- Browse books by category, publisher, and author
- Shopping cart and checkout workflow
- Order management
- Paymob payment gateway integration
- Book, Category, Author, and Publisher management
- Responsive user interface

## Technologies

- ASP.NET Core MVC
- C#
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- LINQ
- Bootstrap
- HTML, CSS, JavaScript
- Paymob API

## Architecture

The application separates business logic from controllers through a Service Layer.

```
Controllers
    │
    ▼
Services
    │
    ▼
Entity Framework Core
    │
    ▼
SQL Server
```

## Database

The database schema is illustrated below.

![Database Diagram](images/ERD.png)

## Project Structure

```
BookStore
├── Controllers
├── Data
├── Models
├── Services
├── ViewModels
├── Views
└── wwwroot
```

## Getting Started

1. Clone the repository.

```bash
git clone https://github.com/your-username/BookStore.git
```

2. Update the connection string and Paymob configuration in `appsettings.json`.

3. Apply the database migrations.

```bash
dotnet ef database update
```

4. Run the application.

```bash
dotnet run
```

## Screenshots

### Home Page

_Add screenshot_

### Book Details

_Add screenshot_

### Shopping Cart

_Add screenshot_

### Checkout

_Add screenshot_

## Future Improvements

- Wishlist
- Product reviews and ratings
- Advanced search and filtering
- Email notifications
- Discount coupons

## Author

**Khaled Ahmed**

Computer Science Student | ASP.NET Core Backend Developer
